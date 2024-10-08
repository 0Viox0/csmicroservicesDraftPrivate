namespace Task1;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<TResult> InfiniteAsyncZip<T, TResult>(
        this IAsyncEnumerable<T> first,
        Func<T[], TResult> resultSelector,
        params IAsyncEnumerable<T>[] others)
    {
        var allEnumerableEnumerators = new List<IAsyncEnumerator<T>> { first.GetAsyncEnumerator() };
        allEnumerableEnumerators.AddRange(others.Select(other => other.GetAsyncEnumerator()));

        try
        {
            while ((await Task.WhenAll(allEnumerableEnumerators
                       .Select(e => e.MoveNextAsync().AsTask())).ConfigureAwait(false))
                   .All(result => result))
            {
                yield return resultSelector(allEnumerableEnumerators.Select(e => e.Current).ToArray());
            }
        }
        finally
        {
            foreach (IAsyncEnumerator<T> enumerator in allEnumerableEnumerators)
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
