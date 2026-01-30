namespace BankMore.Shared.ValueObjects;

using Exceptions;
using Enums;

public class Cpf : ValueObject
{
    public string Numero { get; }

    public Cpf(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new DomainException("CPF não pode ser vazio", TipoFalha.InvalidDocument);

        var cleanCpf = RemoveFormatting(numero);
        
        if (!IsValid(cleanCpf))
            throw new DomainException("CPF inválido", TipoFalha.InvalidDocument);

        Numero = cleanCpf;
    }

    private static string RemoveFormatting(string cpf)
    {
        return new string(cpf.Where(char.IsDigit).ToArray());
    }

    private static bool IsValid(string cpf)
    {
        if (cpf.Length != 11)
            return false;

        if (cpf.Distinct().Count() == 1)
            return false;

        var multiplier1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var multiplier2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        var tempCpf = cpf[..9];
        var sum = 0;

        for (var i = 0; i < 9; i++)
            sum += int.Parse(tempCpf[i].ToString()) * multiplier1[i];

        var remainder = sum % 11;
        remainder = remainder < 2 ? 0 : 11 - remainder;

        var digit = remainder.ToString();
        tempCpf += digit;
        sum = 0;

        for (var i = 0; i < 10; i++)
            sum += int.Parse(tempCpf[i].ToString()) * multiplier2[i];

        remainder = sum % 11;
        remainder = remainder < 2 ? 0 : 11 - remainder;

        digit += remainder.ToString();

        return cpf.EndsWith(digit);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Numero;
    }

    public override string ToString() => Numero;

    public static implicit operator string(Cpf cpf) => cpf.Numero;
    
    public static bool TryParse(string? value, out Cpf? cpf)
    {
        cpf = null;
        
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            cpf = new Cpf(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
