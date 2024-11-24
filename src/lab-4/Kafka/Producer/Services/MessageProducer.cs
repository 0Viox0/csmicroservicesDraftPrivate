using Confluent.Kafka;
using Google.Protobuf;
using Kafka.Models;
using Kafka.Options;
using Kafka.Serializers;
using Microsoft.Extensions.Options;

namespace Kafka.Producer.Services;

public class MessageProducer<TKey, TValue> : IMessageProducer<TKey, TValue>, IDisposable
    where TKey : IMessage<TKey>, new()
    where TValue : IMessage<TValue>, new()
{
    private readonly IProducer<TKey, TValue> _producer;
    private readonly string?_topic;

    public MessageProducer(
        ProtobufSerializer<TKey> protobufKeySerializer,
        ProtobufSerializer<TValue> protobufValueSerializer,
        IOptions<ProducerOptions> producerConfig)
    {
        ProducerOptions producerOptions = producerConfig.Value;
        _topic = producerOptions.Topic;

        var config = new ProducerConfig
        {
            BootstrapServers = producerOptions.ConnectionUrl,
            Acks = Acks.Leader,
        };

        _producer = new ProducerBuilder<TKey, TValue>(config)
            .SetKeySerializer(protobufKeySerializer)
            .SetValueSerializer(protobufValueSerializer)
            .Build();
    }

    public Task ProduceAsync(KafkaMessage<TKey, TValue> message, CancellationToken cancellationToken)
    {
        return _producer.ProduceAsync(
            _topic,
            new Message<TKey, TValue>
            {
                Key = message.Key,
                Value = message.Value,
            },
            cancellationToken);
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}