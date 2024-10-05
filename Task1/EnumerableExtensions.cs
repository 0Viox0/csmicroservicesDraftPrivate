namespace Task1;

public static class EnumerableExtensions
{
    public static IEnumerable<TResult> InfiniteZip<T, TResult>(
        this IEnumerable<T> first,
        Func<T[], TResult> resultSelector,
        params IEnumerable<T>[] others)
    {
        var allEnumerableEnumerators = new List<IEnumerator<T>> { first.GetEnumerator() };
        allEnumerableEnumerators.AddRange(others.Select(other => other.GetEnumerator()));

        try
        {
            while (allEnumerableEnumerators.All(enumerator => enumerator.MoveNext()))
            {
                yield return resultSelector(allEnumerableEnumerators
                    .Select(enumerator => enumerator.Current).ToArray());
            }
        }
        finally
        {
            foreach (IEnumerator<T> enumerator in allEnumerableEnumerators)
            {
                enumerator.Dispose();
            }
        }
    }
}