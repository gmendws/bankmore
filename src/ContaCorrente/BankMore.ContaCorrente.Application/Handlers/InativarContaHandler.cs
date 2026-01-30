namespace BankMore.ContaCorrente.Application.Handlers;

using Commands;
using Domain.Repositories;
using MediatR;
using Shared.Exceptions;
using Shared.Enums;

public class DeactivateAccountHandler(IContaCorrenteRepository repository) : IRequestHandler<DeactivateAccountCommand>
{
    public async Task Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.LoggedUserId);
        
        if (account == null)
            throw new DomainException("Conta não encontrada", TipoFalha.InvalidAccount);

        if (!account.ValidatePassword(request.Password))
            throw new DomainException("Senha inválida", TipoFalha.UserUnauthorized);

        account.Deactivate();

        await repository.UpdateAsync(account);
    }
}
