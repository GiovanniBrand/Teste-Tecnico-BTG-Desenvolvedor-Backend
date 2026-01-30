using KrtBank.Application.Events;
using KrtBank.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public class CardServiceHandler : INotificationHandler<AccountBaseEvent>
{
    private readonly ILogger<CardServiceHandler> _logger;
    private readonly IMessageBus _messageBus;

    public CardServiceHandler(ILogger<CardServiceHandler> logger, IMessageBus messageBus)
    {
        _logger = logger;
        _messageBus = messageBus;
    }

    public async Task Handle(AccountBaseEvent notification, CancellationToken cancellationToken)
    {
        var queue = "cards-issuing-service";

        var messageData = new
        {
            AccountId = notification.Account.Id,
            HolderName = notification.Account.AccountHolderName,
            Cpf = notification.Account.Cpf
        };

        switch (notification)
        {
            case AccountCreatedEvent:
                // Solicita emissão de novo cartão
                await _messageBus.PublishAsync(queue, new { Action = "ISSUE_NEW_CARD", Data = messageData });
                _logger.LogInformation("Cartões: Solicitação de emissão enviada para {Cpf}.", notification.Account.Cpf);
                break;

            case AccountUpdatedEvent:
                // Verifica se a atualização exige novos limites
                await _messageBus.PublishAsync(queue, new { Action = "SYNC_LIMITS", Data = messageData });
                break;

            case AccountDeletedEvent:
                // Bloqueia cartões imediatamente
                await _messageBus.PublishAsync(queue, new { Action = "BLOCK_ALL_CARDS", Data = messageData });
                _logger.LogWarning("Cartões: Comando de bloqueio enviado para conta encerrada {Cpf}.", notification.Account.Cpf);
                break;
        }
    }
}