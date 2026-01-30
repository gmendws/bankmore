namespace BankMore.ContaCorrente.Domain.Repositories;

using Entities;

public interface IIdempotenciaRepository
{
    Task<Idempotencia?> GetByKeyAsync(string key);
    Task AddAsync(Idempotencia idempotency);
}
