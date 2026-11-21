using System;

namespace DigitalCircularityToolkit.Numerics
{
    /// <summary>
    /// Distances between two point sequences, given only the pairwise point distance <c>dist(i, j)</c> (i indexes the
    /// first sequence, j the second). The Rhino components pass Point3d.DistanceTo; tests pass anything.
    /// </summary>
    public static class SequenceDistances
    {
        /// <summary>
        /// Discrete Fréchet distance (n, m ≥ 1). Iterative dynamic programme with two rows of memory (B10: the former
        /// memoized recursion was n+m deep and could overflow the stack).
        /// </summary>
        public static double DiscreteFrechet(int n, int m, Func<int, int, double> dist)
        {
            var prev = new double[m];
            var curr = new double[m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    double d = dist(i, j);
                    if (i == 0 && j == 0)
                        curr[j] = d;
                    else if (i > 0 && j == 0)
                        curr[j] = Math.Max(prev[0], d);
                    else if (i == 0)
                        curr[j] = Math.Max(curr[j - 1], d);
                    else
                        curr[j] = Math.Max(Math.Min(prev[j], Math.Min(prev[j - 1], curr[j - 1])), d);
                }
                var t = prev; prev = curr; curr = t;
            }
            return prev[m - 1];
        }

        /// <summary>Directed Hausdorff distance h(A,B) = max over i of min over j of dist(i, j) (n, m ≥ 1).</summary>
        public static double DirectedHausdorff(int n, int m, Func<int, int, double> dist)
        {
            double maxDist = 0;
            for (int i = 0; i < n; i++)
            {
                double minDist = double.PositiveInfinity;
                for (int j = 0; j < m; j++)
                {
                    double d = dist(i, j);
                    if (d < minDist) minDist = d;
                }
                if (minDist > maxDist) maxDist = minDist;
            }
            return maxDist;
        }

        /// <summary>Symmetric Hausdorff distance max(h(A,B), h(B,A)) (B6).</summary>
        public static double Hausdorff(int n, int m, Func<int, int, double> dist)
        {
            return Math.Max(DirectedHausdorff(n, m, dist), DirectedHausdorff(m, n, (j, i) => dist(i, j)));
        }
    }
}
