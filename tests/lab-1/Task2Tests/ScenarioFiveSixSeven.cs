using FluentAssertions;
using NSubstitute;
using Task2.Implementations;
using Task2.Interfaces;
using Task2.Models;

namespace Task2Tests;

public class ScenarioFiveSixSeven
{
    private readonly ILibraryOperationService _mockLibraryOperationService;
    private readonly RequestClient _requestClient;

    public ScenarioFiveSixSeven()
    {
        _mockLibraryOperationService = Substitute.For<ILibraryOperationService>();
        _requestClient = new RequestClient(_mockLibraryOperationService);
    }

    [Fact]
    public async Task RunAsyncAndGetProperResultWhenCallingHandlerInBeginOperation()
    {
        var request = new RequestModel("something", [1, 2, 3]);
        byte[] response = [4, 5, 6];

        CancellationToken cancellationToken = CancellationToken.None;

        _mockLibraryOperationService.When(x => x.BeginOperation(Arg.Any<Guid>(), request, cancellationToken))
            .Do(callInfo =>
            {
                Guid passedRequestId = callInfo.Arg<Guid>();
                _requestClient.HandleOperationResult(passedRequestId, response);
            });

        Task<ResponseModel> resultTask = _requestClient.SendAsync(request, cancellationToken);
        ResponseModel result = await resultTask.ConfigureAwait(true);

        result.Data.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task SendAsyncAndThenGetEnteredExceptionHandlerAsResult()
    {
        var request = new RequestModel("something", [1, 2, 3]);

        CancellationToken cancellationToken = CancellationToken.None;

        _mockLibraryOperationService.When(x => x.BeginOperation(Arg.Any<Guid>(), request, cancellationToken))
            .Do(callInfo =>
            {
                Guid passedRequestId = callInfo.Arg<Guid>();
                Task.Delay(100).ContinueWith(_ =>
                {
                    _requestClient.HandleOperationError(
                        passedRequestId,
                        new InvalidOperationException("Something went wrong"));
                });
            });

        Task<ResponseModel> resultTask = _requestClient.SendAsync(request, cancellationToken);
        Func<Task<ResponseModel>> resultFunction = async () => await resultTask.ConfigureAwait(true);

        await resultFunction.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Something went wrong").ConfigureAwait(true);
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