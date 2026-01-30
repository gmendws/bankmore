namespace BankMore.ContaCorrente.Domain.Entities;

using Shared.ValueObjects;
using Shared.Exceptions;
using Shared.Enums;

public class ContaCorrente
{
    public Guid Id { get; private set; }
    public int Numero { get; private set; }
    public Cpf CpfTitular { get; private set; }
    public string Nome { get; private set; }
    public bool Ativo { get; private set; }
    public Senha Senha { get; private set; }

    private ContaCorrente() { }

    public static ContaCorrente Create(Cpf cpf, string nome, string plainTextPassword, int numero)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do titular é obrigatório", TipoFalha.InvalidValue);

        return new ContaCorrente
        {
            Id = Guid.NewGuid(),
            Numero = numero,
            CpfTitular = cpf,
            Nome = nome,
            Ativo = true,
            Senha = Senha.Create(plainTextPassword)
        };
    }

    public static ContaCorrente Reconstruct(
        Guid id, 
        int numero, 
        Cpf cpf, 
        string nome, 
        bool ativo, 
        string passwordHash, 
        string passwordSalt)
    {
        return new ContaCorrente
        {
            Id = id,
            Numero = numero,
            CpfTitular = cpf,
            Nome = nome,
            Ativo = ativo,
            Senha = Senha.Reconstruct(passwordHash, passwordSalt)
        };
    }

    public bool ValidatePassword(string plainTextPassword)
    {
        return Senha.Verify(plainTextPassword);
    }

    public void Deactivate()
    {
        if (!Ativo)
            throw new DomainException("Conta já está inativa", TipoFalha.InactiveAccount);

        Ativo = false;
    }

    public void ValidateIsActive()
    {
        if (!Ativo)
            throw new DomainException("Conta inativa não pode realizar operações", TipoFalha.InactiveAccount);
    }
}
