using System;
using System.Collections.Generic;
using System.Linq;
using DigitalCircularityToolkit.Numerics;
using Xunit;

public class LinearAssignmentTests
{
    /// <summary>Best (unassigned count, cost) over every partial injection rows → columns (−1 = unassigned).</summary>
    static (int unassigned, double cost) BruteForce(double[,] c)
    {
        int n = c.GetLength(0), m = c.GetLength(1);
        var best = (u: int.MaxValue, c: double.PositiveInfinity);
        var used = new bool[m];
        void Rec(int i, int u, double cost)
        {
            if (i == n)
            {
                if (u < best.u || (u == best.u && cost < best.c)) best = (u, cost);
                return;
            }
            Rec(i + 1, u + 1, cost);
            for (int j = 0; j < m; j++)
            {
                if (used[j] || double.IsPositiveInfinity(c[i, j])) continue;
                used[j] = true;
                Rec(i + 1, u, cost + c[i, j]);
                used[j] = false;
            }
        }
        Rec(0, 0, 0);
        return best;
    }

    static CostMatrix ToMatrix(double[,] c)
    {
        int n = c.GetLength(0), m = c.GetLength(1);
        var v = new double[n * m];
        for (int i = 0; i < n; i++) for (int j = 0; j < m; j++) v[i * m + j] = c[i, j];
        return new CostMatrix(n, m, v);
    }

    static void CheckValid(double[,] c, AssignmentResult r)
    {
        int n = c.GetLength(0);
        var cols = r.Assignment.Where(j => j >= 0).ToArray();
        Assert.Equal(cols.Length, cols.Distinct().Count());
        Assert.Equal(n - cols.Length, r.Unassigned);
        double sum = 0;
        for (int i = 0; i < n; i++)
            if (r.Assignment[i] >= 0)
            {
                Assert.False(double.IsPositiveInfinity(c[i, r.Assignment[i]]), "assigned a forbidden pair");
                sum += c[i, r.Assignment[i]];
            }
        Assert.Equal(sum, r.Cost, 9);
    }

    [Theory]
    [InlineData(5, 5, 0.0)]
    [InlineData(4, 6, 0.0)]
    [InlineData(6, 4, 0.0)]
    [InlineData(1, 3, 0.0)]
    [InlineData(3, 1, 0.0)]
    [InlineData(5, 5, 0.3)]
    [InlineData(4, 6, 0.5)]
    [InlineData(6, 4, 0.4)]
    [InlineData(5, 5, 0.9)]
    public void General_solver_matches_brute_force(int rows, int cols, double forbiddenShare)
    {
        var rng = new Random(rows * 131 + cols * 7 + (int)(forbiddenShare * 10));
        for (int trial = 0; trial < 60; trial++)
        {
            var c = new double[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    c[i, j] = rng.NextDouble() < forbiddenShare ? double.PositiveInfinity
                        : (trial % 2 == 0 ? Math.Round(rng.NextDouble() * 20) : rng.NextDouble() * 100 - 30); // ties; negatives

            var r = LinearAssignment.Solve(ToMatrix(c));
            CheckValid(c, r);
            var best = BruteForce(c);
            Assert.Equal(best.unassigned, r.Unassigned);
            Assert.Equal(best.cost, r.Cost, 9);
        }
    }

    [Fact]
    public void All_forbidden_row_is_unassigned_without_exception()
    {
        double inf = double.PositiveInfinity;
        var c = new double[,] { { inf, inf, inf }, { 1, 2, 3 }, { 3, 1, 2 } };
        var r = LinearAssignment.Solve(ToMatrix(c));
        Assert.Equal(-1, r.Assignment[0]);
        Assert.Equal(1, r.Unassigned);
        Assert.Equal(2, r.Cost, 12);
    }

    [Fact]
    public void Same_matrix_same_assignment()
    {
        var rng = new Random(3);
        var v = Enumerable.Range(0, 40 * 50).Select(_ => Math.Round(rng.NextDouble() * 5)).ToArray();
        var a = LinearAssignment.Solve(new CostMatrix(40, 50, v)).Assignment;
        var b = LinearAssignment.Solve(new CostMatrix(40, 50, v)).Assignment;
        Assert.Equal(a, b);
    }

    public static IEnumerable<object[]> MonotoneCases()
    {
        foreach (var (n, m) in new[] { (30, 30), (20, 35), (35, 20), (1, 5), (5, 1), (60, 60) })
            foreach (int metric in new[] { 0, 1, 2, 3, 4 })
                yield return new object[] { n, m, metric };
    }

    /// <summary>The monotone path gives the general solver's objective on 1-D problems (assignments may differ on ties).</summary>
    [Theory]
    [MemberData(nameof(MonotoneCases))]
    public void Monotone_path_equals_general_solver(int n, int m, int metric)
    {
        var rng = new Random(n * 1000 + m * 10 + metric);
        for (int trial = 0; trial < 20; trial++)
        {
            // lengths with many ties (quantized), like reclaimed members
            double[][] d = Enumerable.Range(0, n).Select(_ => new[] { Math.Round(rng.NextDouble() * 40) * 100 }).ToArray();
            double[][] s = Enumerable.Range(0, m).Select(_ => new[] { Math.Round(rng.NextDouble() * 40) * 100 }).ToArray();
            CostMatrix c = metric switch
            {
                0 => CostFunctions.EuclideanMatrix(d, s, 1),
                1 => CostFunctions.EuclideanMatrix(d, s, 2),
                2 => CostFunctions.ManhattanMatrix(d, s),
                3 => CostFunctions.AsymmetricEuclideanMatrix(d, s, 1, 3, false),
                _ => CostFunctions.AsymmetricEuclideanMatrix(d, s, 1, 2, true),
            };
            Assert.NotNull(c.Monotone);
            var fast = LinearAssignment.Solve(c);
            var general = LinearAssignment.Solve(c, allowMonotonePath: false);
            Assert.True(fast.UsedMonotonePath);
            Assert.Equal(general.Unassigned, fast.Unassigned);
            Assert.Equal(general.Cost, fast.Cost, 6);
            Assert.Equal(general.Cost == 0 ? 0 : 1.0, general.Cost == 0 ? 0 : fast.Cost / general.Cost, 12);
            // the fast path's cost is the matrix's cost for its assignment
            double sum = 0;
            for (int i = 0; i < n; i++) if (fast.Assignment[i] >= 0) sum += c[i, fast.Assignment[i]];
            Assert.Equal(sum, fast.Cost);
        }
    }

    [Fact]
    public void Monotone_flag_only_for_one_feature_and_convex_costs()
    {
        double[][] d1 = { new[] { 1.0 } }, s1 = { new[] { 2.0 } };
        double[][] d2 = { new[] { 1.0, 2.0 } }, s2 = { new[] { 2.0, 3.0 } };
        Assert.NotNull(CostFunctions.EuclideanMatrix(d1, s1, 1).Monotone);
        Assert.Null(CostFunctions.EuclideanMatrix(d1, s1, 0.5).Monotone);
        Assert.Null(CostFunctions.EuclideanMatrix(d2, s2, 1).Monotone);
        Assert.Null(CostFunctions.AsymmetricEuclideanMatrix(d1, s1, 1, 0.5, false).Monotone);
        Assert.NotNull(CostFunctions.AsymmetricEuclideanMatrix(d1, s1, 1, 0.5, true).Monotone);
    }

    [Fact]
    public void Large_one_dimensional_problem_is_fast()
    {
        var rng = new Random(9);
        double[][] d = Enumerable.Range(0, 4000).Select(_ => new[] { Math.Round(rng.NextDouble() * 400) * 10 }).ToArray();
        double[][] s = Enumerable.Range(0, 4000).Select(_ => new[] { Math.Round(rng.NextDouble() * 400) * 10 }).ToArray();
        var c = CostFunctions.AsymmetricEuclideanMatrix(d, s, 1, 2, false);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var r = LinearAssignment.Solve(c);
        sw.Stop();
        Assert.True(r.UsedMonotonePath);
        Assert.True(sw.ElapsedMilliseconds < 200, $"monotone path took {sw.ElapsedMilliseconds} ms");
    }
}
