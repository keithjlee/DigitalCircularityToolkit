using System;
using System.Threading.Tasks;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace DigitalCircularityToolkit.Distance
{
    /// <summary>
    /// Fast builders for the legacy integer cost trees (P1). They compute exactly the same values as the original
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

        /// <summary>
        /// Null when every vector in both sets has the same length; otherwise an error message.
        /// (The old check only compared branch 0 and continued after its Error.)
        /// </summary>
        public static string CheckDimensions(double[][] demand, double[][] supply)
        {
            if (demand.Length == 0 || supply.Length == 0) return "Demand and supply must each contain at least one feature vector";
            int k = demand[0].Length;
            for (int i = 0; i < demand.Length; i++)
                if (demand[i].Length != k)
                    return string.Format("Demand feature vector {0} has {1} values; vector 0 has {2}", i, demand[i].Length, k);
            for (int j = 0; j < supply.Length; j++)
                if (supply[j].Length != k)
                    return string.Format("Supply feature vector {0} has {1} values, but demand vectors have {2}. Demand and supply feature vectors must be of same dimension", j, supply[j].Length, k);
            return null;
        }

        /// <summary>
        /// double → int as .NET 9+ on arm64 does it: saturating, NaN → 0 (B3). On x64 and .NET Framework a plain
        /// cast turns overflow into int.MinValue, i.e. the cheapest possible cost.
        /// </summary>
        public static int SaturatingInt(double x)
        {
            if (double.IsNaN(x)) return 0;
            if (x >= int.MaxValue) return int.MaxValue;
            if (x <= int.MinValue) return int.MinValue;
            return (int)x;
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

        // ---- metrics: identical arithmetic to Euclidean.EuclideanDist / AsymmEuclideanDist (Math.Pow(x, 2) kept on purpose)

        public static double EuclideanDist(double[] x1, double[] x2)
        {
            double dist = 0;
            for (int i = 0; i < x1.Length; i++)
            {
                dist += Math.Pow(x1[i] - x2[i], 2);
            }
            return Math.Sqrt(dist);
        }

        public static double AsymmEuclideanDist(double[] source, double[] target, int p, int q)
        {
            double dist = 0;
            bool valid = true;
            for (int i = 0; i < source.Length; i++)
            {
                double diff = target[i] - source[i];
                if (diff < 0) valid = false;
                dist += Math.Pow(diff, 2);
            }
            return valid ? Math.Pow(Math.Sqrt(dist), p) : Math.Pow(Math.Sqrt(dist), q);
        }

        public static double ManhattanDist(double[] x1, double[] x2)
        {
            double dist = 0;
            for (int k = 0; k < x1.Length; k++)
            {
                dist += Math.Abs(x1[k] - x2[k]);
            }
            return dist;
        }
    }
}
