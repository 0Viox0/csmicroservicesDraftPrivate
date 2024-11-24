using Kafka.Models;

namespace Kafka.Producer;

public interface IMessageProducer<TKey, TValue>
{
    public Task ProduceAsync(KafkaMessage<TKey, TValue> message, CancellationToken cancellationToken = default);
}