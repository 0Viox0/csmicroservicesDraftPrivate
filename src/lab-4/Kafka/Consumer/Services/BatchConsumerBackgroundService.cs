using Kafka.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

namespace Kafka.Consumer.Services;

public class BatchConsumerBackgroundService<TKey, TValue> : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public BatchConsumerBackgroundService(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        IMessageConsumerChannelReader<TKey, TValue> channelReader =
            scope.ServiceProvider.GetRequiredService<IMessageConsumerChannelReader<TKey, TValue>>();

        IMessageConsumerChannelWriter<TKey, TValue> channelWriter =
            scope.ServiceProvider.GetRequiredService<IMessageConsumerChannelWriter<TKey, TValue>>();

        var channelOptions = new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        };

        var channel = Channel.CreateUnbounded<KafkaMessage<TKey, TValue>>(channelOptions);

        await Task.WhenAll(
            channelReader.ReadMessagesFromChannel(channel.Reader, stoppingToken),
            channelWriter.WriteToChannelAsync(channel.Writer, stoppingToken));
    }
}