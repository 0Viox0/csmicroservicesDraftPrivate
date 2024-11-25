using Kafka.Models;
using Orders.Kafka.Contracts;

namespace Bll.ConsumedMessagesChainOfResponsibility;

public abstract class HandlerBase : IHandler
{
    private IHandler? _nextHandler;

    public IHandler SetNext(IHandler handler)
    {
        _nextHandler = handler;

        return handler;
    }

    public virtual async Task HandleAsync(
        KafkaMessage<OrderProcessingKey, OrderProcessingValue> message,
        CancellationToken cancellationToken)
    {
        if (_nextHandler is not null)
            await _nextHandler.HandleAsync(message, cancellationToken);
    }
}