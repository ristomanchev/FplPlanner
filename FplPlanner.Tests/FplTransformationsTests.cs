using FplPlanner.Domain.Enums;
using FplPlanner.Service.Implementation;

namespace FplPlanner.Tests;

public class FplTransformationsTests
{
    [Theory]
    [InlineData(1, Position.Goalkeeper)]
    [InlineData(2, Position.Defender)]
    [InlineData(3, Position.Midfielder)]
    [InlineData(4, Position.Forward)]
    public void ElementType_MapsToPosition(int elementType, Position expected)
    {
        Assert.Equal(expected, FplTransformations.ToPosition(elementType));
    }

    [Fact]
    public void UnknownElementType_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FplTransformations.ToPosition(5));
    }

    [Theory]
    [InlineData("a", PlayerStatus.Available)]
    [InlineData("d", PlayerStatus.Doubtful)]
    [InlineData("i", PlayerStatus.Injured)]
    [InlineData("s", PlayerStatus.Suspended)]
    [InlineData("u", PlayerStatus.Unavailable)]
    [InlineData("n", PlayerStatus.NotInSquad)]
    public void StatusCode_MapsToStatus(string code, PlayerStatus expected)
    {
        Assert.Equal(expected, FplTransformations.ToPlayerStatus(code));
    }

    [Theory]
    [InlineData(105, 10.5)]
    [InlineData(40, 4.0)]
    public void NowCost_IsTenthsOfAMillion(int nowCost, decimal expected)
    {
        Assert.Equal(expected, FplTransformations.ToMillions(nowCost));
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("  ", null)]
    [InlineData(" Hamstring injury ", "Hamstring injury")]
    public void News_IsTrimmedAndEmptyBecomesNull(string news, string? expected)
    {
        Assert.Equal(expected, FplTransformations.ToNews(news));
    }
}
