namespace BankMore.Transferencia.Infrastructure.Services;

using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Shared.Results;
using Shared.Enums;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class ContaCorrenteService(HttpClient httpClient, IConfiguration configuration) : IContaCorrenteService
{
    private readonly string _baseUrl = configuration["ContaCorrenteApi:BaseUrl"]
                                       ?? throw new InvalidOperationException("ContaCorrenteApi:BaseUrl not configured");

    public async Task<Result> DebitAsync(Guid accountId, string idempotencyKey, decimal amount, string token)
    {
        var request = new
        {
            idempotenciaKey = idempotencyKey,
            idContaCorrente = accountId,
            numeroConta = (int?)null,
            tipoMovimento = 1,
            valor = amount
        };

        return await CallTransactionApi(request, token);
    }

    public async Task<Result> CreditAsync(int accountNumber, string idempotencyKey, decimal amount, string token)
    {
        var request = new
        {
            idempotenciaKey = idempotencyKey,
            idContaCorrente = (Guid?)null,
            numeroConta = accountNumber,
            tipoMovimento = 0,
            valor = amount
        };

        return await CallTransactionApi(request, token);
    }

    public async Task<Result> CreditByIdAsync(Guid accountId, string idempotencyKey, decimal amount, string token)
    {
        var request = new
        {
            idempotenciaKey = idempotencyKey,
            idContaCorrente = accountId,
            numeroConta = (int?)null,
            tipoMovimento = 0,
            valor = amount
        };

        return await CallTransactionApi(request, token);
    }

    private async Task<Result> CallTransactionApi(object request, string token)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/conta-corrente/movimentacao")
            {
                Content = content
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                var tipoFalha = Enum.TryParse<TipoFalha>(errorResponse?.Tipo, out var tipo)
                    ? tipo 
                    : TipoFalha.InvalidValue;

                return Result.Falha(errorResponse?.Mensagem ?? "Error calling Account API", tipoFalha);
            }
            catch
            {
                return Result.Falha($"HTTP Error {response.StatusCode}: {errorContent}", TipoFalha.InvalidValue);
            }
        }
        catch (Exception ex)
        {
            return Result.Falha($"Error communicating with Account API: {ex.Message}", TipoFalha.InvalidValue);
        }
    }

    private class ErrorResponse
    {
        public string? Mensagem { get; set; }
        public string? Tipo { get; set; }
    }
}
