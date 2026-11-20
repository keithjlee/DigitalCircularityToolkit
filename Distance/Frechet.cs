using System;
using Rhino.Geometry;

namespace DigitalCircularityToolkit
{
    public static class Frechet
    {
        /// <summary>
        /// Discrete Fréchet distance between two point sequences (both non-empty).
        /// Iterative dynamic programme, two rows of memory (B10: the former memoized recursion was n+m deep and
        /// could overflow the stack and crash Rhino). Same recurrence and operation order, so values are identical.
        /// </summary>
        public static double FrechetDistance(Point3d[] A, Point3d[] B)
        {
            int n = A.Length, m = B.Length;
            var prev = new double[m];
            var curr = new double[m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    double d = A[i].DistanceTo(B[j]);
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
    }
}
