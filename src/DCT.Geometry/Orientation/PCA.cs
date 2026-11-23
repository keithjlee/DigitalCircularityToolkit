using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
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
        /// Former sign convention (kept for reference; no longer used): flip PCA2 and PCA3 together so that PCA2
        /// points along +Y. It left PCA1's sign arbitrary and did not enforce a right-handed frame.
        /// </summary>
        public static void AlignY(Vector3d[] vectors)
        {
            if (vectors[1].Y < 0)
            {
                vectors[1] *= -1;
                vectors[2] *= -1;
            }
        }

        /// <summary>
        /// Deterministic signs (ALIGNMENT_SPEC §6): PCA1 has a non-negative world-X component (ties: Y, then Z),
        /// PCA2 a non-negative world-Y component (ties: Z, then X), and PCA3 = PCA1 × PCA2 (right-handed).
        /// </summary>
        public static void Canonicalize(Vector3d[] vectors)
        {
            vectors[0] = SignFirstNonZero(vectors[0], 0);
            vectors[1] = SignFirstNonZero(vectors[1], 1);
            vectors[2] = Vector3d.CrossProduct(vectors[0], vectors[1]);
            vectors[2].Unitize();
        }

        private static Vector3d SignFirstNonZero(Vector3d v, int first)
        {
            const double eps = 1e-9;
            for (int k = 0; k < 3; k++)
            {
                double c = v[(first + k) % 3];
                if (Math.Abs(c) > eps) return c < 0 ? -v : v;
            }
            return v;
        }

        /// <summary>
        /// Principal axes of a point set: eigenvectors of the covariance matrix, by decreasing variance.
        /// Two passes (mean, then centred second moments) and a 3×3 Jacobi eigensolve; O(n), no per-point
        /// allocation. Replaces Accord's PCA (P1). With <paramref name="align"/>, signs follow
        /// <see cref="Canonicalize"/>; otherwise only the right-handed frame is enforced.
        /// </summary>
        public static Vector3d[] PCAvectors(IReadOnlyList<Point3d> points, bool align)
        {
            int n = points.Count;
            double mx = 0, my = 0, mz = 0;
            for (int i = 0; i < n; i++) { mx += points[i].X; my += points[i].Y; mz += points[i].Z; }
            if (n > 0) { mx /= n; my /= n; mz /= n; }

            double xx = 0, xy = 0, xz = 0, yy = 0, yz = 0, zz = 0;
            for (int i = 0; i < n; i++)
            {
                double x = points[i].X - mx, y = points[i].Y - my, z = points[i].Z - mz;
                xx += x * x; xy += x * y; xz += x * z; yy += y * y; yz += y * z; zz += z * z;
            }
            var cov = new double[,] { { xx, xy, xz }, { xy, yy, yz }, { xz, yz, zz } };

            Numerics.SymmetricEigen3.Decompose(cov, out double[] _, out double[][] vecs);
            var pca_vectors = new Vector3d[3];
            for (int k = 0; k < 3; k++) pca_vectors[k] = new Vector3d(vecs[k][0], vecs[k][1], vecs[k][2]);

            if (align)
            {
                Canonicalize(pca_vectors);
            }
            else
            {
                pca_vectors[2] = Vector3d.CrossProduct(pca_vectors[0], pca_vectors[1]);
                pca_vectors[2].Unitize();
            }
            return pca_vectors;
        }

        /// <summary>Compatibility overload: rows of [x, y, z].</summary>
        public static Vector3d[] PCAvectors(double[][] positions, bool align)
        {
            var pts = new Point3d[positions.Length];
            for (int i = 0; i < pts.Length; i++) pts[i] = new Point3d(positions[i][0], positions[i][1], positions[i][2]);
            return PCAvectors(pts, align);
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
        public static Point3d GetCentroid(IReadOnlyList<Point3d> points)
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
            pca_vectors = PCAvectors(points, align);

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

            pca_vectors = PCAvectors(discretized_points, align);

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
        /// <summary>PCA of a mesh; returns the plane transform instead of applying it to a copy (P1 performance).</summary>
        public static void SolvePCA(Mesh mesh, bool align, out Vector3d[] pca_vectors, out Point3d[] discretized_points, out Transform plane_transform)
        {
            discretized_points = mesh.Vertices.ToPoint3dArray();
            pca_vectors = PCAvectors(discretized_points, align);
            plane_transform = GetPlaneTransformer(pca_vectors, discretized_points.ToList());
        }

        public static void SolvePCA(Mesh mesh, bool align, out Vector3d[] pca_vectors, out Point3d[] discretized_points, Mesh transformed_mesh)
        {
            // Get mesh vertices
            discretized_points = mesh.Vertices.ToPoint3dArray();

            pca_vectors = PCAvectors(discretized_points, align);

            // transform point set
            Transform plane_transform = GetPlaneTransformer(pca_vectors, discretized_points.ToList());

            transformed_mesh.Transform(plane_transform);
        }

        /// <summary>
        /// PCA of a Brep without the transformed copy: no VolumeMassProperties (P1 performance).
        /// Use <see cref="TransformedBrep"/> when the transformed geometry is actually needed.
        /// </summary>
        public static void SolvePCA(Brep brep, int n, bool align, out Vector3d[] pca_vectors, out Point3d[] discretized_points)
        {
            discretized_points = Discretizer.DiscretizeBrep(brep, n).ToArray();
            pca_vectors = PCAvectors(discretized_points, align);
        }

        /// <summary>The Brep rotated into the world axes about its volume centroid (as SolvePCA used to produce eagerly).</summary>
        public static Brep TransformedBrep(Brep brep, Vector3d[] pca_vectors)
        {
            VolumeMassProperties props = VolumeMassProperties.Compute(brep);
            Transform plane_transform = GetPlaneTransformer(pca_vectors, props.Centroid);
            Brep transformed_brep = brep.DuplicateBrep();
            transformed_brep.Transform(plane_transform);
            return transformed_brep;
        }

        public static void SolvePCA(Brep brep, int n, bool align, out Vector3d[] pca_vectors, out Point3d[] discretized_points, Brep transformed_brep)
        {
            // number of samples per face
            //List<int> sample_densities = AssignSampleDensity(brep, n);

            // density of UV sampling per face
            //List<int> n_uv = AssignUV(sample_densities);

            // get approx evenly distributed points on surfaces
            discretized_points = Discretizer.DiscretizeBrep(brep, n).ToArray();

            pca_vectors = PCAvectors(discretized_points, align);

            // transform point set
            // get the centroid
            VolumeMassProperties props = VolumeMassProperties.Compute(brep);

            // transformation plane-to-plane
            Transform plane_transform = GetPlaneTransformer(pca_vectors, props.Centroid);

            // apply
            transformed_brep.Transform(plane_transform);
        }
    }
}
