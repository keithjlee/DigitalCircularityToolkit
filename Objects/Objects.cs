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
        public Box Localbox;
        public GeometryBase Geometry;
        public string ID { get; set; }

        // ---- Lazily computed members (P1 performance). Same values as the former eager fields; computed on first
        // access, so objects that are never inspected never pay for them. A setter replaces the lazy value.

        private Mesh _hull3D;
        private Func<Mesh> _hull3DFactory;

        /// <summary>3D convex hull of the sampled points (computed on first access).</summary>
        public Mesh Hull3D
        {
            get
            {
                var factory = _hull3DFactory;
                if (_hull3D == null && factory != null)
                {
                    _hull3D = factory();
                    _hull3DFactory = null;
                }
                return _hull3D;
            }
            set { _hull3D = value; _hull3DFactory = null; }
        }

        /// <summary>Defer the 3D hull of <paramref name="points"/> until first access.</summary>
        protected void SetHull3DLazy(Point3d[] points)
        {
            _hull3D = null;
            _hull3DFactory = points == null ? (Func<Mesh>)null : () => Hulls.MakeHull(points);
        }

        private GeometryBase _transformedGeometry;
        private Func<GeometryBase> _transformedGeometryFactory;

        /// <summary>The geometry rotated into the world axes about its analysis pivot (computed on first access; not used by any component).</summary>
        public GeometryBase TransformedGeometry
        {
            get
            {
                var factory = _transformedGeometryFactory;
                if (_transformedGeometry == null && factory != null)
                {
                    _transformedGeometry = factory();
                    _transformedGeometryFactory = null;
                }
                return _transformedGeometry;
            }
            set { _transformedGeometry = value; _transformedGeometryFactory = null; }
        }

        /// <summary>Defer the transformed geometry until first access.</summary>
        protected void SetTransformedGeometryLazy(Func<GeometryBase> factory)
        {
            _transformedGeometry = null;
            _transformedGeometryFactory = factory;
        }

        public DesignObject() { }

        /// <summary>
        /// Shallow copy that preserves the concrete subtype (a LinearObject stays a LinearObject).
        /// Modifier components must copy before changing anything: Grasshopper hands every downstream
        /// component the same instance. Replace fields on the copy; never mutate shared geometry in place.
        /// </summary>
        public DesignObject ShallowCopy() => (DesignObject)MemberwiseClone();

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
            this.ID = other.ID;
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
            SetHull3DLazy(SampledPoints);
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
            SetHull3DLazy(SampledPoints);
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
            // Solve
            PCA.SolvePCA(mesh, true, out Vector3d[] pca_vectors, out Point3d[] discretized_points, out Transform plane_transform);

            // Populate
            NSamples = mesh.Vertices.Count;
            SampledPoints = discretized_points;
            PCA1 = pca_vectors[0];
            PCA2 = pca_vectors[1];
            PCA3 = pca_vectors[2];
            Geometry = mesh;
            SetTransformedGeometryLazy(() =>
            {
                Mesh transformed_mesh = mesh.DuplicateMesh();
                transformed_mesh.Transform(plane_transform);
                return transformed_mesh;
            });

            // Get Centroid
            Centroid = PCA.GetCentroid(discretized_points.ToList());

            // Get Local plane
            LocalPlane = new Plane(Centroid, PCA1, PCA2);

            // Get Bounding Box
            Boundingbox = Geometry.GetBoundingBox(LocalPlane, out Localbox);
            LocalPlane.Origin = Localbox.Center;

            Hull2D = Hulls.MakeHull2d(SampledPoints, LocalPlane);
            SetHull3DLazy(SampledPoints);
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
            // Solve
            PCA.SolvePCA(brep, n_target, true, out Vector3d[] pca_vectors, out Point3d[] discretized_points);

            // Populate
            NSamples = discretized_points.Length;
            SampledPoints = discretized_points;
            PCA1 = pca_vectors[0];
            PCA2 = pca_vectors[1];
            PCA3 = pca_vectors[2];
            Geometry = brep;
            Vector3d[] pca_copy = (Vector3d[])pca_vectors.Clone();
            SetTransformedGeometryLazy(() => PCA.TransformedBrep(brep, pca_copy));

            // Get Centroid
            Centroid = PCA.GetCentroid(discretized_points.ToList());

            // Get Local plane
            LocalPlane = new Plane(Centroid, PCA1, PCA2);

            // Get Bounding Box
            Boundingbox = Geometry.GetBoundingBox(LocalPlane, out Localbox);
            LocalPlane.Origin = Localbox.Center;

            Hull2D = Hulls.MakeHull2d(SampledPoints, LocalPlane);
            SetHull3DLazy(SampledPoints);
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

            double rad = deg / 180 * Math.PI;
            Transform rotater = Transform.Rotation(rad, rotaxis, this.Centroid);

            return TransformedCopy(rotater);
        }

        public DesignObject TransformObject(Transform t)
        {
            return TransformedCopy(t);
        }

        /// <summary>
        /// A copy of this object moved by the rigid transform <paramref name="t"/>, keeping its concrete type
        /// and parameters (B5). The analysis is carried along instead of recomputed: the frame, box, samples
        /// and hulls are equivariant under rigid motion (MODIFIER_SPEC rule 3). The original is not modified.
        /// </summary>
        public DesignObject TransformedCopy(Transform t)
        {
            if (t.IsRigid(1e-6) != TransformRigidType.Rigid)
                throw new ArgumentException("Design objects can only be moved by rigid transforms (rotation + translation).");

            DesignObject obj = ShallowCopy();

            if (Geometry != null)
            {
                obj.Geometry = Geometry.Duplicate();
                obj.Geometry.Transform(t);
            }

            Point3d centroid = Centroid;
            centroid.Transform(t);
            obj.Centroid = centroid;

            if (SampledPoints != null)
            {
                var pts = (Point3d[])SampledPoints.Clone();
                for (int i = 0; i < pts.Length; i++) pts[i].Transform(t);
                obj.SampledPoints = pts;
            }

            Vector3d v1 = PCA1, v2 = PCA2, v3 = PCA3;
            v1.Transform(t); v2.Transform(t); v3.Transform(t);
            obj.PCA1 = v1; obj.PCA2 = v2; obj.PCA3 = v3;

            Plane plane = LocalPlane;
            plane.Transform(t);
            obj.LocalPlane = plane;

            Box box = Localbox;
            box.Transform(t);
            obj.Localbox = box;
            // Boundingbox is expressed in the local frame, so it does not change.

            if (Hull2D != null)
            {
                var hull = new Polyline(Hull2D);
                hull.Transform(t);
                obj.Hull2D = hull;
            }
            if (_hull3D != null)
            {
                Mesh hull3 = _hull3D.DuplicateMesh();
                hull3.Transform(t);
                obj.Hull3D = hull3;
            }
            else if (_hull3DFactory != null)
            {
                // Move the original's hull when first needed (a hull of the moved samples can triangulate
                // coplanar faces differently).
                DesignObject source = this;
                obj._hull3D = null;
                obj._hull3DFactory = () =>
                {
                    Mesh h = source.Hull3D?.DuplicateMesh();
                    h?.Transform(t);
                    return h;
                };
            }

            // TransformedGeometry (the geometry rotated into the world axes about the centroid) keeps its
            // orientation; only its pivot moves. It is not read anywhere today, so it stays lazy.
            if (_transformedGeometry != null || _transformedGeometryFactory != null)
            {
                DesignObject source = this;
                Vector3d delta = obj.Centroid - Centroid;
                obj.SetTransformedGeometryLazy(() =>
                {
                    GeometryBase g = source.TransformedGeometry?.Duplicate();
                    g?.Translate(delta);
                    return g;
                });
            }

            obj.TransformDerived(t);
            return obj;
        }

        /// <summary>Subclasses transform any stored world-space data of their own here (called on the copy).</summary>
        protected virtual void TransformDerived(Transform t) { }

        public void Repopulate(Plane plane)
        {
            // new planes
            LocalPlane = plane;
            PCA1 = plane.XAxis; PCA2 = plane.YAxis; PCA3 = plane.ZAxis;

            // transformed geometry
            Plane plane_global = new Plane(plane.Origin, Vector3d.XAxis, Vector3d.YAxis);
            Transform plane_transform = Transform.PlaneToPlane(plane, plane_global);
            GeometryBase geometry = Geometry;
            SetTransformedGeometryLazy(() =>
            {
                GeometryBase transformed = geometry.Duplicate();
                transformed.Transform(plane_transform);
                return transformed;
            });
            Boundingbox = Geometry.GetBoundingBox(plane, out Localbox);

            LocalPlane.Origin = Localbox.Center;

            //new hull
            Hull2D = Hulls.MakeHull2d(SampledPoints, plane);

        }
    }
}
