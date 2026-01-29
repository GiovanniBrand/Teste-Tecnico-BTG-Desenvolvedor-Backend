using FluentAssertions;
using KrtBank.Application.Commands.Accounts;
using KrtBank.Application.Queries.Accounts;
using KrtBank.Domain.Entities;
using KrtBank.Domain.Interfaces;
using KrtBank.Domain.Repositories;
using Moq;
using System.Linq.Expressions;

public class CreateAccountHandlerTests
{
    private readonly Mock<IRepository<Account>> _repoMock;
    private readonly Mock<IRedisCacheService> _cacheMock;
    private readonly CreateAccountCommandHandler _handler;

    public CreateAccountHandlerTests()
    {
        _repoMock = new Mock<IRepository<Account>>();
        _cacheMock = new Mock<IRedisCacheService>();
        _handler = new CreateAccountCommandHandler(_repoMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_DeveSalvarNoBancoECache_QuandoDadosValidos()
    {
        var command = new CreateAccountCommand("Bruno Teste", "12345678901");
        _repoMock.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Account, bool>>>(), default))
                 .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(x => x.AddAsync(It.IsAny<Account>(), default), Times.Once);
        _repoMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        _cacheMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<AccountResponse>(), It.IsAny<TimeSpan>()), Times.Once);
        result.AccountHolderName.Should().Be(command.Name);
    }
}