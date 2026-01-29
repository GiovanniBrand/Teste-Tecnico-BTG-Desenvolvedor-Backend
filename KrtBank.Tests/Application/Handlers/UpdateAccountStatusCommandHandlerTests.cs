using FluentAssertions;
using KrtBank.Application.Commands.Accounts;
using KrtBank.Domain.Entities;
using KrtBank.Domain.Enums;
using KrtBank.Domain.Interfaces;
using KrtBank.Domain.Repositories;
using Moq;
using System.Linq.Expressions;

public class UpdateAccountStatusCommandHandlerTests
{
    private readonly Mock<IRepository<Account>> _repoMock;
    private readonly Mock<IRedisCacheService> _cacheMock;
    private readonly UpdateAccountStatusCommandHandler _handler;

    public UpdateAccountStatusCommandHandlerTests()
    {
        _repoMock = new Mock<IRepository<Account>>();
        _cacheMock = new Mock<IRedisCacheService>();
        _handler = new UpdateAccountStatusCommandHandler(_repoMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_DeveAtualizarStatusELimparCache_QuandoDadosValidos()
    {
        var cpf = "12345678901";
        var account = new Account("João Silva", cpf);
        var command = new UpdateAccountStatusCommand(cpf, AccountStatus.Inactive);

        _repoMock.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<Account, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(account);

        _repoMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        account.AccountStatus.Should().Be(AccountStatus.Inactive);

        _repoMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Once);
        _repoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _cacheMock.Verify(x => x.RemoveAsync($"account:{cpf}"), Times.Once);
    }
}