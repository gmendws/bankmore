namespace BankMore.ContaCorrente.Infrastructure.Repositories;

using Domain.Repositories;
using Domain.Entities;
using Database;
using Dapper;
using Shared.ValueObjects;

public class ContaCorrenteRepository(IDbConnectionFactory connectionFactory) : IContaCorrenteRepository
{
    public async Task<ContaCorrente?> GetByIdAsync(Guid id)
    {
        using var connection = connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT idcontacorrente, numero, cpf, nome, ativo, senha, salt
            FROM contacorrente
            WHERE idcontacorrente = @Id";

        var result = await connection.QuerySingleOrDefaultAsync<AccountDto>(sql, new { Id = id.ToString() });

        return result == null ? null : MapToEntity(result);
    }

    public async Task<ContaCorrente?> GetByNumberAsync(int number)
    {
        using var connection = connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT idcontacorrente, numero, cpf, nome, ativo, senha, salt
            FROM contacorrente
            WHERE numero = @Number";

        var result = await connection.QuerySingleOrDefaultAsync<AccountDto>(sql, new { Number = number });

        return result == null ? null : MapToEntity(result);
    }

    public async Task<ContaCorrente?> GetByCpfAsync(Cpf cpf)
    {
        using var connection = connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT idcontacorrente, numero, cpf, nome, ativo, senha, salt
            FROM contacorrente
            WHERE cpf = @Cpf";

        var result = await connection.QuerySingleOrDefaultAsync<AccountDto>(sql, new { Cpf = cpf.Numero });

        return result == null ? null : MapToEntity(result);
    }

    public async Task<int> GetNextAccountNumberAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        
        const string sql = "SELECT COALESCE(MAX(numero), 0) + 1 FROM contacorrente";
        
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task AddAsync(ContaCorrente account)
    {
        using var connection = connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO contacorrente (idcontacorrente, numero, cpf, nome, ativo, senha, salt)
            VALUES (@Id, @Number, @Cpf, @Name, @Active, @PasswordHash, @PasswordSalt)";

        await connection.ExecuteAsync(sql, new
        {
            Id = account.Id.ToString(),
            Number = account.Numero,
            Cpf = account.CpfTitular.Numero,
            Name = account.Nome,
            Active = account.Ativo ? 1 : 0,
            PasswordHash = account.Senha.Hash,
            PasswordSalt = account.Senha.Salt
        });
    }

    public async Task UpdateAsync(ContaCorrente account)
    {
        using var connection = connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE contacorrente
            SET ativo = @Active
            WHERE idcontacorrente = @Id";

        await connection.ExecuteAsync(sql, new
        {
            Id = account.Id.ToString(),
            Active = account.Ativo ? 1 : 0
        });
    }

    private static ContaCorrente MapToEntity(AccountDto dto)
    {
        return ContaCorrente.Reconstruct(
            Guid.Parse(dto.Idcontacorrente),
            Convert.ToInt32(dto.Numero),
            new Cpf(dto.Cpf),
            dto.Nome,
            Convert.ToInt32(dto.Ativo) == 1,
            dto.Senha,
            dto.Salt
        );
    }

    private class AccountDto
    {
        public string Idcontacorrente { get; set; } = string.Empty;
        public long Numero { get; set; }
        public string Cpf { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public long Ativo { get; set; }
        public string Senha { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
    }
}
