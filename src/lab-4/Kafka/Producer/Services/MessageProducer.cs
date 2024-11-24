using Confluent.Kafka;
using Google.Protobuf;
using Kafka.Serializers;

namespace Kafka.Producer.Services;

public class MessageProducer<TKey, TValue> : IMessageProducer<TKey, TValue>
    where TKey : IMessage<TKey>, new()
    where TValue : IMessage<TValue>, new()
{
    private readonly IProducer<TKey, TValue> _producer;

    public MessageProducer(
        ProtobufSerializer<TKey> protobufKeySerializer,
        ProtobufSerializer<TValue> protobufValueSerializer)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
        };

        _producer = new ProducerBuilder<TKey, TValue>(config)
            .SetKeySerializer(protobufKeySerializer)
            .SetValueSerializer(protobufValueSerializer)
            .Build();
    }

    public Task ProduceAsync(ProducerMessage<TKey, TValue> message)
    {
        string topicName = "order_creation";

        return _producer.ProduceAsync(
            topicName,
            new Message<TKey, TValue>
            {
                Key = message.Key,
                Value = message.Value,
            });
    }
}