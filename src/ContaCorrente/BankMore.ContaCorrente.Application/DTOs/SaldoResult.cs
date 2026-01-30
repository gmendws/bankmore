namespace BankMore.ContaCorrente.Application.DTOs;

public record BalanceResult(
    int AccountNumber,
    string AccountHolderName,
    DateTime QueryDateTime,
    decimal Balance
);
