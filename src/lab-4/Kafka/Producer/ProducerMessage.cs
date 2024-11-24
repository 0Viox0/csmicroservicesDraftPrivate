namespace Kafka.Producer;

public record ProducerMessage<TKey, TValue>(TKey Key, TValue Value);