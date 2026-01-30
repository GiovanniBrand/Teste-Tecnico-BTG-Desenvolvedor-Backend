using FluentAssertions;
using KrtBank.Application.Queries.Accounts;
using KrtBank.Domain.Entities;
using KrtBank.Domain.Interfaces;
using KrtBank.Domain.Repositories;
using Moq;
using System.Linq.Expressions;

public class GetAccountByCpfHandlerTests
{
    private readonly Mock<IRepository<Account>> _repoMock;
    private readonly Mock<IRedisCacheService> _cacheMock;
    private readonly GetAccountByCpfQueryHandler _handler;

    public GetAccountByCpfHandlerTests()
    {
        _repoMock = new Mock<IRepository<Account>>();
        _cacheMock = new Mock<IRedisCacheService>();
        _handler = new GetAccountByCpfQueryHandler(_repoMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFromCache_WhenDataExistsInRedis()
    {
        var cpf = "12345678901";
        var cachedResponse = new AccountResponse(Guid.NewGuid(), "João do Cache", cpf, "Active");

        _cacheMock.Setup(x => x.GetAsync<AccountResponse>(It.IsAny<string>()))
                  .ReturnsAsync(cachedResponse);

        var result = await _handler.Handle(new GetAccountByCpfQuery(cpf), CancellationToken.None);

        result.Should().NotBeNull();
        result.AccountHolderName.Should().Be("João do Cache");

        _repoMock.Verify(x => x.GetFirstAsync(It.IsAny<Expression<Func<Account, bool>>>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}