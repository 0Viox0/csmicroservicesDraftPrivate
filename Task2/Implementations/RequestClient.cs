using System.Collections.Concurrent;
using Task2.Interfaces;
using Task2.Models;

namespace Task2.Implementations;

public class RequestClient : IRequestClient, ILibraryOperationHandler
{
    private readonly ILibraryOperationService _libraryOperationService;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ResponseModel>> _tasks = new();

    public RequestClient(ILibraryOperationService libraryOperationService)
    {
        _libraryOperationService = libraryOperationService;
    }

    public Task<ResponseModel> SendAsync(RequestModel request, CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid();
        var taskCompletionSource = new TaskCompletionSource<ResponseModel>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (cancellationToken.IsCancellationRequested)
        {
            taskCompletionSource.SetCanceled();
            return taskCompletionSource.Task;
        }

        cancellationToken.Register(() =>
        {
            if (_tasks.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? source))
            {
                source.TrySetCanceled();
            }
        });

        if (!_tasks.TryAdd(requestId, taskCompletionSource))
        {
            throw new InvalidOperationException("Task could not be added");
        }

        try
        {
            _libraryOperationService.BeginOperation(requestId, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _tasks.TryRemove(requestId, out _);
            taskCompletionSource.TrySetException(ex);
        }

        return taskCompletionSource.Task;
    }

    public void HandleOperationResult(Guid requestId, byte[] data)
    {
        if (_tasks.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? source))
        {
            source.TrySetResult(new ResponseModel(data));
        }
    }

    public void HandleOperationError(Guid requestId, Exception ex)
    {
        if (_tasks.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? source))
        {
            source.TrySetException(ex);
        }
    }
}
