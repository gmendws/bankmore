namespace BankMore.ContaCorrente.API.Filters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Exceptions;
using Shared.Enums;

/// <summary>
/// Filtro global que captura DomainException e retorna HTTP 400 com JSON padronizado
/// </summary>
public class DomainExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not DomainException domainException) return;
        var statusCode = domainException.TipoFalha switch
        {
            TipoFalha.UserUnauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };

        context.Result = new ObjectResult(new
        {
            mensagem = domainException.Message,
            tipo = domainException.TipoFalha.ToString()
        })
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }
}
