namespace BankMore.Shared.Enums;

/// <summary>
/// Tipos de falha de validação (conforme especificação do teste)
/// </summary>
public enum TipoFalha
{
    InvalidDocument,
    InvalidAccount,
    InactiveAccount,
    InvalidValue,
    InvalidType,
    UserUnauthorized
}
