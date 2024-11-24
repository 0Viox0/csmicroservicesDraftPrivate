using Kafka.Models;
using System.Threading.Channels;

namespace Kafka.Consumer;

public interface IMessageConsumerChannelWriter<TKey, TValue>
{
    public Task WriteToChannelAsync(
        ChannelWriter<KafkaMessage<TKey, TValue>> writer,
        CancellationToken cancellationToken = default);
}