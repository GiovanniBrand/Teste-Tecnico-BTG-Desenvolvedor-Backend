using FluentValidation;
using KrtBank.Domain.Enums;
using MediatR;

namespace KrtBank.Application.Commands.Accounts
{
    public record UpdateAccountStatusCommand(string Cpf, AccountStatus Status) : IRequest<bool>;

    public class UpdateAccountStatusValidator : AbstractValidator<UpdateAccountStatusCommand>
    {
        public UpdateAccountStatusValidator()
        {
            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("O CPF é obrigatório para localizar a conta.")
                .Length(11).WithMessage("O CPF deve ter exatamente 11 dígitos.")
                .Matches(@"^\d+$").WithMessage("O CPF deve conter apenas números.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("O status fornecido é inválido. Use 0 para Inativo ou 1 para Ativo.");
        }
    }
}
