using FluentAssertions;
using NSubstitute;
using Task2.Implementations;
using Task2.Interfaces;
using Task2.Models;

namespace Task2Tests;

public class ScenarioThreeFour
{
    private readonly ILibraryOperationService _mockLibraryOperationService;
    private readonly RequestClient _requestClient;

    public ScenarioThreeFour()
    {
        _mockLibraryOperationService = Substitute.For<ILibraryOperationService>();
        _requestClient = new RequestClient(_mockLibraryOperationService);
    }

    [Fact]
    public async Task SendAsyncCalledWithAlreadyCanceledCancellationToken()
    {
        var request = new RequestModel("something", [1, 2, 3]);
        var cancellationToken = new CancellationToken(canceled: true);

        Func<Task> resultFunction = async () => await _requestClient.SendAsync(request, cancellationToken).ConfigureAwait(true);

        await resultFunction.Should().ThrowAsync<TaskCanceledException>().ConfigureAwait(true);
    }

    [Fact]
    public async Task SendAsyncCanceledCancellationTokenGetsCancelledAfterSomeTime()
    {
        var request = new RequestModel("something", [1, 2, 3]);
        var tokenCancellationSource = new CancellationTokenSource();
        CancellationToken cancellationToken = tokenCancellationSource.Token;

        _mockLibraryOperationService.When(x => x.BeginOperation(Arg.Any<Guid>(), request, cancellationToken))
            .Do(_ =>
            {
                Task.Delay(100).ContinueWith(_ =>
                {
                    tokenCancellationSource.Cancel();
                });
            });

        Func<Task> resultFunction = async () => await _requestClient
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(true);

        await resultFunction.Should().ThrowAsync<TaskCanceledException>().ConfigureAwait(true);
    }
}