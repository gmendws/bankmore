namespace BankMore.Transferencia.Infrastructure.Services;

using Application.Services;
using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class KafkaProducerService(
    IProducerAccessor producerAccessor,
    ILogger<KafkaProducerService> logger,
    IConfiguration configuration)
    : IKafkaProducerService
{
    private readonly IConfiguration _configuration = configuration;

    public async Task PublishTransferCompletedAsync(string idempotencyKey, Guid sourceAccountId)
    {
        try
        {
            var producer = producerAccessor.GetProducer("transferencias-producer");

            var message = new
            {
                IdempotenciaKey = idempotencyKey,
                IdContaCorrenteOrigem = sourceAccountId
            };

            await producer.ProduceAsync(
                "transferencias-realizadas",
                Guid.NewGuid().ToString(),
                message
            );

            logger.LogInformation(
                "Kafka message published - Transfer completed | IdempotencyKey: {IdempotencyKey}, Account: {AccountId}",
                idempotencyKey,
                sourceAccountId
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing Kafka message");
            throw;
        }
    }
}
