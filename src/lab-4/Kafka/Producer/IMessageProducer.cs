namespace Kafka.Producer;

public interface IMessageProducer<TKey, TValue>
{
    Task ProduceAsync(ProducerMessage<TKey, TValue> message);
}