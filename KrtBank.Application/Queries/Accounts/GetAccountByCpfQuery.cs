using FluentValidation;
using MediatR;

namespace KrtBank.Application.Queries.Accounts
{
    public record GetAccountByCpfQuery(string Cpf) : IRequest<AccountResponse>;
    public record AccountResponse(
        Guid Id,
        string AccountHolderName,
        string Cpf,
        string Status
    );

    public class GetAccountByCpfValidator : AbstractValidator<GetAccountByCpfQuery>
    {
        public GetAccountByCpfValidator()
        {
            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("O CPF é obrigatório.")
                .Length(11).WithMessage("O CPF deve ter exatamente 11 dígitos.")
                .Matches(@"^\d+$").WithMessage("O CPF deve conter apenas números.");
        }
    }
}
