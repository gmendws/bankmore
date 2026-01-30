namespace BankMore.Transferencia.Domain.Repositories;

using Entities;

public interface ITransferenciaRepository
{
    Task AddAsync(Transferencia transfer);
}
