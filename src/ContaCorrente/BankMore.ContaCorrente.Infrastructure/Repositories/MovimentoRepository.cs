namespace BankMore.ContaCorrente.Infrastructure.Repositories;

using Domain.Repositories;
using Domain.Entities;
using Database;
using Dapper;
using Shared.Enums;

public class MovimentoRepository(IDbConnectionFactory connectionFactory) : IMovimentoRepository
{
    public async Task AddAsync(Movimento transaction)
    {
        using var connection = connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor)
            VALUES (@Id, @AccountId, @TransactionDate, @TransactionType, @Amount)";

        await connection.ExecuteAsync(sql, new
        {
            Id = transaction.Id.ToString(),
            AccountId = transaction.IdContaCorrente.ToString(),
            TransactionDate = transaction.DataMovimento.ToString("dd/MM/yyyy HH:mm:ss"),
            TransactionType = transaction.TipoMovimento == TipoMovimento.Credito ? "C" : "D",
            Amount = transaction.Valor
        });
    }

    public async Task<IEnumerable<Movimento>> GetByAccountAsync(Guid accountId)
    {
        using var connection = connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT idmovimento, idcontacorrente, datamovimento, tipomovimento, valor
            FROM movimento
            WHERE idcontacorrente = @AccountId
            ORDER BY datamovimento DESC";

        var results = await connection.QueryAsync<dynamic>(sql, new { AccountId = accountId.ToString() });

        return results.Select(r => Movimento.Reconstruct(Guid.Parse((string)r.idmovimento), Guid.Parse((string)r.idcontacorrente), DateTime.ParseExact((string)r.datamovimento, "dd/MM/yyyy HH:mm:ss", null), (string)r.tipomovimento == "C" ? TipoMovimento.Credito : TipoMovimento.Debito, (decimal)r.valor)).ToList();
    }
}
