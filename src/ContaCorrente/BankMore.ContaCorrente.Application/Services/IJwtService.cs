namespace BankMore.ContaCorrente.Application.Services;

using DTOs;

public interface IJwtService
{
    JwtToken GenerateToken(Guid accountId, int accountNumber, string name);
    Guid? ValidateToken(string token);
}
