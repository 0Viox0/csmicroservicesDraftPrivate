using Kafka.Consumer;
using Kafka.Models;
using Orders.Kafka.Contracts;

namespace Bll.KafkaConsumerMessageHandler;

public class Handler : IConsumedMessageHandler<OrderProcessingKey, OrderProcessingValue>
{
    public async Task HandleMessageAsync(
        IEnumerable<KafkaMessage<OrderProcessingKey, OrderProcessingValue>> messages,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        // TODO: RESTfull principle in api
        // TODO: fix enums
        // TODO: write this handler that will handle all types of different messages
        // TODO: add additional enpoints in http gateway for processing order service
        // TODO: handle mistake cases and test application
        Console.Out.WriteLine("batch started--------------------------------------");

        foreach (KafkaMessage<OrderProcessingKey, OrderProcessingValue> message in messages)
        {
            Console.Out.WriteLine($"message was received reactively: {message.Value}");
        }

        Console.Out.WriteLine("batch finished--------------------------------------");
    }
}