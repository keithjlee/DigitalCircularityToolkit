using System;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Distance
{
    public static class Hausdorff
    {
        /// <summary>
        /// Hausdorff distance max(h(A,B), h(B,A)), where h(X,Y) is the largest distance from a point of X to its
        /// nearest point of Y. Symmetric (B6: only h(A,B) was computed before). Both sets must be non-empty.
        /// </summary>
        public static double ComputeDistance(Point3d[] A, Point3d[] B)
        {
            return Math.Max(Directed(A, B), Directed(B, A));
        }

        /// <summary>Directed distance h(A,B) = max over a in A of min over b in B of |a - b|.</summary>
        public static double Directed(Point3d[] A, Point3d[] B)
        {
            double maxDist = 0;
            foreach (Point3d a in A)
            {
                double minDist = double.PositiveInfinity;
                foreach (Point3d b in B)
                {
                    double dist = a.DistanceTo(b);
                    if (dist < minDist)
                    {
                        minDist = dist;
                    }
                }
                if (minDist > maxDist)
                {
                    maxDist = minDist;
                }
            }
            return maxDist;
        }
    }
}
