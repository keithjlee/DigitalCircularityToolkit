using System;
using System.Collections.Generic;
using System.Linq;
using DigitalCircularityToolkit.Numerics;
using Xunit;

public class OrientationMathTests
{
    /// <summary>A w×h rectangle in the XY plane meshed as an n×m grid of triangles; vertices denser near x = 0 when skewed.</summary>
    static SecondMoments PlateMoments(double w, double h, int n, int m, bool skewed)
    {
        var mo = new SecondMoments();
        double X(int i) { double t = (double)i / n; return w * (skewed ? t * t * t : t); }
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
            {
                double x0 = X(i), x1 = X(i + 1), y0 = h * j / m, y1 = h * (j + 1) / m;
                mo.AddTriangle(x0, y0, 0, x1, y0, 0, x1, y1, 0);
                mo.AddTriangle(x0, y0, 0, x1, y1, 0, x0, y1, 0);
            }
        return mo;
    }

    [Fact]
    public void Triangle_moments_do_not_depend_on_tessellation()
    {
        var coarse = PlateMoments(3, 1, 1, 1, false).Covariance();
        var fine = PlateMoments(3, 1, 40, 13, true).Covariance();
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                Assert.Equal(coarse[i, j], fine[i, j], 9);
        // uniform plate: var(x) = w²/12
        Assert.Equal(9.0 / 12, coarse[0, 0], 12);
        Assert.Equal(1.0 / 12, coarse[1, 1], 12);
    }

    [Fact]
    public void Vertex_PCA_is_biased_by_dense_meshing_but_area_moments_are_not()
    {
        // a 1.2 × 1 plate meshed much more densely along one strip: vertex covariance is pulled, area covariance is exact
        var mo = PlateMoments(1.2, 1, 60, 4, true);
        mo.Principal(out _, out double[][] v);
        Assert.Equal(1.0, Math.Abs(v[0][0]), 9);   // long axis = x
        var verts = new SecondMoments();
        for (int i = 0; i <= 60; i++)
            for (int j = 0; j <= 4; j++)
                verts.AddPoint(1.2 * Math.Pow(i / 60.0, 3), j / 4.0, 0);
        verts.Principal(out _, out double[][] vv);
        Assert.True(Math.Abs(vv[0][0]) < 0.5, "vertex PCA should be pulled off the long axis");
    }

    [Fact]
    public void Segment_and_triangle_formulas_match_fine_sampling()
    {
        var seg = new SecondMoments();
        seg.AddSegment(1, 2, 3, 4, -1, 5);
        var pts = new SecondMoments();
        int n = 20000;
        for (int k = 0; k < n; k++)
        {
            double t = (k + 0.5) / n;
            pts.AddPoint(1 + 3 * t, 2 - 3 * t, 3 + 2 * t);
        }
        AssertClose(seg.Covariance(), pts.Covariance(), 1e-6);

        var tri = new SecondMoments();
        tri.AddTriangle(0, 0, 0, 2, 0, 1, 0.5, 3, -1);
        var tp = new SecondMoments();
        var rng = new Random(1);
        for (int k = 0; k < 400000; k++)
        {
            double r1 = rng.NextDouble(), r2 = rng.NextDouble();
            if (r1 + r2 > 1) { r1 = 1 - r1; r2 = 1 - r2; }
            tp.AddPoint(2 * r1 + 0.5 * r2, 3 * r2, r1 - r2);
        }
        AssertClose(tri.Covariance(), tp.Covariance(), 1e-2);
    }

    [Fact]
    public void Signed_fan_gives_the_region_moments_of_a_nonconvex_polygon()
    {
        // L-shape = 2×1 bar + 1×1 square on top-left; as a signed fan from vertex 0, and as positive triangles
        double[][] p = { new[] { 0.0, 0 }, new[] { 2.0, 0 }, new[] { 2.0, 1 }, new[] { 1.0, 1 }, new[] { 1.0, 2 }, new[] { 0.0, 2 } };
        var fan = new SecondMoments();
        for (int i = 1; i + 1 < p.Length; i++)
        {
            double area = 0.5 * ((p[i][0] - p[0][0]) * (p[i + 1][1] - p[0][1]) - (p[i][1] - p[0][1]) * (p[i + 1][0] - p[0][0]));
            fan.AddTriangle(p[0][0], p[0][1], 0, p[i][0], p[i][1], 0, p[i + 1][0], p[i + 1][1], 0, area);
        }
        var parts = new SecondMoments();
        void Quad(double x0, double y0, double x1, double y1)
        {
            parts.AddTriangle(x0, y0, 0, x1, y0, 0, x1, y1, 0);
            parts.AddTriangle(x0, y0, 0, x1, y1, 0, x0, y1, 0);
        }
        Quad(0, 0, 2, 1);
        Quad(0, 1, 1, 2);
        Assert.Equal(3.0, fan.Weight, 12);
        AssertClose(fan.Covariance(), parts.Covariance(), 1e-12);
    }

    static void AssertClose(double[,] a, double[,] b, double tol)
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                Assert.True(Math.Abs(a[i, j] - b[i, j]) <= tol * Math.Max(1, Math.Abs(a[i, j])), $"[{i},{j}] {a[i, j]} vs {b[i, j]}");
    }

    // ---------------------------------------------------------------- minimum-area rectangle

    static (double[] x, double[] y) Rotated(IEnumerable<(double x, double y)> pts, double angle, double ox = 5, double oy = -3)
    {
        double c = Math.Cos(angle), s = Math.Sin(angle);
        var list = pts.ToList();
        return (list.Select(p => ox + c * p.x - s * p.y).ToArray(), list.Select(p => oy + s * p.x + c * p.y).ToArray());
    }

    static IEnumerable<(double, double)> RectanglePoints(double l, double w, int perSide)
    {
        for (int k = 0; k <= perSide; k++)
        {
            double t = (double)k / perSide;
            yield return (t * l, 0); yield return (t * l, w); yield return (0, t * w); yield return (l, t * w);
        }
    }

    [Theory]
    [InlineData(4.0, 1.0, 0.3)]
    [InlineData(4.0, 1.0, 2.0)]
    [InlineData(1.0, 1.01, 0.7)]
    [InlineData(10.0, 6.0, -1.1)]
    public void Rectangle_is_found_with_its_long_side(double l, double w, double angle)
    {
        var (x, y) = Rotated(RectanglePoints(l, w, 7), angle);
        var r = MinAreaRectangle.Compute(x, y);
        Assert.Equal(Math.Max(l, w), r.Length, 9);
        Assert.Equal(Math.Min(l, w), r.Width, 9);
        double longAngle = l >= w ? angle : angle + Math.PI / 2;
        double cross = Math.Abs(r.Ux * Math.Sin(longAngle) - r.Uy * Math.Cos(longAngle));
        Assert.True(cross < 1e-9, $"direction off by {Math.Asin(cross)} rad");
        Assert.True(r.Uy >= 0);
    }

    [Fact]
    public void Square_is_aligned_with_a_side_whatever_the_sampling()
    {
        var rng = new Random(3);
        var pts = Enumerable.Range(0, 500).Select(_ => (rng.NextDouble(), rng.NextDouble())).Concat(RectanglePoints(1, 1, 3));
        var (x, y) = Rotated(pts, 0.4);
        var r = MinAreaRectangle.Compute(x, y);
        double a = Math.Atan2(r.Uy, r.Ux) % (Math.PI / 2);
        Assert.True(Math.Abs(a - 0.4) < 1e-9 || Math.Abs(a - 0.4 + Math.PI / 2) < 1e-9, $"angle {a}");
        Assert.Equal(1.0, r.Length, 9);
    }

    [Fact]
    public void Result_is_never_larger_than_a_fine_angle_sweep()
    {
        var rng = new Random(11);
        for (int trial = 0; trial < 30; trial++)
        {
            int n = 5 + rng.Next(60);
            double[] x = Enumerable.Range(0, n).Select(_ => rng.NextDouble() * 10).ToArray();
            double[] y = Enumerable.Range(0, n).Select(_ => rng.NextDouble() * 4).ToArray();
            var r = MinAreaRectangle.Compute(x, y);
            for (int k = 0; k < 3600; k++)
            {
                double t = Math.PI * k / 3600, c = Math.Cos(t), s = Math.Sin(t);
                var u = x.Zip(y, (a, b) => a * c + b * s).ToArray();
                var v = x.Zip(y, (a, b) => -a * s + b * c).ToArray();
                double area = (u.Max() - u.Min()) * (v.Max() - v.Min());
                Assert.True(r.Area <= area * (1 + 1e-9), $"trial {trial}: {r.Area} > {area} at {t}");
            }
        }
    }
}
