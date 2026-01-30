namespace BankMore.ContaCorrente.Tests.Entities;

using ContaCorrente.Domain.Entities;
using Shared.ValueObjects;
using Shared.Exceptions;
using FluentAssertions;
using Xunit;

public class ContaCorrenteTests
{
    [Fact]
    public void ContaCorrente_Create_ShouldCreateActiveAccount()
    {
        var cpf = new Cpf("12345678909");
        var nome = "João Silva";
        var senha = "senha123";
        var numero = 1;

        var conta = ContaCorrente.Create(cpf, nome, senha, numero);

        conta.Id.Should().NotBeEmpty();
        conta.Numero.Should().Be(numero);
        conta.Ativo.Should().BeTrue();
    }

    [Fact]
    public void ContaCorrente_Deactivate_ShouldChangeStatusToInactive()
    {
        var cpf = new Cpf("12345678909");
        var conta = ContaCorrente.Create(cpf, "João Silva", "senha123", 1);

        conta.Deactivate();

        conta.Ativo.Should().BeFalse();
    }

    [Fact]
    public void ContaCorrente_Deactivate_WhenAlreadyInactive_ShouldThrowException()
    {
        var cpf = new Cpf("12345678909");
        var conta = ContaCorrente.Create(cpf, "João Silva", "senha123", 1);
        conta.Deactivate();

        var act = conta.Deactivate;

        act.Should().Throw<DomainException>()
            .WithMessage("Conta já está inativa");
    }
}
