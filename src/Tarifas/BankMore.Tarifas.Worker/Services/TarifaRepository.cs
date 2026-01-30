namespace BankMore.Tarifas.Worker.Services;

using Database;
using Dapper;

public interface ITarifaRepository
{
    Task AddAsync(Guid accountId, decimal amount);
}

public class TarifaRepository(IDbConnectionFactory connectionFactory) : ITarifaRepository
{
    public async Task AddAsync(Guid accountId, decimal amount)
    {
        using var connection = connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO tarifa (idtarifa, idcontacorrente, datamovimento, valor)
            VALUES (@Id, @AccountId, @TransactionDate, @Amount)";

        await connection.ExecuteAsync(sql, new
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = accountId.ToString(),
            TransactionDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            Amount = amount
        });
    }
}
