namespace BankMore.ContaCorrente.Application.Queries;

using DTOs;
using MediatR;

public record LoginQuery(
    string CpfOrAccountNumber,
    string Password
) : IRequest<LoginResult>;
