using FluentAssertions;
using Task1;

namespace Task1Tests;

public class ScenarioThree
{
    public static IEnumerable<object[]> GetSameLengthData()
    {
        yield return new object[]
        {
            new object[] { 1, 2, 3 },
            new object[] { "a", "b" },
            new object[] { 1.1, 2.2, 3.3 },
        };
    }

    private static readonly IEnumerable<object> FirstArray = [1, "a", 1.1];
    private static readonly IEnumerable<object> SecondArray = [2, "b", 2.2];

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
            });
    }
}
