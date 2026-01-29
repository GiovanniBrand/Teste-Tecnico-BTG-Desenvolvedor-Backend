using KrtBank.Domain.Entities;
using MediatR;

namespace KrtBank.Application.Events
{
    public abstract record AccountBaseEvent(Account Account) : INotification;

    public record AccountCreatedEvent(Account Account) : AccountBaseEvent(Account);
    public record AccountUpdatedEvent(Account Account) : AccountBaseEvent(Account);
    public record AccountDeletedEvent(Account Account) : AccountBaseEvent(Account);
}
