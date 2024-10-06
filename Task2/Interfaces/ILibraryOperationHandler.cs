namespace Task2.Interfaces;

public interface ILibraryOperationHandler
{
    void HandleOperationResult(Guid requestId, byte[] data);

    void HandleOperationError(Guid requestId, Exception ex);
}