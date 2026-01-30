namespace BankMore.Transferencia.Infrastructure.Repositories;

using Domain.Repositories;
using Domain.Entities;
using Database;
using Dapper;

public class TransferenciaRepository(IDbConnectionFactory connectionFactory) : ITransferenciaRepository
{
    public async Task AddAsync(Transferencia transfer)
    {
        using var connection = connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO transferencia (idtransferencia, idcontacorrente_origem, idcontacorrente_destino, datamovimento, valor)
            VALUES (@Id, @SourceAccountId, @DestinationAccountId, @TransactionDate, @Amount)";

        await connection.ExecuteAsync(sql, new
        {
            Id = transfer.Id.ToString(),
            SourceAccountId = transfer.IdContaCorrenteOrigem.ToString(),
            DestinationAccountId = transfer.IdContaCorrenteDestino.ToString(),
            TransactionDate = transfer.DataMovimento.ToString("dd/MM/yyyy HH:mm:ss"),
            Amount = transfer.Valor
        });
    }
}
