namespace BankMore.Transferencia.Domain.Repositories;

using Shared.Results;

public interface IContaCorrenteService
{
    Task<Result> DebitAsync(Guid accountId, string idempotencyKey, decimal amount, string token);
    Task<Result> CreditAsync(int accountNumber, string idempotencyKey, decimal amount, string token);
    Task<Result> CreditByIdAsync(Guid accountId, string idempotencyKey, decimal amount, string token);
}
