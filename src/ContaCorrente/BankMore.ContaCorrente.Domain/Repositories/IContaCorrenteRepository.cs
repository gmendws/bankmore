namespace BankMore.ContaCorrente.Domain.Repositories;

using Entities;
using Shared.ValueObjects;

public interface IContaCorrenteRepository
{
    Task<ContaCorrente?> GetByIdAsync(Guid id);
    Task<ContaCorrente?> GetByNumberAsync(int number);
    Task<ContaCorrente?> GetByCpfAsync(Cpf cpf);
    Task<int> GetNextAccountNumberAsync();
    Task AddAsync(ContaCorrente account);
    Task UpdateAsync(ContaCorrente account);
}
