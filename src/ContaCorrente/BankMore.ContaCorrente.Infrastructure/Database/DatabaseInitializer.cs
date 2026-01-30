namespace BankMore.ContaCorrente.Infrastructure.Database;

using Microsoft.Data.Sqlite;
using System.Data;

public class DatabaseInitializer(string connectionString)
{
    public void Initialize()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var createTablesSql = @"
            CREATE TABLE IF NOT EXISTS contacorrente (
                idcontacorrente TEXT PRIMARY KEY,
                numero INTEGER NOT NULL UNIQUE,
                cpf TEXT NOT NULL UNIQUE,
                nome TEXT NOT NULL,
                ativo INTEGER NOT NULL DEFAULT 1,
                senha TEXT NOT NULL,
                salt TEXT NOT NULL,
                CHECK (ativo IN (0,1))
            );

            CREATE TABLE IF NOT EXISTS movimento (
                idmovimento TEXT PRIMARY KEY,
                idcontacorrente TEXT NOT NULL,
                datamovimento TEXT NOT NULL,
                tipomovimento TEXT NOT NULL,
                valor REAL NOT NULL,
                CHECK (tipomovimento IN ('C','D')),
                FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente)
            );

            CREATE TABLE IF NOT EXISTS idempotencia (
                chave_idempotencia TEXT PRIMARY KEY,
                requisicao TEXT,
                resultado TEXT
            );

            CREATE INDEX IF NOT EXISTS idx_movimento_conta 
                ON movimento(idcontacorrente);
            
            CREATE INDEX IF NOT EXISTS idx_contacorrente_cpf 
                ON contacorrente(cpf);
        ";

        using var command = connection.CreateCommand();
        command.CommandText = createTablesSql;
        command.ExecuteNonQuery();
    }
}
