using Confluent.Kafka;
using Google.Protobuf;
using Kafka.Models;
using Kafka.Options;
using Kafka.Serializers;
using Microsoft.Extensions.Options;

namespace Kafka.Consumer.Services;

public class MessageConsumer<TKey, TValue> : IMessageConsumer<TKey, TValue>
    where TKey : IMessage<TKey>, new()
    where TValue : IMessage<TValue>, new()
{
    private readonly IDeserializer<TKey> _keyDeserializer;
    private readonly IDeserializer<TValue> _valueDeSerializer;
    private readonly ConsumerOptions _consumerOptions;

    public MessageConsumer(
        ProtobufDeserializer<TKey> protobufKeyDeserializer,
        ProtobufDeserializer<TValue> protobufValueDeSerializer,
        IOptions<ConsumerOptions> consumerConfig)
    {
        _keyDeserializer = protobufKeyDeserializer;
        _valueDeSerializer = protobufValueDeSerializer;
        _consumerOptions = consumerConfig.Value;
    }

    public async Task ConsumeAsync(
        IConsumedMessageHandler<TKey, TValue> handler,
        CancellationToken cancellationToken = default)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _consumerOptions.ConnectionUrl,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        using IConsumer<TKey, TValue> consumer = new ConsumerBuilder<TKey, TValue>(config)
            .SetKeyDeserializer(_keyDeserializer)
            .SetValueDeserializer(_valueDeSerializer)
            .Build();

        consumer.Subscribe(_consumerOptions.Topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<TKey, TValue> resultMessage = consumer.Consume(cancellationToken);
                var message = new KafkaMessage<TKey, TValue>(resultMessage.Message.Key, resultMessage.Message.Value);

                await handler.HandleMessageAsync(message, cancellationToken);
            }
        }
        finally
        {
            consumer.Close();
        }
    }
}