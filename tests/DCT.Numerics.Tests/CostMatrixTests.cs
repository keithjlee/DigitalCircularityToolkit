using System;
using System.Linq;
using DigitalCircularityToolkit.Numerics;
using Xunit;

public class CostMatrixTests
{
    static double NaiveEuclidean(double[] d, double[] s, double p) =>
        Math.Pow(Math.Sqrt(d.Zip(s, (a, b) => (b - a) * (b - a)).Sum()), p);

    [Fact]
    public void Metrics_match_naive_reference()
    {
        var rng = new Random(1);
        for (int t = 0; t < 200; t++)
        {
            int k = 1 + t % 7;
            var d = Enumerable.Range(0, k).Select(_ => rng.NextDouble() * 1000).ToArray();
            var s = Enumerable.Range(0, k).Select(_ => rng.NextDouble() * 1000).ToArray();
            foreach (double p in new[] { 0.5, 1, 1.5, 2, 3 })
            {
                double naive = NaiveEuclidean(d, s, p);
                Assert.Equal(1.0, CostFunctions.Euclidean(d, s, p) / naive, 12);
            }
            bool shortfall = d.Zip(s, (a, b) => b < a).Any(x => x);
            double asym = CostFunctions.AsymmetricEuclidean(d, s, 1, 3, false);
            Assert.Equal(1.0, asym / NaiveEuclidean(d, s, shortfall ? 3 : 1), 12);
            Assert.Equal(shortfall, double.IsPositiveInfinity(CostFunctions.AsymmetricEuclidean(d, s, 1, 3, true)));
            Assert.Equal(d.Zip(s, (a, b) => Math.Abs(b - a)).Sum(), CostFunctions.Manhattan(d, s), 9);
        }
    }

    [Fact]
    public void Overflow_is_forbidden_never_negative()
    {
        double c = CostFunctions.Euclidean(new[] { 0.0 }, new[] { 1e200 }, 3);
        Assert.True(double.IsPositiveInfinity(c));
        Assert.True(CostFunctions.AsymmetricEuclidean(new[] { 1e200 }, new[] { 0.0 }, 1, 3, false) > 0);
    }

    [Fact]
    public void Matrix_rows_are_demand_and_built_values_match_the_function()
    {
        double[][] d = { new[] { 0.0, 0.0 }, new[] { 1.0, 1.0 }, new[] { 5.0, 5.0 } };
        double[][] s = { new[] { 3.0, 4.0 }, new[] { 1.0, 1.0 } };
        var c = CostFunctions.EuclideanMatrix(d, s, 1);
        Assert.Equal(3, c.Rows);
        Assert.Equal(2, c.Cols);
        Assert.Equal(5.0, c[0, 0], 12);
        Assert.Equal(0.0, c[1, 1], 12);
        var t = c.Transpose();
        Assert.Equal(c[2, 0], t[0, 2]);
        Assert.Equal("CostMatrix 3×2", c.ToString());
    }

    [Fact]
    public void Large_matrix_is_built_in_parallel_with_identical_values()
    {
        var rng = new Random(2);
        double[][] d = Enumerable.Range(0, 300).Select(_ => Enumerable.Range(0, 8).Select(__ => rng.NextDouble()).ToArray()).ToArray();
        double[][] s = Enumerable.Range(0, 400).Select(_ => Enumerable.Range(0, 8).Select(__ => rng.NextDouble()).ToArray()).ToArray();
        var c = CostFunctions.EuclideanMatrix(d, s, 2);
        for (int i = 0; i < 300; i += 37)
            for (int j = 0; j < 400; j += 41)
                Assert.Equal(CostFunctions.Euclidean(d[i], s[j], 2), c[i, j]);
    }

    [Fact]
    public void Invalid_values_and_features_are_reported()
    {
        Assert.Throws<ArgumentException>(() => new CostMatrix(1, 2, new[] { 1.0, double.NaN }));
        Assert.Throws<ArgumentException>(() => new CostMatrix(1, 1, new[] { double.NegativeInfinity }));
        var forbidden = new CostMatrix(1, 2, new[] { 1.0, double.PositiveInfinity });
        Assert.Equal(1, forbidden.ForbiddenCount);
        Assert.Equal("CostMatrix 1×2 (50% forbidden)", forbidden.ToString());
        Assert.Contains("non-finite", CostFunctions.CheckFeatures(new[] { new[] { double.NaN } }, new[] { new[] { 1.0 } }));
        Assert.Null(CostFunctions.CheckFeatures(new[] { new[] { 1.0 } }, new[] { new[] { 2.0 } }));
    }
}
