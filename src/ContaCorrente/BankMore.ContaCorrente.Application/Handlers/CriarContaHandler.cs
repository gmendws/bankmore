namespace BankMore.ContaCorrente.Application.Handlers;

using Commands;
using DTOs;
using Domain.Repositories;
using MediatR;
using Shared.ValueObjects;
using Shared.Exceptions;
using Shared.Enums;

public class CreateAccountHandler(IContaCorrenteRepository repository)
    : IRequestHandler<CreateAccountCommand, CreateAccountResult>
{
    public async Task<CreateAccountResult> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var cpf = new Cpf(request.Cpf);

        var existingAccount = await repository.GetByCpfAsync(cpf);
        if (existingAccount != null)
            throw new DomainException("CPF já cadastrado", TipoFalha.InvalidDocument);

        var accountNumber = await repository.GetNextAccountNumberAsync();

        var account = Domain.Entities.ContaCorrente.Create(
            cpf, 
            request.Nome, 
            request.Senha, 
            accountNumber
        );

        await repository.AddAsync(account);

        return new CreateAccountResult(account.Numero);
    }
}
