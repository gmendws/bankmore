namespace BankMore.Transferencia.Domain.Entities;

using Shared.Exceptions;
using Shared.Enums;

public class Transferencia
{
    public Guid Id { get; private set; }
    public Guid IdContaCorrenteOrigem { get; private set; }
    public Guid IdContaCorrenteDestino { get; private set; }
    public DateTime DataMovimento { get; private set; }
    public decimal Valor { get; private set; }

    private Transferencia() { }

    public static Transferencia Create(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Valor deve ser positivo", TipoFalha.InvalidValue);

        if (sourceAccountId == destinationAccountId)
            throw new DomainException("Não é possível transferir para a mesma conta", TipoFalha.InvalidAccount);

        return new Transferencia
        {
            Id = Guid.NewGuid(),
            IdContaCorrenteOrigem = sourceAccountId,
            IdContaCorrenteDestino = destinationAccountId,
            DataMovimento = DateTime.Now,
            Valor = amount
        };
    }

    public static Transferencia Reconstruct(
        Guid id,
        Guid sourceAccountId,
        Guid destinationAccountId,
        DateTime transactionDate,
        decimal amount)
    {
        return new Transferencia
        {
            Id = id,
            IdContaCorrenteOrigem = sourceAccountId,
            IdContaCorrenteDestino = destinationAccountId,
            DataMovimento = transactionDate,
            Valor = amount
        };
    }
}
