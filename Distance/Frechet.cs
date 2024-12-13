using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace DigitalCircularityToolkit{

/// <summary>
/// Frechet distance for similarity between curves.
/// This is adapted from: https://github.com/Thought-Weaver/Frechet-Distance (MIT LICENSE)
/// Only changes are that we can use built in data structures/methods for points and distances.
/// </summary>
public static class Frechet
{

    public static double ComputeDistance(double[,] distances, int i, int j, Point3d[] A, Point3d[] B){

        if (distances[i, j] > -1)
        {
            return distances[i, j];
        }
        else if (i == 0 && j == 0)
        {
            distances[i, j] = A[i].DistanceTo(B[j]);
        }
        else if (i > 0 && j == 0)
        {
            distances[i, j] = Math.Max(ComputeDistance(distances, i - 1, 0, A, B), A[i].DistanceTo(B[j]));
        }
        else if (i == 0 && j > 0)
        {
            distances[i, j] = Math.Max(ComputeDistance(distances, 0, j - 1, A, B), A[i].DistanceTo(B[j]));
        }
        else if (i > 0 && j > 0)
        {
            double d = A[i].DistanceTo(B[j]);
            distances[i, j] = Math.Max(Math.Min(ComputeDistance(distances, i - 1, j, A, B), Math.Min(ComputeDistance(distances, i - 1, j - 1, A, B), ComputeDistance(distances, i, j - 1, A, B))), d);
        }
        else
        {
            distances[i, j] = double.PositiveInfinity;
        }

        return distances[i, j];

    }
    public static double FrechetDistance(Point3d[] A, Point3d[] B){
        double [,] distances = new double[A.Length, B.Length];

        //initialize all distances to -1
        for (int i = 0; i < A.Length; i++)
        {
            for (int j = 0; j < B.Length; j++)
            {
                distances[i, j] = -1;
            }
        }

        return ComputeDistance(distances, A.Length - 1, B.Length - 1, A, B);

    }
}
}