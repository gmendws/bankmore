namespace BankMore.ContaCorrente.Application.Handlers;

using Queries;
using DTOs;
using Domain.Repositories;
using Services;
using MediatR;
using Shared.Exceptions;
using Shared.Enums;
using Shared.ValueObjects;

public class LoginHandler(
    IContaCorrenteRepository repository,
    IJwtService jwtService)
    : IRequestHandler<LoginQuery, LoginResult>
{
    public async Task<LoginResult> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.ContaCorrente? account = null;

        if (Cpf.TryParse(request.CpfOrAccountNumber, out var cpf))
        {
            account = await repository.GetByCpfAsync(cpf);
        }
        else if (int.TryParse(request.CpfOrAccountNumber, out var accountNumber))
        {
            account = await repository.GetByNumberAsync(accountNumber);
        }

        if (account == null)
            throw new DomainException("Usuário ou senha inválidos", TipoFalha.UserUnauthorized);

        if (!account.ValidatePassword(request.Password))
            throw new DomainException("Usuário ou senha inválidos", TipoFalha.UserUnauthorized);

        var token = jwtService.GenerateToken(account.Id, account.Numero, account.Nome);

        return new LoginResult(token.Token, token.Expiration);
    }
}
