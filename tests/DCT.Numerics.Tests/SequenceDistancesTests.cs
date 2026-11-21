using System;
using DigitalCircularityToolkit.Numerics;
using Xunit;

public class SequenceDistancesTests
{
    static Func<int, int, double> Dist(double[] a, double[] b) => (i, j) => Math.Abs(a[i] - b[j]);

    [Fact]
    public void Frechet_of_identical_sequences_is_zero_and_respects_order()
    {
        var a = new[] { 0.0, 1.0, 2.0, 3.0 };
        Assert.Equal(0.0, SequenceDistances.DiscreteFrechet(4, 4, Dist(a, a)));
        // Same points in reverse order: Hausdorff 0, but Frechet must walk both in order.
        var r = new[] { 3.0, 2.0, 1.0, 0.0 };
        Assert.Equal(0.0, SequenceDistances.Hausdorff(4, 4, Dist(a, r)));
        Assert.Equal(3.0, SequenceDistances.DiscreteFrechet(4, 4, Dist(a, r)));
    }

    [Fact]
    public void Frechet_handles_long_sequences_without_recursion()
    {
        int n = 20000;
        var a = new double[n]; var b = new double[n];
        for (int i = 0; i < n; i++) { a[i] = i; b[i] = i + 0.5; }
        Assert.Equal(0.5, SequenceDistances.DiscreteFrechet(n, n, Dist(a, b)), 12);
    }

    [Fact]
    public void Hausdorff_is_the_larger_directed_distance()
    {
        var a = new[] { 0.0, 1.0 };
        var b = new[] { 0.0, 1.0, 10.0 };
        Assert.Equal(0.0, SequenceDistances.DirectedHausdorff(2, 3, Dist(a, b)));
        Assert.Equal(9.0, SequenceDistances.DirectedHausdorff(3, 2, Dist(b, a)));
        Assert.Equal(9.0, SequenceDistances.Hausdorff(2, 3, Dist(a, b)));
        Assert.Equal(9.0, SequenceDistances.Hausdorff(3, 2, Dist(b, a)));
    }
}
