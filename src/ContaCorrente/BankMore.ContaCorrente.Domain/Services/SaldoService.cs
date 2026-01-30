namespace BankMore.ContaCorrente.Domain.Services;

using Entities;
using Shared.Enums;

public class SaldoService
{
    public static decimal CalculateBalance(IEnumerable<Movimento> transactions)
    {
        var movimentos = transactions as Movimento[] ?? transactions.ToArray();
        var credits = movimentos
            .Where(m => m.TipoMovimento == TipoMovimento.Credito)
            .Sum(m => m.Valor);

        var debits = movimentos
            .Where(m => m.TipoMovimento == TipoMovimento.Debito)
            .Sum(m => m.Valor);

        return credits - debits;
    }
}
