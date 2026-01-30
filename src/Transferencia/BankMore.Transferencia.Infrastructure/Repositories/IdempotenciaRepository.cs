namespace BankMore.Transferencia.Infrastructure.Repositories;

using Domain.Repositories;
using Domain.Entities;
using Database;
using Dapper;

public class IdempotenciaRepository(IDbConnectionFactory connectionFactory) : IIdempotenciaRepository
{
    public async Task<Idempotencia?> GetByKeyAsync(string key)
    {
        using var connection = connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT chave_idempotencia, requisicao, resultado
            FROM idempotencia
            WHERE chave_idempotencia = @Key";

        var result = await connection.QuerySingleOrDefaultAsync<IdempotencyDto>(sql, new { Key = key });

        if (result == null)
            return null;

        return Idempotencia.Reconstruct(
            result.ChaveIdempotencia,
            result.Requisicao ?? string.Empty,
            result.Resultado ?? string.Empty
        );
    }

    public async Task AddAsync(Idempotencia idempotency)
    {
        using var connection = connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado)
            VALUES (@Key, @Request, @Result)";

        await connection.ExecuteAsync(sql, new
        {
            Key = idempotency.ChaveIdempotencia,
            Request = idempotency.Requisicao,
            Result = idempotency.Resultado
        });
    }

    private class IdempotencyDto
    {
        public string ChaveIdempotencia { get; set; } = string.Empty;
        public string? Requisicao { get; set; }
        public string? Resultado { get; set; }
    }
}
