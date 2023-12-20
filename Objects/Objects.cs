using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DigitalCircularityToolkit.Orientation;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class Object
    {
        public Point3d Centroid;
        public int NSamples;
        public Point3d[] SampledPoints;
        public Vector3d PCA1;
        public Vector3d PCA2;
        public Vector3d PCA3;
        public Plane LocalPlane;
        public BoundingBox Boundingbox;
        public double Length
        {
            get
            {
                return Boundingbox.Corner(true, true, true).DistanceTo(Boundingbox.Corner(false, true, true));
            }
        }
        public double Width
        {
            get
            {
                return Boundingbox.Corner(true, true, true).DistanceTo(Boundingbox.Corner(true, false, true));
            }
        }
        public double Height
        {
            get
            {
                return Boundingbox.Corner(true, true, true).DistanceTo(Boundingbox.Corner(true, true, false));
            }
        }
        public Box Localbox;
        public GeometryBase Geometry;
        public GeometryBase TransformedGeometry;

        public Object() { }

        /// <summary>
        /// A linear object
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="n"></param>
        public Object(Curve curve, int n)
        {
            Populate(curve, n);
        }

        /// <summary>
        /// A linear object with a user-defined primary axis
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="n"></param>
        /// <param name="PCA1_override"></param>
        public Object(Curve curve, int n, Vector3d PCA1_override)
        {
            Populate(curve, n);

            PCA1_override.Unitize();
            ObjectAnalysis.OverridePCA(PCA1_override, this);
        }

        /// <summary>
        /// Populate the fields of an Object
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="n"></param>
        private void Populate(Curve curve, int n)
        {
            // Initialize
            Vector3d[] pca_vectors;
            Point3d[] discretized_points;
            Curve transformed_curve;

            // Solve
            PCA.SolvePCA(curve, true, n, out pca_vectors, out discretized_points, out transformed_curve);

            // Populate
            NSamples = n;
            SampledPoints = discretized_points;
            PCA1 = pca_vectors[0];
            PCA2 = pca_vectors[1];
            PCA3 = pca_vectors[2];
            Geometry = curve;
            TransformedGeometry = transformed_curve;

            // Get Centroid
            Centroid = PCA.GetCentroid(curve, discretized_points);

            // Get Local plane
            LocalPlane = new Plane(Centroid, PCA1, PCA2);

            // Get Bounding Box
            Boundingbox = Geometry.GetBoundingBox(LocalPlane, out Localbox);

        }

        public Object(List<Point3d> points, int n)
        {
            Populate(points);
        }

        public Object(List<Point3d> points, int n, Vector3d PCA1_override)
        {
            Populate(points);
            if (PCA1_override.Length > 0) ObjectAnalysis.OverridePCA(PCA1_override, this);
        }

        public Object(PointCloud points, int n)
        {
            Populate(points.GetPoints().ToList());
        }

        public Object(PointCloud points, int n, Vector3d PCA1_override)
        {
            Populate(points.GetPoints().ToList());
            ObjectAnalysis.OverridePCA(PCA1_override, this);
        }

        private void Populate(List<Point3d> points)
        {
            // Initialize
            Vector3d[] pca_vectors;
            Point3d[] transformed_points;

            // Solve
            PCA.SolvePCA(points, true, out pca_vectors, out transformed_points);

            // Populate
            NSamples = points.Count;
            SampledPoints = points.ToArray();
            PCA1 = pca_vectors[0];
            PCA2 = pca_vectors[1];
            PCA3 = pca_vectors[2];
            Geometry = new PointCloud(points);
            TransformedGeometry = new PointCloud(transformed_points);

            Centroid = PCA.GetCentroid(points);

            // Get Local plane
            LocalPlane = new Plane(Centroid, PCA1, PCA2);

            // Get Bounding Box
            Boundingbox = Geometry.GetBoundingBox(LocalPlane, out Localbox);
        }

        public Object(Mesh mesh, int n)
        {
            Populate(mesh);
        }

        public Object(Mesh mesh, int n, Vector3d PCA1_override)
        {
            Populate(mesh);
            ObjectAnalysis.OverridePCA(PCA1_override, this);
        }

        private void Populate(Mesh mesh)
        {
            // Initialize
            Vector3d[] pca_vectors;
            Point3d[] discretized_points;
            Mesh transformed_mesh = mesh.DuplicateMesh();

            // Solve
            PCA.SolvePCA(mesh, true, out pca_vectors, out discretized_points, transformed_mesh);

            // Populate
            NSamples = mesh.Vertices.Count;
            SampledPoints = discretized_points;
            PCA1 = pca_vectors[0];
            PCA2 = pca_vectors[1];
            PCA3 = pca_vectors[2];
            Geometry = mesh;
            TransformedGeometry = transformed_mesh;

            // Get Centroid
            Centroid = PCA.GetCentroid(discretized_points.ToList());

            // Get Local plane
            LocalPlane = new Plane(Centroid, PCA1, PCA2);

            // Get Bounding Box
            Boundingbox = Geometry.GetBoundingBox(LocalPlane, out Localbox);
        }

        public Object(Brep brep, int n)
        {
            Populate(brep, n);
        }

        public Object(Brep brep, int n, Vector3d PCA1_override)
        {
            Populate(brep, n);
            ObjectAnalysis.OverridePCA(PCA1_override, this);
        }

        private void Populate(Brep brep, int n_target)
        {
            // Initialize
            Point3d[] discretized_points;
            Vector3d[] pca_vectors;
            Brep transformed_brep = brep.DuplicateBrep();

            // Solve
            PCA.SolvePCA(brep, n_target, true, out pca_vectors, out discretized_points, transformed_brep);

            // Populate
            NSamples = discretized_points.Length;
            SampledPoints = discretized_points;
            PCA1 = pca_vectors[0];
            PCA2 = pca_vectors[1];
            PCA3 = pca_vectors[2];
            Geometry = brep;
            TransformedGeometry = transformed_brep;

            // Get Centroid
            Centroid = PCA.GetCentroid(discretized_points.ToList());

            // Get Local plane
            LocalPlane = new Plane(Centroid, PCA1, PCA2);

            // Get Bounding Box
            Boundingbox = Geometry.GetBoundingBox(LocalPlane, out Localbox);
        }


    }
}
