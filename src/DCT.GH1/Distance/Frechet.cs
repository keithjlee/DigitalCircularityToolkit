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
            return Numerics.SequenceDistances.DiscreteFrechet(A.Length, B.Length, (i, j) => A[i].DistanceTo(B[j]));
        }
    }
}
