using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DigitalCircularityToolkit.Orientation;
using DigitalCircularityToolkit.Utilities;
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

            if (PCA1_override.Length > 0)
            {
                PCA1_override.Unitize();
                ObjectAnalysis.OverridePCA(PCA1_override, this);
            }
        }

        /// <summary>
        /// Populate the fields of an Object
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="n"></param>
        public void Populate(Curve curve, int n)
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

            if (PCA1_override.Length > 0)
            {
                PCA1_override.Unitize();
                ObjectAnalysis.OverridePCA(PCA1_override, this);
            }
        }

        public Object(PointCloud points, int n)
        {
            Populate(points.GetPoints().ToList());
        }

        public Object(PointCloud points, int n, Vector3d PCA1_override)
        {
            Populate(points.GetPoints().ToList());

            if (PCA1_override.Length > 0)
            {
                PCA1_override.Unitize();
                ObjectAnalysis.OverridePCA(PCA1_override, this);
            }
        }

        public void Populate(List<Point3d> points)
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

            if (PCA1_override.Length > 0)
            {
                PCA1_override.Unitize();
                ObjectAnalysis.OverridePCA(PCA1_override, this);
            }
        }

        public void Populate(Mesh mesh)
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

            if (PCA1_override.Length > 0)
            {
                PCA1_override.Unitize();
                ObjectAnalysis.OverridePCA(PCA1_override, this);
            }
        }

        public void Populate(Brep brep, int n_target)
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

        public Object Rotate(int axis, double deg)
        {
            Vector3d rotaxis = new Vector3d();

            if (axis == 1)
            {
                rotaxis = this.PCA1;
            }
            else if (axis == 2)
            {
                rotaxis = this.PCA2;
            }
            else
            {
                rotaxis = this.PCA3;
            }

            // rotate the local plane
            double rad = deg / 180 * Math.PI;
            Transform rotater = Transform.Rotation(rad, rotaxis, this.Centroid);

            // transform all relevant data
            var rotated_geo = Geometry.Duplicate();
            rotated_geo.Transform(rotater);

            // figure out type and set
            Object obj = new Object();
            int n = NSamples;

            var curve = rotated_geo as Curve;
            if (curve != null) obj = new Object(curve, n);

            var brep = rotated_geo as Brep;
            if (brep != null) obj = new Object(brep, n);

            var mesh = rotated_geo as Mesh;
            if (mesh != null) obj = new Object(mesh, n);

            var pointcloud = rotated_geo as PointCloud;
            if (pointcloud != null) obj = new Object(pointcloud, n);

            return obj;
        }
    }
}
