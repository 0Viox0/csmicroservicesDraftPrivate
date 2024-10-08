using Itmo.Dev.Platform.Common.Extensions;
using System.Threading.Channels;
using Task3.TaskModel;

namespace Task3.ImplementationOfAllStuff;

public class MessageProcessor : IMessageSender, IMessageProcessor
{
    private readonly Channel<Message> _channel;
    private readonly IMessageHandler _handler;
    private readonly int _batchSize;
    private readonly TimeSpan _bachTimeout;

    public MessageProcessor(
        IMessageHandler handler,
        int batchSize,
        TimeSpan bachTimeout)
    {
        _handler = handler;
        _batchSize = batchSize;
        _bachTimeout = bachTimeout;

        _channel = Channel.CreateBounded<Message>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest,
        });
    }

    public async ValueTask SendAsync(Message message, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(message, cancellationToken).ConfigureAwait(false);
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await foreach (IReadOnlyList<Message>? batch in _channel.Reader.ReadAllAsync(cancellationToken)
                           .ChunkAsync(_batchSize, _bachTimeout).WithCancellation(cancellationToken))
        {
            if (batch.Count == 0) continue;

            await _handler.HandleAsync(batch.ToList(), cancellationToken).ConfigureAwait(false);
        }
    }

    public void Complete()
    {
        _channel.Writer.Complete();
    }
}