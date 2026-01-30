namespace BankMore.Transferencia.Infrastructure.Database;

using Microsoft.Data.Sqlite;

public class DatabaseInitializer(string connectionString)
{
    public void Initialize()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        const string createTablesSql = @"
            CREATE TABLE IF NOT EXISTS transferencia (
                idtransferencia TEXT PRIMARY KEY,
                idcontacorrente_origem TEXT NOT NULL,
                idcontacorrente_destino TEXT NOT NULL,
                datamovimento TEXT NOT NULL,
                valor REAL NOT NULL
            );

            CREATE TABLE IF NOT EXISTS idempotencia (
                chave_idempotencia TEXT PRIMARY KEY,
                requisicao TEXT,
                resultado TEXT
            );

            CREATE INDEX IF NOT EXISTS idx_transferencia_origem 
                ON transferencia(idcontacorrente_origem);
        ";

        using var command = connection.CreateCommand();
        command.CommandText = createTablesSql;
        command.ExecuteNonQuery();
    }
}
