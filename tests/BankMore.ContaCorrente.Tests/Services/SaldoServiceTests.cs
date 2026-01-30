namespace BankMore.ContaCorrente.Tests.Services;

using ContaCorrente.Domain.Services;
using ContaCorrente.Domain.Entities;
using Shared.Enums;
using FluentAssertions;
using Xunit;

public class SaldoServiceTests
{
    [Fact]
    public void CalculateBalance_WithNoTransactions_ShouldReturnZero()
    {
        var transactions = new List<Movimento>();

        var balance = SaldoService.CalculateBalance(transactions);

        balance.Should().Be(0);
    }

    [Fact]
    public void CalculateBalance_WithCreditsAndDebits_ShouldReturnDifference()
    {
        var accountId = Guid.NewGuid();
        var transactions = new List<Movimento>
        {
            Movimento.Create(accountId, TipoMovimento.Credito, 1000),
            Movimento.Create(accountId, TipoMovimento.Debito, 100),
            Movimento.Create(accountId, TipoMovimento.Debito, 2)
        };

        var balance = SaldoService.CalculateBalance(transactions);

        balance.Should().Be(898);
    }
}
