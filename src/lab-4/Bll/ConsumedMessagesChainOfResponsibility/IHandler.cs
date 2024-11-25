using Kafka.Models;
using Orders.Kafka.Contracts;

namespace Bll.ConsumedMessagesChainOfResponsibility;

public interface IHandler
{
    public IHandler SetNext(IHandler handler);

    public Task HandleAsync(
        KafkaMessage<OrderProcessingKey, OrderProcessingValue> message,
        CancellationToken cancellationToken);
}