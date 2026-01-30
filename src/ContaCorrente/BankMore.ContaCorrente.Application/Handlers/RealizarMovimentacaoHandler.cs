namespace BankMore.ContaCorrente.Application.Handlers;

using Commands;
using Domain.Repositories;
using Domain.Entities;
using MediatR;
using Shared.Exceptions;
using Shared.Enums;
using System.Text.Json;

public class CreateTransactionHandler(
    IContaCorrenteRepository accountRepository,
    IMovimentoRepository transactionRepository,
    IIdempotenciaRepository idempotencyRepository)
    : IRequestHandler<CreateTransactionCommand>
{
    public async Task Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var existingIdempotency = await idempotencyRepository
            .GetByKeyAsync(request.IdempotencyKey);
        
        if (existingIdempotency != null)
            return;

        var accountId = request.AccountId ?? request.LoggedUserId!.Value;

        Domain.Entities.ContaCorrente? account = null;
        
        if (request.AccountNumber.HasValue)
        {
            account = await accountRepository.GetByNumberAsync(request.AccountNumber.Value);
            if (account == null)
                throw new DomainException("Conta não encontrada", TipoFalha.InvalidAccount);
            
            accountId = account.Id;
        }
        else
        {
            account = await accountRepository.GetByIdAsync(accountId);
            if (account == null)
                throw new DomainException("Conta não encontrada", TipoFalha.InvalidAccount);
        }

        account.ValidateIsActive();

        if (request.TransactionType == TipoMovimento.Debito)
        {
            if (account.Id != request.LoggedUserId)
                throw new DomainException(
                    "Apenas o titular pode debitar da própria conta", 
                    TipoFalha.InvalidType
                );
        }

        var transaction = Movimento.Create(accountId, request.TransactionType, request.Amount);

        await transactionRepository.AddAsync(transaction);

        var idempotency = Idempotencia.Create(
            request.IdempotencyKey,
            JsonSerializer.Serialize(request),
            "SUCCESS"
        );
        await idempotencyRepository.AddAsync(idempotency);
    }
}
