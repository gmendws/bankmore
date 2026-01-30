namespace BankMore.ContaCorrente.Tests.ValueObjects;

using Shared.ValueObjects;
using Shared.Exceptions;
using FluentAssertions;
using Xunit;

public class SenhaTests
{
    [Fact]
    public void Senha_WithValidText_ShouldCreateWithHashAndSalt()
    {
        const string plainTextPassword = "senha123";

        var senha = Senha.Create(plainTextPassword);

        senha.Hash.Should().NotBeNullOrEmpty();
        senha.Salt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Senha_Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        const string plainTextPassword = "senha123";
        var senha = Senha.Create(plainTextPassword);

        var result = senha.Verify(plainTextPassword);

        result.Should().BeTrue();
    }

    [Fact]
    public void Senha_Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        var senha = Senha.Create("senha123");

        var result = senha.Verify("senhaErrada");

        result.Should().BeFalse();
    }
}
