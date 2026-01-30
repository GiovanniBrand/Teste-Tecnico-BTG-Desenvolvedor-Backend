using FluentAssertions;
using KrtBank.Application.Commands.Login;
using KrtBank.Domain.Entities;
using KrtBank.Domain.Interfaces;
using KrtBank.Domain.Repositories;
using Moq;
using System.Linq.Expressions;

public class LoginCommandHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepoMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepoMock = new Mock<IRepository<User>>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _handler = new LoginCommandHandler(_userRepoMock.Object, _passwordServiceMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_DeveLogarComEmail_QuandoDadosForemValidos()
    {
        var command = new LoginCommand("teste@krtbank.com.br", "Senha123!");
        var user = new User("Usuario Teste", "teste@krtbank.com.br", "hash_senha");

        _userRepoMock.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
                     .ReturnsAsync(user);

        _passwordServiceMock.Setup(x => x.VerifyPassword(command.Password, It.IsAny<string>()))
                            .Returns(true);

        _tokenServiceMock.Setup(x => x.GenerateToken(user)).Returns("token_valido");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Token.Should().Be("token_valido");
        _userRepoMock.Verify(x => x.GetFirstAsync(It.Is<Expression<Func<User, bool>>>(exp => exp.ToString().Contains("Email")), default), Times.Once);
    }
}