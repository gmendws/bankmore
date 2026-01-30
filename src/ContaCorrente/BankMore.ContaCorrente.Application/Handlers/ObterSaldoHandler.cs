namespace BankMore.ContaCorrente.Application.Handlers;

using Queries;
using DTOs;
using Domain.Repositories;
using Domain.Services;
using MediatR;
using Shared.Exceptions;
using Shared.Enums;

public class GetBalanceHandler(
    IContaCorrenteRepository accountRepository,
    IMovimentoRepository transactionRepository)
    : IRequestHandler<GetBalanceQuery, BalanceResult>
{
    public async Task<BalanceResult> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(request.AccountId);
        
        if (account == null)
            throw new DomainException("Conta não encontrada", TipoFalha.InvalidAccount);

        account.ValidateIsActive();

        var transactions = await transactionRepository.GetByAccountAsync(account.Id);

        var balance = SaldoService.CalculateBalance(transactions);

        return new BalanceResult(
            account.Numero,
            account.Nome,
            DateTime.Now,
            balance
        );
    }
}
