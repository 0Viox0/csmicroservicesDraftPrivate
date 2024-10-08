namespace Task3.TaskModel;

public interface IMessageHandler
{
    ValueTask HandleAsync(IEnumerable<Message> messages, CancellationToken cancellationToken);
}