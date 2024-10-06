using System.Threading.Channels;
using Task3.TaskModel;

namespace Task3.ImplementationOfAllStuff;

public class MessageProcessor : IMessageSender, IMessageProcessor
{
    private readonly Channel<Message> _channel;
    private readonly IMessageHandler _handler;
    private readonly int _batchSize;
    private bool _isCompleted = false;

    public MessageProcessor(
        IMessageHandler handler,
        int batchSize)
    {
        _handler = handler;
        _batchSize = batchSize;

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
        var currentMessages = new List<Message>();

        while (!_isCompleted || _channel.Reader.Count > 0)
        {
            while (await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (currentMessages.Count < _batchSize && _channel.Reader.TryRead(out Message? message))
                {
                    currentMessages.Add(message);
                }

                if (currentMessages.Count == 0) continue;

                await _handler.HandleAsync(currentMessages, cancellationToken).ConfigureAwait(false);
                currentMessages.Clear();
            }
        }
    }

    public void Complete()
    {
        _isCompleted = true;
        _channel.Writer.Complete();
    }
}