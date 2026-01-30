namespace BankMore.ContaCorrente.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.Commands;
using Application.Queries;
using System.Security.Claims;

[ApiController]
[Route("api/conta-corrente")]
public class AccountController(IMediator mediator) : ControllerBase
{
    [HttpPost("cadastrar")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new CreateAccountCommand(request.Cpf, request.Nome, request.Senha);
        var result = await mediator.Send(command);

        return Ok(new { numeroConta = result.AccountNumber });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var query = new LoginQuery(request.CpfOuNumeroConta, request.Senha);
        var result = await mediator.Send(query);

        return Ok(new 
        { 
            token = result.Token, 
            expiracao = result.Expiration 
        });
    }

    [Authorize]
    [HttpPost("inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate([FromBody] DeactivateRequest request)
    {
        var userId = GetLoggedUserId();
        
        var command = new DeactivateAccountCommand(userId, request.Senha);
        await mediator.Send(command);

        return NoContent();
    }

    [Authorize]
    [HttpPost("movimentacao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Transaction([FromBody] TransactionRequest request)
    {
        var userId = GetLoggedUserId();
        
        var command = new CreateTransactionCommand(
            request.IdempotenciaKey,
            request.IdContaCorrente,
            request.NumeroConta,
            request.TipoMovimento,
            request.Valor,
            userId
        );

        await mediator.Send(command);

        return NoContent();
    }

    [Authorize]
    [HttpGet("saldo")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Balance()
    {
        var userId = GetLoggedUserId();
        
        var query = new GetBalanceQuery(userId);
        var result = await mediator.Send(query);

        return Ok(new
        {
            numeroConta = result.AccountNumber,
            nomeTitular = result.AccountHolderName,
            dataHoraConsulta = result.QueryDateTime,
            saldo = result.Balance
        });
    }

    private Guid GetLoggedUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        return string.IsNullOrEmpty(userIdClaim) ? throw new UnauthorizedAccessException("Invalid token") : Guid.Parse(userIdClaim);
    }
}

public record RegisterRequest(string Cpf, string Nome, string Senha);
public record LoginRequest(string CpfOuNumeroConta, string Senha);
public record DeactivateRequest(string Senha);
public record TransactionRequest(
    string IdempotenciaKey,
    Guid? IdContaCorrente,
    int? NumeroConta,
    Shared.Enums.TipoMovimento TipoMovimento,
    decimal Valor
);
