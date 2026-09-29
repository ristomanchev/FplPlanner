namespace ProektIntegrirani.Service.Logic;

// Poisson distribution helpers used by the expected-points model.
public static class PoissonMath
{
    // P(X = k) for k = 0..max.
    public static double[] Pmf(double mean, int max)
    {
        var pmf = new double[max + 1];
        pmf[0] = Math.Exp(-mean);
        for (var k = 1; k <= max; k++)
        {
            pmf[k] = pmf[k - 1] * mean / k;
        }

        return pmf;
    }

    // P(X >= k).
    public static double AtLeast(int k, double mean)
    {
        if (k <= 0) return 1;
        if (mean <= 0) return 0;

        var pmf = Pmf(mean, k - 1);
        return Math.Max(0, 1 - pmf.Sum());
    }

    // E[floor(X / d)], e.g. points for "every 3 saves" or "every 2 goals conceded".
    public static double ExpectedFloorDiv(double mean, int d)
    {
        if (mean <= 0) return 0;

        var max = (int)Math.Ceiling(mean + 12 * Math.Sqrt(mean) + 15);
        var pmf = Pmf(mean, max);

        var expected = 0.0;
        for (var k = d; k <= max; k++)
        {
            expected += k / d * pmf[k];
        }

        return expected;
    }
}
