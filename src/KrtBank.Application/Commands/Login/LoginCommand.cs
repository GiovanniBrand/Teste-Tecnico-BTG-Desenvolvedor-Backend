using FluentValidation;
using KrtBank.Domain.Entities;
using MediatR;

namespace KrtBank.Application.Commands.Login
{
    public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;
    public record LoginResponse(string Token);
    public class LoginValidator : AbstractValidator<LoginCommand>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("E-mail inválido.");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Senha deve ter ao menos 6 caracteres.");
        }
    }
}
