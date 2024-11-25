using Kafka.Models;

namespace Kafka.Consumer;

public interface IConsumedMessageHandler<TKey, TValue>
{
    public Task HandleMessageAsync(
        IEnumerable<KafkaMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken);
}