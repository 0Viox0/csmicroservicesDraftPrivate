using Bll.ConsumedMessagesChainOfResponsibility;
using Kafka.Consumer;
using Kafka.Models;
using Orders.Kafka.Contracts;

namespace Bll.KafkaConsumerMessageHandler;

public class Handler : IConsumedMessageHandler<OrderProcessingKey, OrderProcessingValue>
{
    private readonly IHandler _handler;

    public Handler(IHandler handler)
    {
        _handler = handler;
    }

    public async Task HandleMessageAsync(
        IEnumerable<KafkaMessage<OrderProcessingKey, OrderProcessingValue>> messages,
        CancellationToken cancellationToken)
    {
        foreach (KafkaMessage<OrderProcessingKey, OrderProcessingValue> message in messages)
        {
            await _handler.HandleAsync(message, cancellationToken);
        }
    }
}