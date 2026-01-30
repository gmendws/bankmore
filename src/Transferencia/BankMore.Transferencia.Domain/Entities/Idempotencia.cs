namespace BankMore.Transferencia.Domain.Entities;

public class Idempotencia
{
    public string ChaveIdempotencia { get; private set; }
    public string Requisicao { get; private set; }
    public string Resultado { get; private set; }

    private Idempotencia() { }

    public static Idempotencia Create(string key, string request, string result)
    {
        return new Idempotencia
        {
            ChaveIdempotencia = key,
            Requisicao = request,
            Resultado = result
        };
    }

    public static Idempotencia Reconstruct(string key, string request, string result)
    {
        return new Idempotencia
        {
            ChaveIdempotencia = key,
            Requisicao = request,
            Resultado = result
        };
    }
}
