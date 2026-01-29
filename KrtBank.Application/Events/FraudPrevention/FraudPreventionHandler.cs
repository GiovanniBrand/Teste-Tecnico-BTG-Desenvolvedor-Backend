using KrtBank.Application.Events;
using KrtBank.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public class FraudPreventionHandler : INotificationHandler<AccountBaseEvent>
{
    private readonly ILogger<FraudPreventionHandler> _logger;
    private readonly IMessageBus _messageBus; // Injeção da interface

    public FraudPreventionHandler(ILogger<FraudPreventionHandler> logger, IMessageBus messageBus)
    {
        _logger = logger;
        _messageBus = messageBus;
    }

    public async Task Handle(AccountBaseEvent notification, CancellationToken cancellationToken)
    {
        var queue = "fraud-prevention-service";

        // Criamos um DTO de integração para não vazar a Entidade inteira para a fila
        var integrationEvent = new
        {
            AccountId = notification.Account.Id,
            Cpf = notification.Account.Cpf,
            Timestamp = DateTime.UtcNow
        };

        switch (notification)
        {
            case AccountCreatedEvent:
                await _messageBus.PublishAsync(queue, new { Action = "CREATE", Data = integrationEvent });
                break;
            case AccountUpdatedEvent:
                await _messageBus.PublishAsync(queue, new { Action = "UPDATE", Data = integrationEvent });
                break;
            case AccountDeletedEvent:
                await _messageBus.PublishAsync(queue, new { Action = "DELETE", Data = integrationEvent });
                break;
        }
    }
}