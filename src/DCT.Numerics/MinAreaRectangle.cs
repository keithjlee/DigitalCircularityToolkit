using System;
using System.Collections.Generic;

namespace DigitalCircularityToolkit.Numerics
{
    /// <summary>The minimum-area rectangle around a 2D point set: its side directions and side lengths.</summary>
    /// <param name="Ux">Unit direction of the longer side (Length ≥ Width), with Uy; the other side is (−Uy, Ux).</param>
    public readonly record struct Rectangle2D(double Ux, double Uy, double Length, double Width)
    {
        public double Area => Length * Width;
    }

    /// <summary>
    /// Minimum-area enclosing rectangle (ALIGNMENT_SPEC §3–4, MinBox mode). One side of an optimal rectangle is flush
    /// with an edge of the convex hull, so every hull edge direction is tried. O(h²) on the h hull vertices, which is
    /// a few milliseconds even for round shapes with thousands of hull vertices.
    /// </summary>
    public static class MinAreaRectangle
    {
        /// <summary>
        /// The rectangle with the longer side as <see cref="Rectangle2D.Ux"/>/<see cref="Rectangle2D.Uy"/>. Deterministic:
        /// among rectangles of equal area (within 1e-9 relative), the one whose long side has the smallest angle in
        /// [0, π) from +x wins. Fewer than 3 distinct hull points: the direction of the points' spread.
        /// </summary>
        public static Rectangle2D Compute(IReadOnlyList<double> x, IReadOnlyList<double> y)
        {
            int[] hull = ConvexHull2D.Indices(x, y);
            if (hull.Length == 0) return new Rectangle2D(1, 0, 0, 0);
            if (hull.Length < 3)
            {
                if (hull.Length == 1) return new Rectangle2D(1, 0, 0, 0);
                double dx = x[hull[1]] - x[hull[0]], dy = y[hull[1]] - y[hull[0]];
                double l = Math.Sqrt(dx * dx + dy * dy);
                return Canonical(dx / l, dy / l, l, 0);
            }

            Rectangle2D best = default;
            double bestArea = double.PositiveInfinity, bestAngle = double.PositiveInfinity;
            int h = hull.Length;
            for (int e = 0; e < h; e++)
            {
                int a = hull[e], b = hull[(e + 1) % h];
                double ux = x[b] - x[a], uy = y[b] - y[a];
                double len = Math.Sqrt(ux * ux + uy * uy);
                if (len == 0) continue;
                ux /= len; uy /= len;
                double minU = double.PositiveInfinity, maxU = double.NegativeInfinity;
                double minV = double.PositiveInfinity, maxV = double.NegativeInfinity;
                foreach (int k in hull)
                {
                    double pu = x[k] * ux + y[k] * uy, pv = -x[k] * uy + y[k] * ux;
                    if (pu < minU) minU = pu;
                    if (pu > maxU) maxU = pu;
                    if (pv < minV) minV = pv;
                    if (pv > maxV) maxV = pv;
                }
                var r = Canonical(ux, uy, maxU - minU, maxV - minV);
                double area = r.Area;
                double angle = Math.Atan2(r.Uy, r.Ux);
                double tol = 1e-9 * Math.Max(area, bestArea == double.PositiveInfinity ? area : bestArea);
                if (area < bestArea - tol || (Math.Abs(area - bestArea) <= tol && angle < bestAngle))
                {
                    best = r;
                    bestArea = area;
                    bestAngle = angle;
                }
            }
            return best;
        }

        /// <summary>Longer side first; its direction pointed into the upper half plane (angle in [0, π)).</summary>
        private static Rectangle2D Canonical(double ux, double uy, double lu, double lv)
        {
            if (lv > lu)
            {
                (ux, uy) = (-uy, ux);
                (lu, lv) = (lv, lu);
            }
            if (uy < 0 || (uy == 0 && ux < 0)) { ux = -ux; uy = -uy; }
            return new Rectangle2D(ux, uy, lu, lv);
        }
    }
}
