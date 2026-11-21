using System;

namespace DigitalCircularityToolkit.Numerics
{
    /// <summary>
    /// Distances between feature vectors, as used by the cost-matrix components. The arithmetic is kept exactly as the
    /// original per-item code (<c>Math.Pow(x, 2)</c> on purpose) so the integer cost matrices do not change.
    /// </summary>
    public static class Metrics
    {
        /// <summary>Euclidean distance ‖x1 − x2‖₂. Both vectors must have the same length.</summary>
        public static double Euclidean(double[] x1, double[] x2)
        {
            double dist = 0;
            for (int i = 0; i < x1.Length; i++)
            {
                dist += Math.Pow(x1[i] - x2[i], 2);
            }
            return Math.Sqrt(dist);
        }

        /// <summary>
        /// Asymmetric Euclidean cost from a demand (<paramref name="source"/>) to a supply (<paramref name="target"/>):
        /// ‖target − source‖^p when target ≥ source in every feature ("valid": the supply can be cut down), otherwise
        /// ‖target − source‖^q.
        /// </summary>
        public static double AsymmetricEuclidean(double[] source, double[] target, int p, int q)
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

        /// <summary>Manhattan distance Σ |x1 − x2|.</summary>
        public static double Manhattan(double[] x1, double[] x2)
        {
            double dist = 0;
            for (int k = 0; k < x1.Length; k++)
            {
                dist += Math.Abs(x1[k] - x2[k]);
            }
            return dist;
        }

        /// <summary>
        /// double → int as .NET 9+ on arm64 does it: saturating, NaN → 0 (B3). On x64 a plain cast turns overflow into
        /// int.MinValue, i.e. the cheapest possible cost.
        /// </summary>
        public static int SaturatingInt(double x)
        {
            if (double.IsNaN(x)) return 0;
            if (x >= int.MaxValue) return int.MaxValue;
            if (x <= int.MinValue) return int.MinValue;
            return (int)x;
        }

        /// <summary>Null when every vector in both sets has the same length; otherwise an error message.</summary>
        public static string? CheckDimensions(double[][] demand, double[][] supply)
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
    }
}
