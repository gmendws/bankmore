namespace BankMore.ContaCorrente.API.Handlers;

using KafkaFlow;
using MediatR;
using Models;
using Application.Commands;
using Shared.Enums;
using Microsoft.Extensions.Logging;

public class TarifacaoRealizadaHandler(
    IMediator mediator,
    ILogger<TarifacaoRealizadaHandler> logger)
    : IMessageHandler<TarifacaoRealizadaMessage>
{
    public async Task Handle(IMessageContext context, TarifacaoRealizadaMessage message)
    {
        try
        {
            logger.LogInformation(
                "Processando tarifação - Conta: {ContaId}, Valor: {Valor}, Idempotencia: {Idempotencia}",
                message.IdContaCorrente,
                message.Valor,
                message.IdempotenciaKey
            );

            var command = new CreateTransactionCommand(
                message.IdempotenciaKey,
                message.IdContaCorrente,
                null,
                TipoMovimento.Debito,
                message.Valor,
                message.IdContaCorrente
            );

            await mediator.Send(command);

            logger.LogInformation(
                "Tarifa debitada com sucesso - Conta: {ContaId}",
                message.IdContaCorrente
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao processar tarifação - Conta: {ContaId}",
                message.IdContaCorrente
            );
            throw;
        }
    }
}
