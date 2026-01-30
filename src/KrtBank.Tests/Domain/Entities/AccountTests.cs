using FluentAssertions;
using KrtBank.Domain.Entities;
using KrtBank.Domain.Enums;

public class AccountTests
{
    [Fact]
    public void UpdateStatus_ShouldChangeStatus_WhenValidStatusProvided()
    {
        var account = new Account("João Silva", "12345678901");
        account.UpdateStatus(AccountStatus.Inactive);
        account.AccountStatus.Should().Be(AccountStatus.Inactive);
    }
}