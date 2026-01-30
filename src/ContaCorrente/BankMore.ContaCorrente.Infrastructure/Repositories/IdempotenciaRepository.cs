namespace BankMore.ContaCorrente.Infrastructure.Repositories;

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
            result.chave_idempotencia,
            result.requisicao ?? string.Empty,
            result.resultado ?? string.Empty
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
        public string chave_idempotencia { get; set; } = string.Empty;
        public string? requisicao { get; set; }
        public string? resultado { get; set; }
    }
}
