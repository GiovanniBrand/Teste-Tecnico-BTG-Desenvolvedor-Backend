using FluentValidation;
using KrtBank.Application.Queries.Accounts;
using MediatR;

namespace KrtBank.Application.Commands.Accounts
{
    public record CreateAccountCommand(string Name, string Cpf) : IRequest<AccountResponse>;
    public class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
    {
        public CreateAccountValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome do titular é obrigatório.")
                .MaximumLength(150).WithMessage("O nome não pode exceder 150 caracteres.");

            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("O CPF é obrigatório.")
                .Length(11).WithMessage("O CPF deve ter 11 dígitos.")
                .Matches(@"^\d+$").WithMessage("Use apenas números no CPF.");
        }
    }
}
