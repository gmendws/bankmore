namespace BankMore.ContaCorrente.Domain.Entities;

using Shared.Exceptions;
using Shared.Enums;

public class Movimento
{
    public Guid Id { get; private set; }
    public Guid IdContaCorrente { get; private set; }
    public DateTime DataMovimento { get; private set; }
    public TipoMovimento TipoMovimento { get; private set; }
    public decimal Valor { get; private set; }

    private Movimento() { }

    public static Movimento Create(Guid accountId, TipoMovimento type, decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Valor deve ser positivo", TipoFalha.InvalidValue);

        return new Movimento
        {
            Id = Guid.NewGuid(),
            IdContaCorrente = accountId,
            DataMovimento = DateTime.Now,
            TipoMovimento = type,
            Valor = amount
        };
    }

    public static Movimento Reconstruct(
        Guid id,
        Guid accountId,
        DateTime transactionDate,
        TipoMovimento type,
        decimal amount)
    {
        return new Movimento
        {
            Id = id,
            IdContaCorrente = accountId,
            DataMovimento = transactionDate,
            TipoMovimento = type,
            Valor = amount
        };
    }
}
