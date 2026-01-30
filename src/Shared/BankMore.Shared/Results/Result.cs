namespace BankMore.Shared.Results;

using Enums;

/// <summary>
/// Padrão Result para evitar exceções em fluxos esperados
/// Permite retornar sucesso OU falha de forma explícita
/// </summary>
public class Result
{
    public bool Sucesso { get; }
    public string? Mensagem { get; }
    public TipoFalha? TipoFalha { get; }

    protected Result(bool sucesso, string? mensagem, TipoFalha? tipoFalha)
    {
        Sucesso = sucesso;
        Mensagem = mensagem;
        TipoFalha = tipoFalha;
    }

    public static Result Ok() => new(true, null, null);
    
    public static Result Falha(string mensagem, TipoFalha tipoFalha) 
        => new(false, mensagem, tipoFalha);
}

/// <summary>
/// Result genérico que carrega um valor em caso de sucesso
/// </summary>
public class Result<T> : Result
{
    public T? Valor { get; }

    private Result(bool sucesso, T? valor, string? mensagem, TipoFalha? tipoFalha) 
        : base(sucesso, mensagem, tipoFalha)
    {
        Valor = valor;
    }

    public static Result<T> Ok(T valor) => new(true, valor, null, null);
    
    public new static Result<T> Falha(string mensagem, TipoFalha tipoFalha) 
        => new(false, default, mensagem, tipoFalha);
}
