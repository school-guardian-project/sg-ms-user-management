using System.Text.Json;
using Confluent.Kafka;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Infrastructure.Messaging.Kafka;

public class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<string, string> _producer;

    public KafkaEventPublisher(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishAsync<T>(string topic, T @event)
    {
        var message = JsonSerializer.Serialize(@event);

        await _producer.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message
            }

        );
    }
}