namespace BankMore.Transferencia.Application.Services;

public interface IKafkaProducerService
{
    Task PublishTransferCompletedAsync(string idempotencyKey, Guid sourceAccountId);
}
