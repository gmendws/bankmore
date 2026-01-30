namespace BankMore.Tarifas.Worker.Models;

public class TransferCompletedMessage
{
    public string IdempotenciaKey { get; set; } = string.Empty;
    public Guid IdContaCorrenteOrigem { get; set; }
}
