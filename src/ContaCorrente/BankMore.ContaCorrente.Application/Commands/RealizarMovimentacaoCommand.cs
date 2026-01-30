namespace BankMore.ContaCorrente.Application.Commands;

using MediatR;
using Shared.Enums;

public record CreateTransactionCommand(
    string IdempotencyKey,
    Guid? AccountId,
    int? AccountNumber,
    TipoMovimento TransactionType,
    decimal Amount,
    Guid? LoggedUserId
) : IRequest;
