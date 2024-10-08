using FluentAssertions;
using Task1;

namespace Task1Tests;

public class ScenarioTwo
{
    public static IEnumerable<object[]> GetSameLengthData()
    {
        yield return new object[]
        {
            new object[] { 1, 2, 3 },
            new object[] { "a", "b", "c" },
            new object[] { 1.1, 2.2, 3.3 },
        };
    }

    private static readonly IEnumerable<object> FirstArray = [1, "a", 1.1];
    private static readonly IEnumerable<object> SecondArray = [2, "b", 2.2];
    private static readonly IEnumerable<object> ThirdArray = [3, "c", 3.3];

    [Theory]
    [MemberData(nameof(GetSameLengthData))]
    public void ZipWithSameLengthData(
        IEnumerable<object> collection1,
        IEnumerable<object> collection2,
        IEnumerable<object> collection3)
    {
        IEnumerable<object[]> result =
            collection1.InfiniteZip(collections => collections, collection2, collection3);

        result.Should().BeEquivalentTo(new[]
        {
            FirstArray,
            SecondArray,
            ThirdArray,
        });
    }

    [Theory]
    [MemberData(nameof(GetSameLengthData))]
    public async Task ZipAsyncWithSameLengthData(
        IEnumerable<object> collection1,
        IEnumerable<object> collection2,
        IEnumerable<object> collection3)
    {
        List<object[]> result = await collection1
            .ToAsyncEnumerable()
            .InfiniteAsyncZip(
                collections => collections,
                collection2.ToAsyncEnumerable(),
                collection3.ToAsyncEnumerable()).ToListAsync();

        result.Should()
            .BeEquivalentTo(new[]
            {
                FirstArray,
                SecondArray,
                ThirdArray,
            });
    }
}