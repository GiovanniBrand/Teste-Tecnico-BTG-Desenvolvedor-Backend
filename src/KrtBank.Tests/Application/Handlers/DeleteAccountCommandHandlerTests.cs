using Moq;
using FluentAssertions;
using System.Linq.Expressions;
using MediatR;
using KrtBank.Application.Commands.Accounts;
using KrtBank.Domain.Entities;
using KrtBank.Domain.Interfaces;
using KrtBank.Domain.Repositories;

namespace KrtBank.UnitTests.Application.Handlers;

public class DeleteAccountCommandHandlerTests
{
    private readonly Mock<IRepository<Account>> _repoMock;
    private readonly Mock<IRedisCacheService> _cacheMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly DeleteAccountCommandHandler _handler;

    public DeleteAccountCommandHandlerTests()
    {
        _repoMock = new Mock<IRepository<Account>>();
        _cacheMock = new Mock<IRedisCacheService>();
        _mediatorMock = new Mock<IMediator>();
        _uowMock = new Mock<IUnitOfWork>();
        _handler = new DeleteAccountCommandHandler(_repoMock.Object, _uowMock.Object, _mediatorMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRemoverDoBancoELimparCache_QuandoSucesso()
    {
        var cpf = "12345678901";
        var account = new Account("João Silva", cpf);

        _repoMock.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<Account, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(account);

        _repoMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        var result = await _handler.Handle(new DeleteAccountCommand(cpf), CancellationToken.None);

        result.Should().BeTrue();

        _repoMock.Verify(x => x.Delete(It.IsAny<Account>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _cacheMock.Verify(x => x.RemoveAsync($"account:{cpf}"), Times.Once);
    }
}