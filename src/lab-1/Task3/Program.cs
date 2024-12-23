using Task3.ImplementationOfAllStuff;
using Task3.TaskModel;

namespace Task3;

public static class Program
{
    public static async void Main()
    {
        var handler = new ConsoleMessageHandler();
        var processor = new MessageProcessor(handler, batchSize: 5, TimeSpan.FromMinutes(30));

        MessageProcessor sender = processor;
        MessageProcessor messageProcessor = processor;

        var cancellationToken = new CancellationTokenSource();

        Task processingTask = messageProcessor.ProcessAsync(cancellationToken.Token);

        IEnumerable<Message> messages = Enumerable.Range(1, 50).Select(i => new Message($"Title {i}", $"Text {i}"));

        await Parallel.ForEachAsync(messages, cancellationToken.Token, async (message, token) =>
        {
            await sender.SendAsync(message, token).ConfigureAwait(false);
        }).ConfigureAwait(false);

        messageProcessor.Complete();
        await processingTask.ConfigureAwait(false);
    }
}