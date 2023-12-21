using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DigitalCircularityToolkit.GeometryProcessing;
using DigitalCircularityToolkit.Orientation;
using DigitalCircularityToolkit.Utilities;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class DesignObject
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
        public Polyline Hull2D;
        public Mesh Hull3D;
        public Box Localbox;
        public GeometryBase Geometry;
        public GeometryBase TransformedGeometry;

        public DesignObject() { }

        public DesignObject(DesignObject other)
        {
            this.Centroid= other.Centroid;
            this.NSamples= other.NSamples;
            this.SampledPoints= other.SampledPoints;
            this.PCA1= other.PCA1;
            this.PCA2= other.PCA2;
            this.PCA3= other.PCA3;
            this.Localbox= other.Localbox;
            this.LocalPlane= other.LocalPlane;
            this.Boundingbox= other.Boundingbox;
            this.Geometry= other.Geometry;
            this.TransformedGeometry = other.TransformedGeometry;
        }

        /// <summary>
        /// A linear object
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="n"></param>
        public DesignObject(Curve curve, int n)
        {
            Populate(curve, n);
        }

        /// <summary>
        /// A linear object with a user-defined primary axis
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="n"></param>
        /// <param name="PCA1_override"></param>
        public DesignObject(Curve curve, int n, Vector3d PCA1_override)
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

            // Solve
            PCA.SolvePCA(curve, true, n, out Vector3d[] pca_vectors, out Point3d[] discretized_points, out Curve transformed_curve);

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
            LocalPlane.Origin = Localbox.Center;

            Hull2D = Hulls.MakeHull2d(SampledPoints, LocalPlane);
            Hull3D = Hulls.MakeHull(SampledPoints);
        }

        public DesignObject(List<Point3d> points, int _)
        {
            Populate(points);
        }

        public DesignObject(List<Point3d> points, int _, Vector3d PCA1_override)
        {
            Populate(points);

            if (PCA1_override.Length > 0)
            {
                PCA1_override.Unitize();
                ObjectAnalysis.OverridePCA(PCA1_override, this);
            }
        }

        public DesignObject(PointCloud points, int _)
        {
            Populate(points.GetPoints().ToList());
        }

        public DesignObject(PointCloud points, int _, Vector3d PCA1_override)
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

            // Solve
            PCA.SolvePCA(points, true, out Vector3d[] pca_vectors, out Point3d[] transformed_points);

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
            LocalPlane.Origin = Localbox.Center;

            Hull2D = Hulls.MakeHull2d(SampledPoints, LocalPlane);
            Hull3D = Hulls.MakeHull(SampledPoints);
        }

        public DesignObject(Mesh mesh, int _)
        {
            Populate(mesh);
        }

        public DesignObject(Mesh mesh, int _, Vector3d PCA1_override)
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
            Mesh transformed_mesh = mesh.DuplicateMesh();

            // Solve
            PCA.SolvePCA(mesh, true, out Vector3d[] pca_vectors, out Point3d[] discretized_points, transformed_mesh);

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
            LocalPlane.Origin = Localbox.Center;

            Hull2D = Hulls.MakeHull2d(SampledPoints, LocalPlane);
            Hull3D = Hulls.MakeHull(SampledPoints);
        }

        public DesignObject(Brep brep, int n)
        {
            Populate(brep, n);
        }

        public DesignObject(Brep brep, int n, Vector3d PCA1_override)
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
            Brep transformed_brep = brep.DuplicateBrep();

            // Solve
            PCA.SolvePCA(brep, n_target, true, out Vector3d[] pca_vectors, out Point3d[] discretized_points, transformed_brep);

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
            LocalPlane.Origin = Localbox.Center;

            Hull2D = Hulls.MakeHull2d(SampledPoints, LocalPlane);
            Hull3D = Hulls.MakeHull(SampledPoints);
        }

        public DesignObject Rotate(int axis, double deg)
        {
            Vector3d rotaxis;

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
            DesignObject obj = new DesignObject();

            if (rotated_geo is Curve curve) obj = new DesignObject(curve, NSamples);

            if (rotated_geo is Brep brep) obj = new DesignObject(brep, NSamples);

            if (rotated_geo is Mesh mesh) obj = new DesignObject(mesh, NSamples);

            if (rotated_geo is PointCloud pointcloud) obj = new DesignObject(pointcloud, NSamples);

            return obj;
        }

        public DesignObject TransformObject(Transform t)
        {

            var transformed_geo = Geometry.Duplicate();
            transformed_geo.Transform(t);

            DesignObject obj = new DesignObject();

            if (transformed_geo is Curve curve) obj = new DesignObject(curve, NSamples);

            if (transformed_geo is Brep brep) obj = new DesignObject(brep, NSamples);

            if (transformed_geo is Mesh mesh) obj = new DesignObject(mesh, NSamples);

            if (transformed_geo is PointCloud pointcloud) obj = new DesignObject(pointcloud, NSamples);

            return obj;
        }

        public void Repopulate(Plane plane)
        {
            // new planes
            LocalPlane = plane;
            PCA1 = plane.XAxis; PCA2 = plane.YAxis; PCA3 = plane.ZAxis;

            // transformed geometry
            Plane plane_global = new Plane(plane.Origin, Vector3d.XAxis, Vector3d.YAxis);
            Transform plane_transform = Transform.PlaneToPlane(plane, plane_global);
            TransformedGeometry = Geometry.Duplicate();
            TransformedGeometry.Transform(plane_transform);
            Boundingbox = Geometry.GetBoundingBox(plane, out Localbox);

            //new hull
            Hull2D = Hulls.MakeHull2d(SampledPoints, plane);

        }
    }
}
