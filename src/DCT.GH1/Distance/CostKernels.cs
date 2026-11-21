using System;
using System.Threading.Tasks;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace DigitalCircularityToolkit.Distance
{
    /// <summary>
    /// Fast builders for the legacy integer cost trees (P1); the metrics themselves are in DigitalCircularityToolkit.Numerics.Metrics. They compute exactly the same values as the original
    /// per-item code (same formulas, same operation order); only the data handling changed: feature vectors are
    /// read once into flat arrays, rows are filled in parallel, and each row is appended to the tree in one call.
    /// </summary>
    internal static class CostKernels
    {
        /// <summary>One double[] per branch, in branch order.</summary>
        public static double[][] ToRows(GH_Structure<GH_Number> tree)
        {
            var rows = new double[tree.PathCount][];
            for (int i = 0; i < rows.Length; i++)
            {
                var branch = tree.Branches[i];
                var row = new double[branch.Count];
                for (int k = 0; k < row.Length; k++) row[k] = branch[k].Value;
                rows[i] = row;
            }
            return rows;
        }

        /// <summary>Builds a cost tree with one branch per demand row (path from <paramref name="pathOf"/>).</summary>
        public static GH_Structure<GH_Integer> BuildTree(int nDemand, int nSupply, Func<int, int, int> cost, Func<int, GH_Path> pathOf)
        {
            var rows = new GH_Integer[nDemand][];
            Parallel.For(0, nDemand, i =>
            {
                var row = new GH_Integer[nSupply];
                for (int j = 0; j < nSupply; j++) row[j] = new GH_Integer(cost(i, j));
                rows[i] = row;
            });

            var tree = new GH_Structure<GH_Integer>();
            for (int i = 0; i < nDemand; i++) tree.AppendRange(rows[i], pathOf(i));
            return tree;
        }
    }
}
