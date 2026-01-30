namespace BankMore.Transferencia.Tests.Entities;

using Transferencia.Domain.Entities;
using Shared.Exceptions;
using Shared.Enums;
using FluentAssertions;
using Xunit;

public class TransferenciaTests
{
    [Fact]
    public void Transferencia_Create_WithValidData_ShouldCreateSuccessfully()
    {
        var sourceAccount = Guid.NewGuid();
        var destinationAccount = Guid.NewGuid();
        const decimal amount = 100m;

        var transfer = Transferencia.Create(sourceAccount, destinationAccount, amount);

        transfer.Id.Should().NotBeEmpty();
        transfer.IdContaCorrenteOrigem.Should().Be(sourceAccount);
        transfer.IdContaCorrenteDestino.Should().Be(destinationAccount);
        transfer.Valor.Should().Be(amount);
        transfer.DataMovimento.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-100.50)]
    public void Transferencia_Create_WithInvalidAmount_ShouldThrowException(decimal invalidAmount)
    {
        var sourceAccount = Guid.NewGuid();
        var destinationAccount = Guid.NewGuid();

        Action act = () => Transferencia.Create(sourceAccount, destinationAccount, invalidAmount);

        act.Should().Throw<DomainException>()
            .WithMessage("Valor deve ser positivo");
    }

    [Fact]
    public void Transferencia_Create_WithSameAccount_ShouldThrowException()
    {
        var accountId = Guid.NewGuid();

        Action act = () => Transferencia.Create(accountId, accountId, 100);

        act.Should().Throw<DomainException>()
            .WithMessage("Não é possível transferir para a mesma conta");
    }

    [Fact]
    public void Transferencia_Reconstruct_ShouldReturnCompleteTransfer()
    {
        var id = Guid.NewGuid();
        var sourceAccount = Guid.NewGuid();
        var destinationAccount = Guid.NewGuid();
        var transactionDate = DateTime.Now;
        const decimal amount = 150m;

        var transfer = Transferencia.Reconstruct(
            id,
            sourceAccount,
            destinationAccount,
            transactionDate,
            amount
        );

        transfer.Id.Should().Be(id);
        transfer.IdContaCorrenteOrigem.Should().Be(sourceAccount);
        transfer.IdContaCorrenteDestino.Should().Be(destinationAccount);
        transfer.DataMovimento.Should().Be(transactionDate);
        transfer.Valor.Should().Be(amount);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(10)]
    [InlineData(100.50)]
    [InlineData(1000)]
    [InlineData(9999.99)]
    public void Transferencia_Create_WithPositiveAmounts_ShouldCreateSuccessfully(decimal validAmount)
    {
        var sourceAccount = Guid.NewGuid();
        var destinationAccount = Guid.NewGuid();

        var transfer = Transferencia.Create(sourceAccount, destinationAccount, validAmount);

        transfer.Valor.Should().Be(validAmount);
    }
}
