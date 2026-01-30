namespace BankMore.Transferencia.Application.Commands;

using MediatR;

public record CreateTransferCommand(
    string IdempotencyKey,
    int DestinationAccountNumber,
    decimal Amount,
    Guid LoggedUserId,
    string Token
) : IRequest;
