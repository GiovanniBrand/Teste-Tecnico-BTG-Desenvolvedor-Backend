using FluentValidation;
using MediatR;

namespace KrtBank.Application.Commands.Accounts
{
    public record DeleteAccountCommand(string Cpf) : IRequest<bool>;

    public class DeleteAccountValidator : AbstractValidator<DeleteAccountCommand>
    {
        public DeleteAccountValidator()
        {
            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("O CPF é obrigatório para exclusão.")
                .Length(11).WithMessage("CPF inválido.")
                .Matches(@"^\d+$").WithMessage("CPF deve conter apenas números.");
        }
    }
}
