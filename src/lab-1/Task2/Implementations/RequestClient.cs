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

    public async Task<ResponseModel> SendAsync(RequestModel request, CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid();
        var taskCompletionSource = new TaskCompletionSource<ResponseModel>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (cancellationToken.IsCancellationRequested)
        {
            return await Task.FromCanceled<ResponseModel>(cancellationToken).ConfigureAwait(false);
        }

        CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(() =>
        {
            if (_tasks.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? source))
            {
                source.SetCanceled(cancellationToken);
            }
        });

        try
        {
            if (!_tasks.TryAdd(requestId, taskCompletionSource))
            {
                throw new InvalidOperationException("Task could not be added");
            }

            await Task.Run(() => _libraryOperationService
                    .BeginOperation(requestId, request, cancellationToken))
                .ConfigureAwait(false);

            return await taskCompletionSource.Task.ConfigureAwait(false);
        }
        finally
        {
            cancellationTokenRegistration.Dispose();
        }
    }

    public void HandleOperationResult(Guid requestId, byte[] data)
    {
        if (_tasks.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? source))
        {
            source.SetResult(new ResponseModel(data));
        }
    }

    public void HandleOperationError(Guid requestId, Exception ex)
    {
        if (_tasks.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? source))
        {
            source.SetException(ex);
        }
    }
}
