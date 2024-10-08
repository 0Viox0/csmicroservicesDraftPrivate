namespace Task3.TaskModel;

public interface IMessageSender
{
    ValueTask SendAsync(Message message, CancellationToken cancellationToken);
}