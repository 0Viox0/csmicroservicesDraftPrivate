using FluentAssertions;
using Task1;

namespace Task1Tests;

public class ScenarioOne
{
    private static readonly int[] Array1 = [1];
    private static readonly int[] Array2 = [2];
    private static readonly int[] Array3 = [3];
    private static readonly int[] SourceArray = [1, 2, 3];

    [Fact]
    public void ZipNoArgumentTest()
    {
        int[] collection = [1, 2, 3];

        IEnumerable<int[]> result = collection.InfiniteZip(ints => ints);

        result.Should().BeEquivalentTo(new[]
        {
            Array1,
            Array2,
            Array3,
        });
    }

    [Fact]
    public async Task ZipAsyncNoArgumentTest()
    {
        IAsyncEnumerable<int> collection = SourceArray.ToAsyncEnumerable();

        List<int[]> result = await collection.InfiniteAsyncZip(col => col).ToListAsync();

        result.Should().BeEquivalentTo(new[]
        {
            Array1,
            Array2,
            Array3,
        });
    }
}