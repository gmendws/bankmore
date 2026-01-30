namespace BankMore.Tarifas.Worker.Models;

public class FeeProcessedMessage
{
    public Guid IdContaCorrente { get; set; }
    public decimal Valor { get; set; }
    public string IdempotenciaKey { get; set; } = string.Empty;
}
