using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Accord.Statistics;
using Accord;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression.Linear;

namespace DigitalCircularityToolkit.Characterization
{
    /// <summary>
    /// PCA is a static class that performs PCA alignment for curves, solids, and points
    /// </summary>
    public static class PCA
    {
        /// <summary>
        /// Discretize a curve into n points
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Point3d[] DiscretizeCurve(Curve curve, int n)
        {
            // sample points
            Point3d[] discretized_points = new Point3d[n];

            // populated discretized_points
            double[] discretized_params = curve.DivideByCount(n, true, out discretized_points);

            return discretized_points;
        }

        /// <summary>
        /// Convert an array of Point3ds to a jagged array positions[][] = [[X,Y,Z], [X,Y,Z],...]
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static double[][] PositionMatrix(Point3d[] points)
        {
            // data of x, y, z points
            double[][] positions = new double[points.Length][];

            for (int i = 0; i < points.Length; i++)
            {
                // current point
                var point = points[i];

                // populate data array
                positions[i] = new double[] { point.X, point.Y, point.Z };
            }

            return positions;
        }

        public static double[][] PositionMatrix(List<Point3d> points)
        {
            // data of x, y, z points
            double[][] positions = new double[points.Count][];

            for (int i = 0; i < points.Count; i++)
            {
                // current point
                var point = points[i];

                // populate data array
                positions[i] = new double[] { point.X, point.Y, point.Z };
            }

            return positions;
        }

        /// <summary>
        /// Flip PCA2 and PCA3 such that PCA2 is in the same direction as the global Y axis (if not perpendicular)
        /// </summary>
        /// <param name="vectors"></param>
        public static void AlignY(Vector3d[] vectors)
        {
            if (vectors[1].Y < 0)
            {
                vectors[1] *= -1;
                vectors[2] *= -1;
            }
        }

        /// <summary>
        /// Get the spatial PCA vectors PCA1, PCA2, PCA
        /// Optionally aligned such that PCA2 is aligned with global Y
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="align"></param>
        /// <returns></returns>
        public static Vector3d[] PCAvectors(double[][] positions, bool align)
        {
            // create a PCA class
            var pca = new PrincipalComponentAnalysis()
            {
                Method = PrincipalComponentMethod.Center
            };

            // analyze data
            MultivariateLinearRegression transform = pca.Learn(positions);

            // get PCAs
            Vector3d[] pca_vectors = new Vector3d[]
            {
                new Vector3d(),
                new Vector3d(),
                new Vector3d()
            };

            // populate vectors
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    pca_vectors[j][i] = transform.Weights[i][j];
                }
            }

            if (align) AlignY(pca_vectors);

            return pca_vectors;
        }

        /// <summary>
        /// Get the centroid of a curve
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="discretized_points"></param>
        /// <returns></returns>
        public static Point3d GetCentroid(Curve curve, Point3d[] discretized_points)
        {
            // Transform input curve
            Point3d centroid = new Point3d();

            // get the "centroid" of the curve
            if (curve.IsClosed)
            {
                var amp = AreaMassProperties.Compute(curve);
                centroid = amp.Centroid;
            }
            else
            {
                int n = discretized_points.Length;
                // if open curve, centroid is the average of the sampled points
                for (int i = 0; i < discretized_points.Length; i++)
                {
                    centroid += discretized_points[i] / n;
                }
            }

            return centroid;
        }

        /// <summary>
        /// Get the centroid of a list of points
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Point3d GetCentroid(List<Point3d> points)
        {
            Point3d centroid = new Point3d();

            int n = points.Count;

            for (int i = 0; i < n; i++)
            {
                centroid += points[i] / n;
            }

            return centroid;
        }

        /// <summary>
        /// Return the transformation object to align the local XYZ axes to the global XYZ axes centered about the curve centroid
        /// </summary>
        /// <param name="pca_vectors"></param>
        /// <param name="curve"></param>
        /// <param name="discretized_points"></param>
        /// <returns></returns>
        public static Transform Aligner(Vector3d[] pca_vectors, Curve curve, Point3d[] discretized_points)
        {
            // Transform input curve
            Point3d centroid = GetCentroid(curve, discretized_points);

            // PCA XY plane and world XY plane centered about curve centroid
            Plane plane_PCA = new Plane(centroid, pca_vectors[0], pca_vectors[1]);
            Plane plane_XYZ = new Plane(centroid, Vector3d.XAxis, Vector3d.YAxis);

            // Map the PCA XY plane to the world XY plane
            Transform plane_transform = Transform.PlaneToPlane(plane_PCA, plane_XYZ);

            return plane_transform;
        }

        /// <summary>
        /// Return the transformation object to align the local XYZ axes to the global XYZ axes centered about the point collection centroid
        /// </summary>
        /// <param name="pca_vectors"></param>
        /// <param name="curve"></param>
        /// <param name="discretized_points"></param>
        /// <returns></returns>
        public static Transform Aligner(Vector3d[] pca_vectors, List<Point3d> discretized_points)
        {
            // Transform input curve
            Point3d centroid = GetCentroid(discretized_points);

            // PCA XY plane and world XY plane centered about curve centroid
            Plane plane_PCA = new Plane(centroid, pca_vectors[0], pca_vectors[1]);
            Plane plane_XYZ = new Plane(centroid, Vector3d.XAxis, Vector3d.YAxis);

            // Map the PCA XY plane to the world XY plane
            Transform plane_transform = Transform.PlaneToPlane(plane_PCA, plane_XYZ);

            return plane_transform;
        }
    }
}
