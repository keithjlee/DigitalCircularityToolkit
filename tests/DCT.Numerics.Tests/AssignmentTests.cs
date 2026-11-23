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
}
