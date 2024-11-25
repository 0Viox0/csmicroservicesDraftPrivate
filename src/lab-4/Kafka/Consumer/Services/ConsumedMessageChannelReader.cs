using Google.Protobuf;
using Itmo.Dev.Platform.Common.Extensions;
using Kafka.Models;
using Kafka.Options;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace Kafka.Consumer.Services;

public class ConsumedMessageChannelReader<TKey, TValue>
    : IMessageConsumerChannelReader<TKey, TValue>
        where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
{
    private readonly ConsumerOptions _consumerOptions;
    private readonly IConsumedMessageHandler<TKey, TValue> _handler;

    public ConsumedMessageChannelReader(
        IOptions<ConsumerOptions> consumerConfig,
        IConsumedMessageHandler<TKey, TValue> handler)
    {
        _consumerOptions = consumerConfig.Value;
        _handler = handler;
    }

    public async Task ReadMessagesFromChannel(
        ChannelReader<KafkaMessage<TKey, TValue>> reader,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        IAsyncEnumerable<IReadOnlyList<KafkaMessage<TKey, TValue>>> enumerable = reader
            .ReadAllAsync(cancellationToken)
            .ChunkAsync(_consumerOptions.BatchSize, _consumerOptions.BatchTimeout);

        await foreach (IReadOnlyList<KafkaMessage<TKey, TValue>> chunk in enumerable)
        {
            await _handler.HandleMessageAsync(chunk, cancellationToken);
        }
    }
}