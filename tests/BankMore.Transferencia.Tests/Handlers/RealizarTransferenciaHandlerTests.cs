namespace BankMore.Transferencia.Tests.Handlers;

using Transferencia.Application.Commands;
using Transferencia.Application.Handlers;
using Transferencia.Application.Services;
using Transferencia.Domain.Repositories;
using Moq;
using FluentAssertions;
using Xunit;
using Shared.Results;
using Shared.Exceptions;
using Shared.Enums;

public class CreateTransferHandlerTests
{
    private readonly Mock<ITransferenciaRepository> _transferRepositoryMock;
    private readonly Mock<IIdempotenciaRepository> _idempotencyRepositoryMock;
    private readonly Mock<IContaCorrenteService> _accountServiceMock;
    private readonly Mock<IKafkaProducerService> _kafkaProducerServiceMock;
    private readonly CreateTransferHandler _handler;

    public CreateTransferHandlerTests()
    {
        _transferRepositoryMock = new Mock<ITransferenciaRepository>();
        _idempotencyRepositoryMock = new Mock<IIdempotenciaRepository>();
        _accountServiceMock = new Mock<IContaCorrenteService>();
        _kafkaProducerServiceMock = new Mock<IKafkaProducerService>();

        _handler = new CreateTransferHandler(
            _transferRepositoryMock.Object,
            _idempotencyRepositoryMock.Object,
            _accountServiceMock.Object,
            _kafkaProducerServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateTransfer()
    {
        var command = new CreateTransferCommand(
            "idempotency-001",
            2,
            100,
            Guid.NewGuid(),
            "token-jwt"
        );

        _idempotencyRepositoryMock
            .Setup(r => r.GetByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync((Transferencia.Domain.Entities.Idempotencia?)null);

        _accountServiceMock
            .Setup(s => s.DebitAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Ok());

        _accountServiceMock
            .Setup(s => s.CreditAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Ok());

        await _handler.Handle(command, CancellationToken.None);

        _transferRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Transferencia.Domain.Entities.Transferencia>()),
            Times.Once
        );

        _idempotencyRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Transferencia.Domain.Entities.Idempotencia>()),
            Times.Once
        );

        _kafkaProducerServiceMock.Verify(
            k => k.PublishTransferCompletedAsync(It.IsAny<string>(), It.IsAny<Guid>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithExistingIdempotency_ShouldNotExecuteAgain()
    {
        var command = new CreateTransferCommand(
            "idempotency-001",
            2,
            100,
            Guid.NewGuid(),
            "token-jwt"
        );

        var existingIdempotency = Transferencia.Domain.Entities.Idempotencia.Create(
            "idempotency-001",
            "request",
            "SUCCESS"
        );

        _idempotencyRepositoryMock
            .Setup(r => r.GetByKeyAsync("idempotency-001"))
            .ReturnsAsync(existingIdempotency);

        await _handler.Handle(command, CancellationToken.None);

        _accountServiceMock.Verify(
            s => s.DebitAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()),
            Times.Never
        );

        _transferRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Transferencia.Domain.Entities.Transferencia>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WithDebitFailure_ShouldThrowException()
    {
        var command = new CreateTransferCommand(
            "idempotency-001",
            2,
            100,
            Guid.NewGuid(),
            "token-jwt"
        );

        _idempotencyRepositoryMock
            .Setup(r => r.GetByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync((Transferencia.Domain.Entities.Idempotencia?)null);

        _accountServiceMock
            .Setup(s => s.DebitAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Falha("Saldo insuficiente", TipoFalha.InvalidValue));

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Saldo insuficiente");

        _accountServiceMock.Verify(
            s => s.CreditAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WithCreditFailure_ShouldPerformChargeback()
    {
        var command = new CreateTransferCommand(
            "idempotency-001",
            2,
            100,
            Guid.NewGuid(),
            "token-jwt"
        );

        _idempotencyRepositoryMock
            .Setup(r => r.GetByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync((Transferencia.Domain.Entities.Idempotencia?)null);

        _accountServiceMock
            .Setup(s => s.DebitAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Ok());

        _accountServiceMock
            .Setup(s => s.CreditAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Falha("Conta destino inválida", TipoFalha.InvalidAccount));

        _accountServiceMock
            .Setup(s => s.CreditByIdAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Ok());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();

        _accountServiceMock.Verify(
            s => s.CreditByIdAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()),
            Times.Once,
            "Chargeback should be executed when credit fails"
        );

        _transferRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Transferencia.Domain.Entities.Transferencia>()),
            Times.Never
        );
    }
}
