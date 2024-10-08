namespace Task3.TaskModel;

public interface IMessageProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken);

    void Complete();
}