using System;
using System.Collections.Generic;

namespace DigitalCircularityToolkit.Numerics
{
    /// <summary>
    /// 2D convex hull (Andrew's monotone chain, O(n log n)). Replaces Grasshopper's ConvexHull.Solver so the geometry
    /// layer needs no Grasshopper reference (P2.5). Counter-clockwise; duplicate and (nearly) collinear points dropped.
    /// <para>
    /// Two passes: the chain uses the exact orientation test, which never drops an extreme point; then vertices that
    /// are collinear with their neighbours within sin 1e-10 are removed from the finished polygon. (Applying a tolerance
    /// inside the chain dropped real corners when noisy points along a near-vertical edge sort out of order.)
    /// Grasshopper's solver returned self-overlapping polylines on such noisy sample sets (B17).
    /// </para>
    /// </summary>
    public static class ConvexHull2D
    {
        /// <summary>Relative collinearity tolerance (sine of the turn angle) for the simplification pass.</summary>
        public const double CollinearTolerance = 1e-10;

        /// <summary>
        /// Indices of the hull vertices in counter-clockwise order, without repeating the first one. Fewer than 3
        /// distinct points give the distinct extreme points (0, 1 or 2 of them).
        /// </summary>
        public static int[] Indices(IReadOnlyList<double> x, IReadOnlyList<double> y)
        {
            int n = x.Count;
            if (y.Count != n) throw new ArgumentException("x and y must have the same length");
            var order = new int[n];
            for (int i = 0; i < n; i++) order[i] = i;
            Array.Sort(order, (a, b) => x[a] != x[b] ? x[a].CompareTo(x[b]) : y[a].CompareTo(y[b]));

            // Monotone chain with the exact test: cross <= 0 drops clockwise turns, exactly collinear points and duplicates.
            var hull = new int[2 * n + 1];
            int k = 0;
            for (int s = 0; s < n; s++)
            {
                while (k >= 2 && Cross(x, y, hull[k - 2], hull[k - 1], order[s]) <= 0) k--;
                hull[k++] = order[s];
            }
            for (int s = n - 2, lower = k + 1; s >= 0; s--)
            {
                while (k >= lower && Cross(x, y, hull[k - 2], hull[k - 1], order[s]) <= 0) k--;
                hull[k++] = order[s];
            }
            if (k > 1) k--;   // the upper chain ends at the start point again

            var poly = new List<int>(k);
            for (int i = 0; i < k; i++)
                if (poly.Count == 0 || x[poly[poly.Count - 1]] != x[hull[i]] || y[poly[poly.Count - 1]] != y[hull[i]])
                    poly.Add(hull[i]);
            if (poly.Count > 1 && x[poly[0]] == x[poly[poly.Count - 1]] && y[poly[0]] == y[poly[poly.Count - 1]])
                poly.RemoveAt(poly.Count - 1);

            RemoveNearlyCollinear(x, y, poly);
            return poly.ToArray();
        }

        // Drop vertices whose neighbours continue straight through them (|sin| <= tolerance, same direction).
        private static void RemoveNearlyCollinear(IReadOnlyList<double> x, IReadOnlyList<double> y, List<int> poly)
        {
            bool changed = true;
            while (changed && poly.Count > 3)
            {
                changed = false;
                for (int i = 0; i < poly.Count; i++)
                {
                    int u = poly[(i + poly.Count - 1) % poly.Count], v = poly[i], w = poly[(i + 1) % poly.Count];
                    double ax = x[v] - x[u], ay = y[v] - y[u], bx = x[w] - x[v], by = y[w] - y[v];
                    double cross = ax * by - ay * bx;
                    double scale = Math.Sqrt((ax * ax + ay * ay) * (bx * bx + by * by));
                    if (Math.Abs(cross) <= CollinearTolerance * scale && ax * bx + ay * by > 0)
                    {
                        poly.RemoveAt(i);
                        changed = true;
                        break;
                    }
                }
            }
        }

        private static double Cross(IReadOnlyList<double> x, IReadOnlyList<double> y, int o, int a, int b)
        {
            return (x[a] - x[o]) * (y[b] - y[o]) - (y[a] - y[o]) * (x[b] - x[o]);
        }
    }
}
