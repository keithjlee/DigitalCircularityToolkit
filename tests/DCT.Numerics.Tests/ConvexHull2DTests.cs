using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using DigitalCircularityToolkit.Numerics;
using Xunit;

public class ConvexHull2DTests
{
    static double SignedArea(double[] x, double[] y, int[] idx)
    {
        double a = 0;
        for (int i = 0; i < idx.Length; i++)
        {
            int p = idx[i], q = idx[(i + 1) % idx.Length];
            a += x[p] * y[q] - x[q] * y[p];
        }
        return a / 2;
    }

    [Fact]
    public void Matches_Grasshoppers_hull_vertex_set_and_orientation()
    {
        // data/grasshopper_hulls.json: Grasshopper ConvexHull.Solver.ComputeHull outputs recorded in Rhino 8
        // (random sets, duplicates, collinear points, a grid, integer ties, a circle).
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "grasshopper_hulls.json")));
        int cases = 0;
        foreach (var c in doc.RootElement.EnumerateArray())
        {
            var pts = c.GetProperty("points").EnumerateArray().Select(p => (p[0].GetDouble(), p[1].GetDouble())).ToArray();
            var ghClosed = c.GetProperty("hull").EnumerateArray().Select(p => (p[0].GetDouble(), p[1].GetDouble())).ToArray();
            var gh = ghClosed.Take(ghClosed.Length - 1).ToArray();   // GH repeats the first vertex at the end

            double[] x = pts.Select(p => p.Item1).ToArray(), y = pts.Select(p => p.Item2).ToArray();
            int[] idx = ConvexHull2D.Indices(x, y);
            var ours = idx.Select(i => (x[i], y[i])).ToArray();

            string name = c.GetProperty("name").GetString()!;
            Assert.True(gh.OrderBy(p => p).SequenceEqual(ours.OrderBy(p => p)), name + ": vertex sets differ");
            if (ours.Length >= 3)
            {
                Assert.True(SignedArea(x, y, idx) > 0, name + ": not counter-clockwise");
                // same cyclic order as Grasshopper: rotate ours to GH's start vertex
                int shift = Array.IndexOf(ours, gh[0]);
                Assert.True(gh.SequenceEqual(ours.Skip(shift).Concat(ours.Take(shift))), name + ": cyclic order differs");
            }
            cases++;
        }
        Assert.True(cases >= 30);
    }

    [Fact]
    public void Noisy_points_along_edges_do_not_add_vertices()
    {
        // A 1000 x 1000 square rotated by 45 degrees, with 50 samples along each edge: rotating the coordinates adds
        // rounding noise (B17: Grasshopper's solver returned a self-overlapping hull here).
        var xs = new System.Collections.Generic.List<double>(); var ys = new System.Collections.Generic.List<double>();
        double c = Math.Cos(Math.PI / 4), s = Math.Sin(Math.PI / 4);
        for (int e = 0; e < 4; e++)
            for (int i = 0; i < 50; i++)
            {
                double t = i / 50.0 * 1000, px, py;
                switch (e) { case 0: px = t; py = 0; break; case 1: px = 1000; py = t; break; case 2: px = 1000 - t; py = 1000; break; default: px = 0; py = 1000 - t; break; }
                xs.Add(c * px - s * py); ys.Add(s * px + c * py);
            }
        int[] idx = ConvexHull2D.Indices(xs, ys);
        Assert.Equal(4, idx.Length);
        double perimeter = 0;
        for (int i = 0; i < idx.Length; i++)
        {
            int p = idx[i], q = idx[(i + 1) % idx.Length];
            perimeter += Math.Sqrt(Math.Pow(xs[q] - xs[p], 2) + Math.Pow(ys[q] - ys[p], 2));
        }
        Assert.Equal(4000.0, perimeter, 6);
    }

    [Fact]
    public void Real_DCT_sample_sets_contain_every_point_with_few_vertices()
    {
        // data/dct_sample_sets.json: Brep box, square slab and cylinder samples projected onto their local plane, recorded
        // in Rhino. Rounding noise along edges previously made Grasshopper's hull overlap itself (B17).
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "dct_sample_sets.json")));
        foreach (var c in doc.RootElement.EnumerateArray())
        {
            var pts = c.GetProperty("xy").EnumerateArray().Select(p => (p[0].GetDouble(), p[1].GetDouble())).ToArray();
            double[] x = pts.Select(p => p.Item1).ToArray(), y = pts.Select(p => p.Item2).ToArray();
            int[] idx = ConvexHull2D.Indices(x, y);
            string name = c.GetProperty("name").GetString()!;
            Assert.InRange(idx.Length, 4, 6);
            Assert.True(SignedArea(x, y, idx) > 0, name);
            for (int i = 0; i < x.Length; i++)
                for (int e = 0; e < idx.Length; e++)
                {
                    int o = idx[e], a = idx[(e + 1) % idx.Length];
                    double cross = (x[a] - x[o]) * (y[i] - y[o]) - (y[a] - y[o]) * (x[i] - x[o]);
                    double len = Math.Sqrt(Math.Pow(x[a] - x[o], 2) + Math.Pow(y[a] - y[o], 2));
                    Assert.True(cross >= -1e-6 * len, name + ": sample " + i + " outside edge " + e);
                }
        }
    }

    [Fact]
    public void Random_point_sets_are_contained_and_convex()
    {
        var rng = new Random(9);
        for (int t = 0; t < 300; t++)
        {
            int n = rng.Next(3, 200);
            var x = new double[n]; var y = new double[n];
            for (int i = 0; i < n; i++) { x[i] = Math.Round(rng.NextDouble() * 20, rng.Next(0, 3)); y[i] = Math.Round(rng.NextDouble() * 20, rng.Next(0, 3)); }
            int[] idx = ConvexHull2D.Indices(x, y);
            if (idx.Length < 3) continue;
            for (int e = 0; e < idx.Length; e++)
            {
                int o = idx[e], a = idx[(e + 1) % idx.Length], b = idx[(e + 2) % idx.Length];
                Assert.True((x[a] - x[o]) * (y[b] - y[o]) - (y[a] - y[o]) * (x[b] - x[o]) > 0, "not strictly convex");
                for (int i = 0; i < n; i++)
                    Assert.True((x[a] - x[o]) * (y[i] - y[o]) - (y[a] - y[o]) * (x[i] - x[o]) >= -1e-9, "point outside");
            }
        }
    }

    [Fact]
    public void Degenerate_inputs()
    {
        Assert.Empty(ConvexHull2D.Indices(new double[0], new double[0]));
        Assert.Single(ConvexHull2D.Indices(new[] { 1.0, 1.0 }, new[] { 2.0, 2.0 }));
        Assert.Equal(2, ConvexHull2D.Indices(new[] { 0.0, 1.0, 2.0 }, new[] { 0.0, 1.0, 2.0 }).Length);
    }
}
