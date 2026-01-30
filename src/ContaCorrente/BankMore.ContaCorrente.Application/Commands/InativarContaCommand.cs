namespace BankMore.ContaCorrente.Application.Commands;

using MediatR;

public record DeactivateAccountCommand(
    Guid LoggedUserId,
    string Password
) : IRequest;
