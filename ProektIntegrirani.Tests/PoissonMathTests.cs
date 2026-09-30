using ProektIntegrirani.Service.Logic;

namespace ProektIntegrirani.Tests;

public class PoissonMathTests
{
    [Fact]
    public void Pmf_MatchesClosedForm()
    {
        var pmf = PoissonMath.Pmf(1.5, 3);

        Assert.Equal(Math.Exp(-1.5), pmf[0], 10);
        Assert.Equal(1.5 * Math.Exp(-1.5), pmf[1], 10);
        Assert.Equal(1.5 * 1.5 / 2 * Math.Exp(-1.5), pmf[2], 10);
    }

    [Fact]
    public void AtLeastOne_IsOneMinusProbabilityOfZero()
    {
        Assert.Equal(1 - Math.Exp(-2.0), PoissonMath.AtLeast(1, 2.0), 10);
    }

    [Theory]
    [InlineData(0, 3.0, 1.0)]   // P(X >= 0) is always 1
    [InlineData(5, 0.0, 0.0)]   // nothing expected, nothing happens
    public void AtLeast_EdgeCases(int k, double mean, double expected)
    {
        Assert.Equal(expected, PoissonMath.AtLeast(k, mean), 10);
    }

    [Fact]
    public void ExpectedFloorDiv_ByOne_IsTheMean()
    {
        // E[floor(X / 1)] = E[X] = mean.
        Assert.Equal(2.7, PoissonMath.ExpectedFloorDiv(2.7, 1), 6);
    }

    [Fact]
    public void ExpectedFloorDiv_ByTwo_IsBelowHalfTheMean()
    {
        // Integer division loses the remainder, so E[floor(X/2)] < E[X]/2.
        var value = PoissonMath.ExpectedFloorDiv(1.4, 2);

        Assert.InRange(value, 0.3, 0.7);
    }
}
