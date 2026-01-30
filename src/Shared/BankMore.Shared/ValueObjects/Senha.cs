namespace BankMore.Shared.ValueObjects;

using Exceptions;
using Enums;

public class Senha : ValueObject
{
    public string Hash { get; }
    public string Salt { get; }

    private Senha(string hash, string salt)
    {
        Hash = hash;
        Salt = salt;
    }

    public static Senha Create(string plainTextPassword)
    {
        if (string.IsNullOrWhiteSpace(plainTextPassword))
            throw new DomainException("Senha não pode ser vazia", TipoFalha.InvalidValue);

        if (plainTextPassword.Length < 6)
            throw new DomainException("Senha deve ter no mínimo 6 caracteres", TipoFalha.InvalidValue);

        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        var hash = BCrypt.Net.BCrypt.HashPassword(plainTextPassword, salt);

        return new Senha(hash, salt);
    }

    public static Senha Reconstruct(string hash, string salt)
    {
        return new Senha(hash, salt);
    }

    public bool Verify(string plainTextPassword)
    {
        return BCrypt.Net.BCrypt.Verify(plainTextPassword, Hash);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Hash;
        yield return Salt;
    }
}
