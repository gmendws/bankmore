namespace BankMore.ContaCorrente.Domain.Repositories;

using Entities;

public interface IMovimentoRepository
{
    Task AddAsync(Movimento transaction);
    Task<IEnumerable<Movimento>> GetByAccountAsync(Guid accountId);
}
