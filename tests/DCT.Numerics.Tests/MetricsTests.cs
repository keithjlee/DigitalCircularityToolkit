using DigitalCircularityToolkit.Numerics;
using Xunit;

public class MetricsTests
{
    [Fact]
    public void Euclidean_and_Manhattan()
    {
        Assert.Equal(5.0, Metrics.Euclidean(new[] { 0.0, 0.0 }, new[] { 3.0, 4.0 }), 12);
        Assert.Equal(7.0, Metrics.Manhattan(new[] { 0.0, 0.0 }, new[] { 3.0, -4.0 }), 12);
    }

    [Fact]
    public void Asymmetric_uses_power1_only_when_supply_is_at_least_demand_in_every_feature()
    {
        // valid: supply (5, 4) >= demand (2, 0); distance 5 -> 5^1
        Assert.Equal(5.0, Metrics.AsymmetricEuclidean(new[] { 2.0, 0.0 }, new[] { 5.0, 4.0 }, 1, 2), 12);
        // invalid in one feature: supply (5, -4) < demand in the second; distance 5 -> 5^2
        Assert.Equal(25.0, Metrics.AsymmetricEuclidean(new[] { 2.0, 0.0 }, new[] { 5.0, -4.0 }, 1, 2), 12);
        // below 1 the higher power makes the penalty smaller (documented caveat)
        Assert.True(Metrics.AsymmetricEuclidean(new[] { 0.5 }, new[] { 0.0 }, 1, 2) < 0.5);
    }

    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(double.PositiveInfinity, int.MaxValue)]
    [InlineData(double.NegativeInfinity, int.MinValue)]
    [InlineData(1e300, int.MaxValue)]
    [InlineData(-1e300, int.MinValue)]
    [InlineData(41.9, 41)]
    [InlineData(-41.9, -41)]
    public void SaturatingInt_never_wraps(double x, int expected) => Assert.Equal(expected, Metrics.SaturatingInt(x));

    [Fact]
    public void CheckDimensions_reports_empty_and_ragged_sets()
    {
        Assert.Null(Metrics.CheckDimensions(new[] { new[] { 1.0, 2.0 } }, new[] { new[] { 3.0, 4.0 }, new[] { 5.0, 6.0 } }));
        Assert.NotNull(Metrics.CheckDimensions(new double[0][], new[] { new[] { 1.0 } }));
        Assert.Contains("Demand feature vector 1", Metrics.CheckDimensions(new[] { new[] { 1.0 }, new[] { 1.0, 2.0 } }, new[] { new[] { 1.0 } }));
        Assert.Contains("Supply feature vector 0", Metrics.CheckDimensions(new[] { new[] { 1.0 } }, new[] { new[] { 1.0, 2.0 } }));
    }
}
