using System;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Distance
{
    public static class Hausdorff
    {
        // Add your methods here
        public static double ComputeDistance(Point3d[] A, Point3d[] B)
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