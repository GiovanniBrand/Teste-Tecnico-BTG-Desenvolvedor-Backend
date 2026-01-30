namespace KrtBank.Application.Interfaces;

/// <summary>
/// Abstração para envio de mensagens para Message Brokers (RabbitMQ, Kafka, etc.)
/// </summary>
public interface IMessageBus
{
    Task PublishAsync<T>(string topicOrQueue, T message);
}