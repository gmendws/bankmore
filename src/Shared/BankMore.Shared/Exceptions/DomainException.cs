namespace BankMore.Shared.Exceptions;

using Enums;

/// <summary>
/// Exceção de regra de negócio
/// Lançada quando uma validação de domínio falha
/// </summary>
public class DomainException : Exception
{
    public TipoFalha TipoFalha { get; }
    
    public DomainException(string mensagem, TipoFalha tipoFalha) 
        : base(mensagem)
    {
        TipoFalha = tipoFalha;
    }
    
    public DomainException(string mensagem, TipoFalha tipoFalha, Exception innerException) 
        : base(mensagem, innerException)
    {
        TipoFalha = tipoFalha;
    }
}
