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
using Rhino.Geometry.Collections;

namespace DigitalCircularityToolkit.Orientation
{
    /// <summary>
    /// PCA is a static class that performs PCA alignment for curves, solids, and points
    /// </summary>
    public static class PCA
    {
        
        /// <summary>
        /// Given a target total number of samples n, find the area-weighted proportion of the samples to perform on each face of a brep
        /// </summary>
        /// <param name="brep"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static List<int> AssignSampleDensity(Brep brep, int n)
        {

            // Get total surface area of brep

            // Area for each face
            List<double> areas = new List<double>();

            // Total area
            double total_area = 0;

            foreach (BrepFace face in brep.Faces)
            {
                double area = face.ToBrep().GetArea();
                areas.Add(area);
                total_area += area;
            }

            // Number of point samples to take for each face
            List<int> n_samples = new List<int>();

            // Number of samples on face is approx proportional to surface area
            foreach (double area in areas)
            {
                int n_sample = (int)Math.Round(area / total_area * n);
                n_samples.Add(n_sample);
            }

            return n_samples;
        }

        /// <summary>
        /// Given a target number of samples on a face n, find approximate the u, v discretization value sqrt(n)
        /// </summary>
        /// <param name="n_samples"></param>
        /// <returns></returns>
        public static List<int> AssignUV(List<int> n_samples)
        {
           List<int> n_per_side = new List<int>();

            foreach (int n in n_samples)
            {
                int uv = (int)Math.Round(Math.Sqrt(n));
                n_per_side.Add(uv);
            }

            return n_per_side;
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

            // Flip PCA2 and PCA3 if we want to align PCA2 to global Y
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
            if (curve.IsClosed && curve.IsPlanar())
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
        public static Transform GetPlaneTransformer(Vector3d[] pca_vectors, Curve curve, Point3d[] discretized_points)
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
        public static Transform GetPlaneTransformer(Vector3d[] pca_vectors, List<Point3d> discretized_points)
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

        public static Transform GetPlaneTransformer(Vector3d[] pca_vectors, Point3d centroid)
        {
            // PCA XY plane and world XY plane centered about curve centroid
            Plane plane_PCA = new Plane(centroid, pca_vectors[0], pca_vectors[1]);
            Plane plane_XYZ = new Plane(centroid, Vector3d.XAxis, Vector3d.YAxis);

            // Map the PCA XY plane to the world XY plane
            Transform plane_transform = Transform.PlaneToPlane(plane_PCA, plane_XYZ);

            return plane_transform;
        }

        /// <summary>
        /// Solve for PCA vectors and transformed geometry for a set of points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="align"></param>
        /// <param name="pca_vectors"></param>
        /// <param name="transformed_points"></param>
        public static void SolvePCA(List<Point3d> points, bool align, out Vector3d[] pca_vectors, out Point3d[] transformed_points)
        {
            //get xyz data
            double[][] positions = Discretizer.PositionMatrix(points);

            // get PCA vecvtors
            pca_vectors = PCAvectors(positions, align);

            // transform point set
            Transform plane_transform = GetPlaneTransformer(pca_vectors, points);

            // apply
            PointCloud new_points = new PointCloud(points);
            new_points.Transform(plane_transform);

            transformed_points = new_points.GetPoints();
        }

        /// <summary>
        /// Solve for PCA vectors and transformed geometry for a curve
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="align"></param>
        /// <param name="n"></param>
        /// <param name="pca_vectors"></param>
        /// <param name="transformed_curve"></param>
        public static void SolvePCA(Curve curve, bool align, int n, out Vector3d[] pca_vectors, out Point3d[] discretized_points, out Curve transformed_curve)
        {
            // sample points
            discretized_points = Discretizer.DiscretizeCurve(curve, n);

            // data of x, y, z points
            double[][] positions = Discretizer.PositionMatrix(discretized_points);

            // get PCA vecvtors
            pca_vectors = PCAvectors(positions, align);

            // get the Transformation object to align curve to global XYZ
            Transform plane_transform = GetPlaneTransformer(pca_vectors, curve, discretized_points);

            // apply to new curve
            transformed_curve = curve.DuplicateCurve();
            transformed_curve.Transform(plane_transform);
        }

        /// <summary>
        /// Solve for PCA vectors and transformed geometry for a brep
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="align"></param>
        /// <param name="pca_vectors"></param>
        /// <param name="discretized_points"></param>
        public static void SolvePCA(Mesh mesh, bool align, out Vector3d[] pca_vectors, out Point3d[] discretized_points, Mesh transformed_mesh)
        {
            // Get mesh vertices
            discretized_points = mesh.Vertices.ToPoint3dArray();

            //get xyz data
            double[][] positions = Discretizer.PositionMatrix(discretized_points);

            // get PCA vecvtors
            pca_vectors = PCAvectors(positions, align);

            // transform point set
            Transform plane_transform = GetPlaneTransformer(pca_vectors, discretized_points.ToList());

            transformed_mesh.Transform(plane_transform);
        }

        public static void SolvePCA(Brep brep, int n, bool align, out Vector3d[] pca_vectors, out Point3d[] discretized_points, Brep transformed_brep)
        {
            // number of samples per face
            List<int> sample_densities = AssignSampleDensity(brep, n);

            // density of UV sampling per face
            List<int> n_uv = AssignUV(sample_densities);

            // get approx evenly distributed points on surfaces
            discretized_points = Discretizer.DiscretizeBrep(brep, n_uv);

            // get PCA vectors
            double[][] positions = Discretizer.PositionMatrix(discretized_points);

            // get PCA vecvtors
            pca_vectors = PCAvectors(positions, align);

            // transform point set
            // get the centroid
            VolumeMassProperties props = VolumeMassProperties.Compute(brep);

            // transformation plane-to-plane
            Transform plane_transform = GetPlaneTransformer(pca_vectors, props.Centroid);

            // apply
            brep.Transform(plane_transform);
        }
    }
}
