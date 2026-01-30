namespace BankMore.Transferencia.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.Commands;
using System.Security.Claims;

[ApiController]
[Route("api/transferencia")]
public class TransferController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request)
    {
        var userId = GetLoggedUserId();
        var token = GetToken();
        
        var command = new CreateTransferCommand(
            request.IdempotenciaKey,
            request.NumeroContaDestino,
            request.Valor,
            userId,
            token
        );

        await mediator.Send(command);

        return NoContent();
    }

    private Guid GetLoggedUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        return string.IsNullOrEmpty(userIdClaim) ? throw new UnauthorizedAccessException("Invalid token") : Guid.Parse(userIdClaim);
    }

    private string GetToken()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            throw new UnauthorizedAccessException("Token not provided");

        return authHeader["Bearer ".Length..].Trim();
    }
}

public record CreateTransferRequest(
    string IdempotenciaKey,
    int NumeroContaDestino,
    decimal Valor
);
