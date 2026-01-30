namespace BankMore.ContaCorrente.Application.Queries;

using DTOs;
using MediatR;

public record GetBalanceQuery(
    Guid AccountId
) : IRequest<BalanceResult>;
