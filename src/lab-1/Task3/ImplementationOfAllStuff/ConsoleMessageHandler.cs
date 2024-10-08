using Task3.TaskModel;

namespace Task3.ImplementationOfAllStuff;

public class ConsoleMessageHandler : IMessageHandler
{
    public async ValueTask HandleAsync(IEnumerable<Message> messages, CancellationToken cancellationToken)
    {
        string messageBatch = string.Empty;

        foreach (Message message in messages)
        {
            messageBatch += $"{message.Title}: {message.Text}\n";
        }

        await Task.Run(() => Console.WriteLine(messageBatch), cancellationToken).ConfigureAwait(false);
    }
}