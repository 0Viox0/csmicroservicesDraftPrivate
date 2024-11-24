using Kafka.Models;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

namespace Kafka.Consumer.Services;

public class BatchConsumerBackgroundService<TKey, TValue> : BackgroundService
{
    private readonly IMessageConsumerChannelReader<TKey, TValue> _channelReader;
    private readonly IMessageConsumerChannelWriter<TKey, TValue> _channelWriter;

    public BatchConsumerBackgroundService(
        IMessageConsumerChannelReader<TKey, TValue> channelReader,
        IMessageConsumerChannelWriter<TKey, TValue> channelWriter)
    {
        _channelReader = channelReader;
        _channelWriter = channelWriter;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channelOptions = new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        };

        var channel = Channel.CreateUnbounded<KafkaMessage<TKey, TValue>>(channelOptions);

        await Task.WhenAll(
            _channelReader.ReadMessagesFromChannel(channel.Reader, stoppingToken),
            _channelWriter.WriteToChannelAsync(channel.Writer, stoppingToken));
    }
}