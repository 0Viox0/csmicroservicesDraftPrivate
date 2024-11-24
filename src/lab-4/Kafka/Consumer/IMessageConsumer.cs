namespace Kafka.Consumer;

public interface IMessageConsumer<TKey, TValue>
{
    public Task ConsumeAsync(
        IConsumedMessageHandler<TKey, TValue> handler,
        CancellationToken cancellationToken = default);
}