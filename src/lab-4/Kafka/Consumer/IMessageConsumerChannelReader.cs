using Kafka.Models;
using System.Threading.Channels;

namespace Kafka.Consumer;

public interface IMessageConsumerChannelReader<TKey, TValue>
{
    public Task ReadMessagesFromChannel(
        ChannelReader<KafkaMessage<TKey, TValue>> reader,
        CancellationToken cancellationToken);
}