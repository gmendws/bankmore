namespace BankMore.Tarifas.Worker.Database;

using Microsoft.Data.Sqlite;

public class DatabaseInitializer(string connectionString)
{
    public void Initialize()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        const string createTablesSql = @"
            CREATE TABLE IF NOT EXISTS tarifa (
                idtarifa TEXT PRIMARY KEY,
                idcontacorrente TEXT NOT NULL,
                datamovimento TEXT NOT NULL,
                valor REAL NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_tarifa_conta 
                ON tarifa(idcontacorrente);
        ";

        using var command = connection.CreateCommand();
        command.CommandText = createTablesSql;
        command.ExecuteNonQuery();
    }
}
