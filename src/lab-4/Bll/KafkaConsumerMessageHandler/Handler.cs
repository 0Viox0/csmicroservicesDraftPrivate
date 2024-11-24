using Kafka.Consumer;
using Kafka.Models;
using Orders.Kafka.Contracts;

namespace Bll.KafkaConsumerMessageHandler;

public class Handler : IConsumedMessageHandler<OrderProcessingKey, OrderProcessingValue>
{
    public Task HandleMessageAsync(
        IAsyncEnumerable<KafkaMessage<OrderProcessingKey, OrderProcessingValue>> message,
        CancellationToken cancellationToken)
    {
        
    }
}