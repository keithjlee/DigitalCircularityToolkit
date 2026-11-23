using System;
using System.Globalization;
using DigitalCircularityToolkit.GeometryProcessing;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    /// <summary>
    /// One analysed piece of inventory or demand: source geometry + <see cref="Frame"/> + Id/Quantity, plus the
    /// abstraction parameters of a subtype (DESIGN_OBJECT_SPEC). An immutable record: a change is
    /// <c>obj with { ... }</c>, <see cref="Transformed"/> or <see cref="WithFrame"/>, all of which keep the concrete type.
    /// Build objects with <see cref="Create"/> (or the subtype's Create), never by setting frame data by hand.
    /// <para>
    /// <see cref="Geometry"/> and <see cref="SampledPoints"/> are shared by reference between copies and must never be
    /// changed in place: duplicate first. Derived data (hulls, transformed geometry) is computed on first access and
    /// cached against the frame/samples/geometry it came from.
    /// </para>
    /// The generic (base) object is what the Object component outputs, so this record is not abstract.
    /// </summary>
    public record DesignObject
    {
        public GeometryBase Geometry { get; init; }
        public ObjectFrame Frame { get; init; }
        public Point3d[] SampledPoints { get; init; }
        public int NSamples { get; init; }
        public string ID { get; init; } = "NoID";
        /// <summary>Number of identical copies in inventory. Only the Set components expand it.</summary>
        public int Quantity { get; init; } = 1;

        public DesignObject() { }

        protected DesignObject(ShapeAnalysis analysis)
        {
            Geometry = analysis.Geometry;
            Frame = analysis.Frame;
            SampledPoints = analysis.Samples;
            NSamples = analysis.NSamples;
        }

        /// <summary>A generic object (frame only) from a Curve, Brep, Mesh or PointCloud.</summary>
        /// <param name="n">Target number of samples (curves and Breps).</param>
        public static DesignObject Create(GeometryBase geometry, int n, string id = "NoID") =>
            new DesignObject(ShapeAnalysis.Analyze(geometry, n)) { ID = id };

        /// <summary>Validation shared by the subtypes' Create methods.</summary>
        protected static double Positive(double value, string name) =>
            value > 0 ? value : throw new ArgumentOutOfRangeException(name, value, name + " must be > 0");

        // ---- Frame members under their original (pre-P3 field) names.

        public Point3d Centroid => Frame.Centroid;
        public Vector3d PCA1 => Frame.PCA1;
        public Vector3d PCA2 => Frame.PCA2;
        public Vector3d PCA3 => Frame.PCA3;
        public Plane LocalPlane => Frame.LocalPlane;
        public Box Localbox => Frame.Localbox;
        public BoundingBox Boundingbox => Frame.Boundingbox;
        public double Length => Frame.Length;
        public double Width => Frame.Width;
        public double Height => Frame.Height;

        // ---- Derived data, computed on first access.

        private Memo<Polyline> _hull2D;
        private Memo<Mesh> _hull3D;
        private Memo<GeometryBase> _transformedGeometry;

        /// <summary>Convex hull of the samples projected onto <see cref="LocalPlane"/> (closed polyline in that plane).</summary>
        public Polyline Hull2D => SampledPoints == null ? null
            : _hull2D.Get(Frame, SampledPoints, Geometry, () => Hulls.MakeHull2d(SampledPoints, Frame.LocalPlane));

        /// <summary>3D convex hull of the samples (empty mesh for flat or straight samples).</summary>
        public Mesh Hull3D => SampledPoints == null ? null
            : _hull3D.Get(default, SampledPoints, null, () => Hulls.MakeHull(SampledPoints));

        /// <summary>The geometry mapped from the local frame to world XY (box centre at the origin, PCA1 along X). Not used by any component.</summary>
        public GeometryBase TransformedGeometry => Geometry == null ? null
            : _transformedGeometry.Get(Frame, null, Geometry, () =>
            {
                GeometryBase g = Geometry.Duplicate();
                g.Transform(Transform.PlaneToPlane(Frame.LocalPlane, Plane.WorldXY));
                return g;
            });

        // ---- Modifiers (MODIFIER_SPEC). All return the same concrete type and leave this object unchanged.

        /// <summary>
        /// A copy moved by the rigid transform <paramref name="t"/>, with the same type and parameters (B5). Nothing is
        /// re-analysed: the frame, samples and hulls are transformed (rigid motions preserve them).
        /// </summary>
        public DesignObject Transformed(Transform t)
        {
            if (!IsRigidMotion(t))
                throw new ArgumentException("Design objects can only be moved by rigid transforms (rotation + translation).");

            GeometryBase geometry = Geometry?.Duplicate();
            geometry?.Transform(t);

            Point3d[] samples = null;
            if (SampledPoints != null)
            {
                samples = (Point3d[])SampledPoints.Clone();
                for (int i = 0; i < samples.Length; i++) samples[i].Transform(t);
            }

            DesignObject moved = this with { Geometry = geometry, Frame = Frame.Transformed(t), SampledPoints = samples };

            // Carry the hulls along instead of recomputing them from the moved samples, which can start the polygon at
            // another vertex or triangulate coplanar faces differently.
            if (samples != null)
            {
                DesignObject source = this;
                moved._hull2D.Set(moved.Frame, samples, geometry, () =>
                {
                    Polyline h = source.Hull2D;
                    if (h == null) return null;
                    var copy = new Polyline(h);
                    copy.Transform(t);
                    return copy;
                });
                moved._hull3D.Set(default, samples, null, () =>
                {
                    Mesh h = source.Hull3D?.DuplicateMesh();
                    h?.Transform(t);
                    return h;
                });
            }
            return moved;
        }

        /// <summary>A copy rotated by <paramref name="deg"/> degrees about PCA1, PCA2 or PCA3 (any other axis: PCA3) through the centroid.</summary>
        public DesignObject Rotate(int axis, double deg)
        {
            Vector3d rotaxis = axis == 1 ? PCA1 : axis == 2 ? PCA2 : PCA3;
            return Transformed(Transform.Rotation(deg / 180 * Math.PI, rotaxis, Centroid));
        }

        /// <summary>
        /// A copy whose axes are those of <paramref name="plane"/> (PCA override). The geometry stays put; only the box
        /// and extents are recomputed for the new axes, and everything derived from them (hull, estimated area,
        /// sphere) follows. The plane's origin is ignored (the box is measured from the current box centre).
        /// </summary>
        public DesignObject WithFrame(Plane plane)
        {
            plane.Origin = Frame.Localbox.Center;
            return this with { Frame = ObjectFrame.FromPlane(Geometry, Frame.Centroid, plane, plane.XAxis, plane.YAxis, plane.ZAxis) };
        }

        /// <summary>Rotation + translation only: orthonormal 3×3 part with determinant +1 and an affine bottom row.</summary>
        public static bool IsRigidMotion(Transform t)
        {
            const double tol = 1e-6;
            if (Math.Abs(t.M30) > tol || Math.Abs(t.M31) > tol || Math.Abs(t.M32) > tol || Math.Abs(t.M33 - 1) > tol) return false;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    double dot = t[0, i] * t[0, j] + t[1, i] * t[1, j] + t[2, i] * t[2, j];
                    if (Math.Abs(dot - (i == j ? 1 : 0)) > tol) return false;
                }
            double det = t.M00 * (t.M11 * t.M22 - t.M12 * t.M21) - t.M01 * (t.M10 * t.M22 - t.M12 * t.M20) + t.M02 * (t.M10 * t.M21 - t.M11 * t.M20);
            return det > 0;
        }

        /// <summary>For panels and tooltips, e.g. <c>LinearObject 'B-12' L=3200 W=100 H=50</c>. Never computes hulls.</summary>
        public sealed override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "{0} '{1}' L={2:0.###} W={3:0.###} H={4:0.###}", GetType().Name, ID, Length, Width, Height);
    }
}
