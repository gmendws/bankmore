namespace BankMore.ContaCorrente.Tests.ValueObjects;

using Shared.ValueObjects;
using Shared.Exceptions;
using FluentAssertions;
using Xunit;

public class CpfTests
{
    [Theory]
    [InlineData("12345678909")]
    [InlineData("11144477735")]
    public void Cpf_WithValidNumber_ShouldCreateSuccessfully(string validCpf)
    {
        var cpf = new Cpf(validCpf);

        cpf.Numero.Should().Be(validCpf);
    }

    [Theory]
    [InlineData("12345678900")]
    [InlineData("11111111111")]
    [InlineData("00000000000")]
    public void Cpf_WithInvalidNumber_ShouldThrowException(string invalidCpf)
    {
        Action act = () => new Cpf(invalidCpf);

        act.Should().Throw<DomainException>()
            .WithMessage("CPF inválido");
    }

    [Fact]
    public void Cpf_WithFormatting_ShouldRemoveSpecialCharacters()
    {
        const string formattedCpf = "123.456.789-09";

        var cpf = new Cpf(formattedCpf);

        cpf.Numero.Should().Be("12345678909");
    }
}
