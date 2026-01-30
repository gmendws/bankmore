namespace BankMore.ContaCorrente.API.Models;

public class TarifacaoRealizadaMessage
{
    public Guid IdContaCorrente { get; set; }
    public decimal Valor { get; set; }
    public string IdempotenciaKey { get; set; } = string.Empty;
}
