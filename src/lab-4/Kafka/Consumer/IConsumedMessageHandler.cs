using Kafka.Models;

namespace Kafka.Consumer;

public interface IConsumedMessageHandler<TKey, TValue>
{
    public Task HandleMessageAsync(KafkaMessage<TKey, TValue> message, CancellationToken cancellationToken);
}