using System;
using System.Collections.Generic;
using System.Linq;
using DigitalCircularityToolkit.Numerics;
using Xunit;

public class AssignmentTests
{
    [Fact]
    public void AssignedCost_skips_unassigned_rows()
    {
        var cost = new int[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } };
        double total = Assignment.AssignedCost(cost, new[] { 1, -1, 0 }, out int unassigned);
        Assert.Equal(2 + 5, total);
        Assert.Equal(1, unassigned);
        Assignment.AssignedCost(cost, new[] { 7, 0, 1 }, out unassigned);   // out of range counts as unassigned
        Assert.Equal(1, unassigned);
    }

    static IEnumerable<int[]> Injections(int rows, int cols, int[] prefix)
    {
        if (prefix.Length == rows) { yield return prefix; yield break; }
        for (int c = 0; c < cols; c++)
            if (!prefix.Contains(c))
                foreach (var p in Injections(rows, cols, prefix.Append(c).ToArray())) yield return p;
    }

    [Theory]
    [InlineData(5, 5)]
    [InlineData(4, 6)]
    [InlineData(1, 3)]
    public void ShortestPathSolver_finds_the_optimum_found_by_brute_force(int rows, int cols)
    {
        var rng = new Random(rows * 31 + cols);
        for (int trial = 0; trial < 50; trial++)
        {
            var cost = new double[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    cost[i, j] = Math.Round(rng.NextDouble() * 20);   // many ties, like quantized lengths

            int[] x = new ShortestPathSolver().Solve(cost);
            Assert.Equal(rows, x.Distinct().Count());
            double solved = Enumerable.Range(0, rows).Sum(i => cost[i, x[i]]);
            double best = Injections(rows, cols, new int[0]).Min(p => Enumerable.Range(0, rows).Sum(i => cost[i, p[i]]));
            Assert.Equal(best, solved, 9);
        }
    }
}
