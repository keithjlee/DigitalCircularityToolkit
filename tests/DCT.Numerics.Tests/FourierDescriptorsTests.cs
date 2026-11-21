using System;
using System.Linq;
using System.Numerics;
using DigitalCircularityToolkit.Numerics;
using Xunit;

public class FourierDescriptorsTests
{
    static void AssertPrefix(double[] expected, double[] actual, int digits = 9)
    {
        for (int i = 0; i < expected.Length; i++) Assert.Equal(expected[i], actual[i], digits);
    }

    [Theory]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(64)]
    public void Real_descriptor_returns_harmonic_amplitudes_independent_of_sample_count(int n)
    {
        // r(t) = 3 + 2 cos t + 1.5 sin 3t  ->  A1 = 2, A2 = 0, A3 = 1.5
        double[] r = Enumerable.Range(0, n).Select(j => { double t = 2 * Math.PI * j / n; return 3 + 2 * Math.Cos(t) + 1.5 * Math.Sin(3 * t); }).ToArray();
        double[] d = FourierDescriptors.FromReal(r, false, out double mean);
        Assert.Equal(n / 2, d.Length);
        AssertPrefix(new[] { 2.0, 0.0, 1.5, 0.0, 0.0 }, d);
        Assert.Equal(3.0, mean, 9);
    }

    static Complex[] Shape(int n, double scale = 1) => Enumerable.Range(0, n).Select(j =>
    {
        double t = 2 * Math.PI * j / n;
        return new Complex(10, -4) + scale * (5 * Complex.Exp(Complex.ImaginaryOne * t) + Complex.Exp(-Complex.ImaginaryOne * t)
                                            + 0.5 * Complex.Exp(2 * Complex.ImaginaryOne * t));
    }).ToArray();

    [Theory]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(64)]
    public void Complex_descriptor_orders_by_frequency_magnitude(int n)
    {
        double[] d = FourierDescriptors.FromComplex(Shape(n), false, out double c1);
        Assert.Equal(n - 1, d.Length);
        AssertPrefix(new[] { 5.0, 1.0, 0.5, 0.0, 0.0 }, d);   // |c1|, |c-1|, |c2|, |c-2|, |c3|
        Assert.Equal(5.0, c1, 9);
    }

    [Fact]
    public void Complex_descriptor_is_invariant_to_reversal_mirroring_rotation_start_and_translation()
    {
        int n = 64;
        var z = Shape(n);
        double[] d = FourierDescriptors.FromComplex(z, false, out _);
        var variants = new[]
        {
            z.Reverse().ToArray(),
            z.Select(Complex.Conjugate).ToArray(),
            z.Select((_, j) => z[(j + 5) % n] * Complex.FromPolarCoordinates(1, 0.7) + new Complex(-30, 12)).ToArray(),
        };
        foreach (var v in variants) AssertPrefix(d, FourierDescriptors.FromComplex(v, false, out _), 10);
    }

    [Fact]
    public void Normalize_makes_descriptors_scale_invariant()
    {
        var a = FourierDescriptors.FromComplex(Shape(64), true, out _);
        var b = FourierDescriptors.FromComplex(Shape(64, 2.5), true, out _);
        Assert.Equal(1.0, a[0], 12);
        AssertPrefix(a, b, 10);

        double[] r = Enumerable.Range(0, 32).Select(j => 4 + Math.Cos(2 * Math.PI * j / 32)).ToArray();
        var ra = FourierDescriptors.FromReal(r, true, out double mean);
        var rb = FourierDescriptors.FromReal(r.Select(x => 3 * x).ToArray(), true, out _);
        Assert.Equal(4.0, mean, 9);
        Assert.Equal(0.25, ra[0], 9);
        AssertPrefix(ra, rb, 10);
    }

    [Fact]
    public void Negligible_scale_is_reported_as_zero_and_nothing_is_divided()
    {
        // A circle traced twice has c2 only: |c1| = 0.
        var twice = Enumerable.Range(0, 32).Select(j => Complex.FromPolarCoordinates(3, 4 * Math.PI * j / 32)).ToArray();
        var d = FourierDescriptors.FromComplex(twice, true, out double c1);
        Assert.Equal(0.0, c1);
        Assert.Equal(3.0, d.Max(), 9);
    }

    [Fact]
    public void Signed_area_is_positive_counter_clockwise()
    {
        var square = new[] { new Complex(0, 0), new Complex(2, 0), new Complex(2, 2), new Complex(0, 2) };
        Assert.Equal(4.0, FourierDescriptors.SignedArea(square), 12);
        Assert.Equal(-4.0, FourierDescriptors.SignedArea(square.Reverse().ToArray()), 12);
    }
}
