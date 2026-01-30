namespace BankMore.Tarifas.Worker.Handlers;

using KafkaFlow;
using KafkaFlow.Producers;
using Models;
using Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class TransferCompletedHandler(
    ITarifaRepository feeRepository,
    IProducerAccessor producerAccessor,
    IConfiguration configuration,
    ILogger<TransferCompletedHandler> logger)
    : IMessageHandler<TransferCompletedMessage>
{
    public async Task Handle(IMessageContext context, TransferCompletedMessage message)
    {
        try
        {
            logger.LogInformation(
                "Processing transfer - Account: {AccountId}, IdempotencyKey: {IdempotencyKey}",
                message.IdContaCorrenteOrigem,
                message.IdempotenciaKey
            );

            var feeAmount = decimal.Parse(configuration["Tarifa:ValorTransferencia"] ?? "2.00");

            await feeRepository.AddAsync(message.IdContaCorrenteOrigem, feeAmount);

            logger.LogInformation(
                "Fee registered - Account: {AccountId}, Amount: {Amount}",
                message.IdContaCorrenteOrigem,
                feeAmount
            );

            var producer = producerAccessor.GetProducer("tarifacoes-producer");
            
            var feeMessage = new FeeProcessedMessage
            {
                IdContaCorrente = message.IdContaCorrenteOrigem,
                Valor = feeAmount,
                IdempotenciaKey = $"{message.IdempotenciaKey}-tarifa"
            };

            await producer.ProduceAsync(
                configuration["Kafka:TopicTarifacoesRealizadas"],
                Guid.NewGuid().ToString(),
                feeMessage
            );

            logger.LogInformation(
                "Fee message published - Account: {AccountId}",
                message.IdContaCorrenteOrigem
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing fee for account {AccountId}", message.IdContaCorrenteOrigem);
            throw;
        }
    }
}
