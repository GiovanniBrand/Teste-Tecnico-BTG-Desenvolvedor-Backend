using KrtBank.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace KrtBank.Infrastructure.Messaging;

public class MockMessageBus : IMessageBus
{
    private readonly ILogger<MockMessageBus> _logger;

    public MockMessageBus(ILogger<MockMessageBus> logger) => _logger = logger;

    public Task PublishAsync<T>(string queue, T message)
    {
        // Simula o envio para o Broker
        _logger.LogWarning("[MESSAGE BUS] Mensagem enviada para a fila {Queue}: {@Message}", queue, message);
        return Task.CompletedTask;
    }
}