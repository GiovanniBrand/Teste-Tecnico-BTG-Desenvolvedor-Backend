using KrtBank.Domain.Entities;
using KrtBank.Domain.Exceptions;
using KrtBank.Domain.Interfaces;
using KrtBank.Domain.Repositories;
using MediatR;

namespace KrtBank.Application.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;

        public LoginCommandHandler(IRepository<User> userRepository, IPasswordService passwordService, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetFirstAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null || !_passwordService.VerifyPassword(request.Password, user.Password))
                throw new UnauthorizedException("E-mail ou senha inválidos.");
            var token = _tokenService.GenerateToken(user);

            return new LoginResponse(token);
        }
    }
}
