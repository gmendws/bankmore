namespace BankMore.Transferencia.Application.Handlers;

using Commands;
using Domain.Repositories;
using Domain.Entities;
using Services;
using MediatR;
using Shared.Exceptions;
using Shared.Enums;
using System.Text.Json;

public class CreateTransferHandler(
    ITransferenciaRepository transferRepository,
    IIdempotenciaRepository idempotencyRepository,
    IContaCorrenteService accountService,
    IKafkaProducerService kafkaProducerService)
    : IRequestHandler<CreateTransferCommand>
{
    public async Task Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        var existingIdempotency = await idempotencyRepository
            .GetByKeyAsync(request.IdempotencyKey);
        
        if (existingIdempotency != null)
            return;

        var debitIdempotencyKey = $"{request.IdempotencyKey}-debit";
        var creditIdempotencyKey = $"{request.IdempotencyKey}-credit";

        try
        {
            var debitResult = await accountService.DebitAsync(
                request.LoggedUserId,
                debitIdempotencyKey,
                request.Amount,
                request.Token
            );

            if (!debitResult.Sucesso)
            {
                throw new DomainException(
                    debitResult.Mensagem ?? "Error processing debit",
                    debitResult.TipoFalha ?? TipoFalha.InvalidValue
                );
            }

            var creditResult = await accountService.CreditAsync(
                request.DestinationAccountNumber,
                creditIdempotencyKey,
                request.Amount,
                request.Token
            );

            if (!creditResult.Sucesso)
            {
                var chargebackIdempotencyKey = $"{request.IdempotencyKey}-chargeback";
                
                await accountService.CreditByIdAsync(
                    request.LoggedUserId,
                    chargebackIdempotencyKey,
                    request.Amount,
                    request.Token
                );

                throw new DomainException(
                    creditResult.Mensagem ?? "Error processing credit. Debit reversed.",
                    creditResult.TipoFalha ?? TipoFalha.InvalidValue
                );
            }

            var transfer = Transferencia.Create(
                request.LoggedUserId,
                Guid.NewGuid(),
                request.Amount
            );

            await transferRepository.AddAsync(transfer);

            var idempotency = Idempotencia.Create(
                request.IdempotencyKey,
                JsonSerializer.Serialize(request),
                "SUCCESS"
            );
            await idempotencyRepository.AddAsync(idempotency);

            await kafkaProducerService.PublishTransferCompletedAsync(
                request.IdempotencyKey,
                request.LoggedUserId
            );
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DomainException(
                $"Error processing transfer: {ex.Message}",
                TipoFalha.InvalidValue
            );
        }
    }
}
