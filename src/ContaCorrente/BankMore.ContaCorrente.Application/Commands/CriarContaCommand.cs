namespace BankMore.ContaCorrente.Application.Commands;

using DTOs;
using MediatR;

public record CreateAccountCommand(
    string Cpf,
    string Nome,
    string Senha
) : IRequest<CreateAccountResult>;
