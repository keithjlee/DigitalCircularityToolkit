using System;
using System.Collections.Generic;
using System.Linq;
using DigitalCircularityToolkit.Numerics;
using DigitalCircularityToolkit.Orientation;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    /// <summary>How a design object's axes are chosen (ALIGNMENT_SPEC §3).</summary>
    public enum OrientationMode
    {
        /// <summary>Principal axes of the shape (area/length weighted), with ambiguous axes resolved by the minimum-area rectangle.</summary>
        Auto = 0,
        /// <summary>Principal axes of the shape (area/length weighted), never resolved.</summary>
        PCA = 1,
        /// <summary>Third principal axis kept; the other two from the minimum-area rectangle.</summary>
        MinBox = 2,
        /// <summary>The pre-v0.7 analysis: unweighted PCA of the sample points (curve divisions, Brep wireframe, mesh vertices).</summary>
        Legacy = 3,
        /// <summary>Axes given by the user (a plane, or an X direction).</summary>
        Override = 4,
    }

    /// <summary>Which ambiguity rule set the axes (ALIGNMENT_SPEC §4).</summary>
    public enum OrientationResolution
    {
        None = 0,
        /// <summary>The two largest principal axes were (nearly) equal: X/Y from the minimum-area rectangle.</summary>
        MinBoxXY = 1,
        /// <summary>The two smallest principal axes were (nearly) equal: Y/Z from the minimum-area rectangle.</summary>
        MinBoxYZ = 2,
        /// <summary>No preferred direction (square, round or isotropic): the deterministic fallback picked the axes.</summary>
        Symmetric = 3,
        /// <summary>A box Brep: axes along its edges.</summary>
        BoxEdges = 4,
    }

    /// <summary>Analysis settings. <see cref="Legacy"/> reproduces the pre-v0.7 frames exactly.</summary>
    /// <param name="Mode">Orientation mode.</param>
    /// <param name="Tolerance">τ: axes are ambiguous when the ratio of their principal variances is below 1 + τ.</param>
    /// <param name="OverridePlane">With <see cref="OrientationMode.Override"/>: X and Y from this plane.</param>
    /// <param name="OverrideX">With <see cref="OrientationMode.Override"/> and no plane: X from this vector, Y and Z from the principal axes.</param>
    public sealed record AnalysisOptions(OrientationMode Mode = OrientationMode.Auto, double Tolerance = 0.05,
        Plane? OverridePlane = null, Vector3d? OverrideX = null)
    {
        public static readonly AnalysisOptions Legacy = new(OrientationMode.Legacy);
        public static readonly AnalysisOptions Default = new();
    }

    /// <summary>
    /// The analysis of one piece of geometry: samples, frame and box (DESIGN_OBJECT_SPEC §4, ALIGNMENT_SPEC). This is
    /// the only place that dispatches on the geometry type; every design object is built from its result. Convert
    /// other input types first with <see cref="ObjectInput.Normalize"/>.
    /// </summary>
    /// <param name="Geometry">The analysed geometry (for point clouds, a plain copy of the points).</param>
    /// <param name="Frame">Axes, centroid, aligned box, and how the axes were chosen.</param>
    /// <param name="Samples">Points for hulls and radii (curve divisions, Brep wireframe samples, mesh vertices, cloud points), in every mode.</param>
    /// <param name="NSamples">The requested sample count for curves; the point count otherwise.</param>
    public sealed record ShapeAnalysis(GeometryBase Geometry, ObjectFrame Frame, Point3d[] Samples, int NSamples)
    {
        /// <param name="geometry">A Curve, Brep, Mesh or PointCloud.</param>
        /// <param name="n">Target number of samples (curves and Breps only).</param>
        /// <param name="options">Orientation settings; null = <see cref="AnalysisOptions.Legacy"/> (frames as before v0.7).</param>
        public static ShapeAnalysis Analyze(GeometryBase geometry, int n, AnalysisOptions options = null)
        {
            options ??= AnalysisOptions.Legacy;
            Point3d[] samples;
            int nSamples;
            Point3d legacyCentroid;
            GeometryBase analysed = geometry;
            switch (geometry)
            {
                case Curve curve:
                    samples = Discretizer.DiscretizeCurve(curve, n);
                    nSamples = n;
                    legacyCentroid = PCA.GetCentroid(curve, samples);
                    break;
                case Brep brep:
                    samples = Discretizer.DiscretizeBrep(brep, n).ToArray();
                    nSamples = samples.Length;
                    legacyCentroid = PCA.GetCentroid(samples);
                    break;
                case Mesh mesh:
                    samples = mesh.Vertices.ToPoint3dArray();
                    nSamples = mesh.Vertices.Count;
                    legacyCentroid = PCA.GetCentroid(samples);
                    break;
                case PointCloud cloud:
                    samples = cloud.GetPoints();
                    nSamples = samples.Length;
                    legacyCentroid = PCA.GetCentroid(samples);
                    analysed = new PointCloud(samples);
                    break;
                case null:
                    throw new ArgumentNullException(nameof(geometry));
                default:
                    throw new ArgumentException("Unsupported geometry type " + geometry.GetType().Name +
                                                " (convert it with ObjectInput.Normalize first)", nameof(geometry));
            }

            ObjectFrame frame = options.Mode == OrientationMode.Legacy
                ? LegacyFrame(analysed, samples, legacyCentroid)
                : ShapeFrame(analysed, samples, options);
            return new ShapeAnalysis(analysed, frame, samples, nSamples);
        }

        /// <summary>The pre-v0.7 frame: unweighted PCA of the samples, centroid as before.</summary>
        private static ObjectFrame LegacyFrame(GeometryBase geometry, Point3d[] samples, Point3d centroid)
        {
            Vector3d[] v = PCA.PCAvectors(samples, true);
            return ObjectFrame.FromPlane(geometry, centroid, new Plane(centroid, v[0], v[1]), v[0], v[1], v[2])
                with { Mode = OrientationMode.Legacy };
        }

        // ------------------------------------------------------------------------------------------- shape frames

        private static ObjectFrame ShapeFrame(GeometryBase geometry, Point3d[] samples, AnalysisOptions options)
        {
            // A box Brep: the frame is its edges, exactly (Auto only; PCA/MinBox are explicit requests).
            if (options.Mode == OrientationMode.Auto && geometry is Brep box && box.IsBox() &&
                BoxAxes(box, out Vector3d[] axes, out bool tieXY, out bool tieYZ))
            {
                // equal edge lengths: only the equal axes are interchangeable
                if (tieXY && tieYZ) axes = ClosestToWorld(axes);
                else if (tieXY) axes = Pick(axes[0], axes[1], axes[2], Vector3d.XAxis, Vector3d.YAxis, xFirst: true);
                else if (tieYZ) axes = Pick(axes[1], axes[2], axes[0], Vector3d.YAxis, Vector3d.ZAxis, xFirst: false);
                Point3d center = box.GetBoundingBox(true).Center;
                return MakeFrame(geometry, center, axes, tieXY || tieYZ ? OrientationResolution.Symmetric : OrientationResolution.BoxEdges,
                    double.PositiveInfinity, double.PositiveInfinity, OrientationMode.Auto);
            }

            SecondMoments moments = Moments(geometry, samples, out List<Point3d> outline);
            var c = moments.Centroid;
            var centroid = new Point3d(c.X, c.Y, c.Z);
            moments.Principal(out double[] lambda, out double[][] vec);
            var e = new Vector3d[3];
            for (int k = 0; k < 3; k++) e[k] = new Vector3d(vec[k][0], vec[k][1], vec[k][2]);
            double g12 = Ratio(lambda[0], lambda[1], lambda[0]), g23 = Ratio(lambda[1], lambda[2], lambda[0]);

            if (options.Mode == OrientationMode.Override)
                return MakeFrame(geometry, centroid, OverrideAxes(options, e), OrientationResolution.None, g12, g23, OrientationMode.Override, canonical: false);

            Vector3d[] result = { e[0], e[1], Vector3d.CrossProduct(e[0], e[1]) };
            var resolved = OrientationResolution.None;
            double limit = 1 + options.Tolerance;
            bool amb12 = g12 < limit, amb23 = g23 < limit;

            if (options.Mode == OrientationMode.MinBox || (options.Mode == OrientationMode.Auto && amb12 && !amb23))
            {
                // Z = e3; X/Y from the rectangle in the e1-e2 plane
                result = InPlane(outline, centroid, e[0], e[2], options.Tolerance, Vector3d.XAxis, Vector3d.YAxis, true, out bool square);
                resolved = square ? OrientationResolution.Symmetric : OrientationResolution.MinBoxXY;
            }
            else if (options.Mode == OrientationMode.Auto && amb23 && !amb12)
            {
                // X = e1; Y/Z from the rectangle in the e2-e3 plane (cross-section of a square or round member)
                result = InPlane(outline, centroid, e[1], e[0], options.Tolerance, Vector3d.YAxis, Vector3d.ZAxis, false, out bool square);
                resolved = square ? OrientationResolution.Symmetric : OrientationResolution.MinBoxYZ;
            }
            else if (options.Mode == OrientationMode.Auto && amb12 && amb23)
            {
                // isotropic (sphere-like): no preferred direction at all
                result = new[] { Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis };
                resolved = OrientationResolution.Symmetric;
            }

            return MakeFrame(geometry, centroid, result, resolved, g12, g23, options.Mode);
        }

        private static ObjectFrame MakeFrame(GeometryBase geometry, Point3d centroid, Vector3d[] axes, OrientationResolution resolved,
            double g12, double g23, OrientationMode mode, bool canonical = true)
        {
            axes[0].Unitize();
            axes[1].Unitize();
            if (canonical) PCA.Canonicalize(axes);
            else
            {
                axes[2] = Vector3d.CrossProduct(axes[0], axes[1]);
                axes[2].Unitize();
            }
            return ObjectFrame.FromPlane(geometry, centroid, new Plane(centroid, axes[0], axes[1]), axes[0], axes[1], axes[2])
                with { Mode = mode, Resolved = resolved, Gap12 = g12, Gap23 = g23 };
        }

        /// <summary>λa/λb, with λ below 1e-12 of the largest treated as 0 (∞ when only the denominator vanishes, 1 when both do).</summary>
        private static double Ratio(double a, double b, double scale)
        {
            double eps = 1e-12 * Math.Max(scale, double.Epsilon);
            if (b <= eps) return a <= eps ? 1 : double.PositiveInfinity;
            return a / b;
        }

        /// <summary>
        /// Weighted moments of the shape, and the points whose 2D hull bounds it (for the minimum-area rectangle):
        /// mesh triangles (by area), Brep render-mesh triangles, closed planar curves as their region (a signed fan),
        /// other curves as polyline segments (by length), points (uniform).
        /// </summary>
        private static SecondMoments Moments(GeometryBase geometry, Point3d[] samples, out List<Point3d> outline)
        {
            var m = new SecondMoments();
            switch (geometry)
            {
                case Mesh mesh:
                    AddMesh(m, mesh);
                    outline = new List<Point3d>(samples);
                    break;
                case Brep brep:
                    {
                        var joined = new Mesh();
                        Mesh[] parts = Mesh.CreateFromBrep(brep, MeshingParameters.FastRenderMesh);
                        if (parts != null) foreach (Mesh p in parts) joined.Append(p);
                        if (joined.Faces.Count > 0)
                        {
                            AddMesh(m, joined);
                            outline = new List<Point3d>(joined.Vertices.ToPoint3dArray());
                            outline.AddRange(samples);
                        }
                        else
                        {
                            foreach (Point3d p in samples) m.AddPoint(p.X, p.Y, p.Z);
                            outline = new List<Point3d>(samples);
                        }
                        break;
                    }
                case Curve curve:
                    {
                        Polyline poly = CurvePolyline(curve);
                        outline = new List<Point3d>(poly);
                        if (curve.IsClosed && curve.TryGetPlane(out Plane plane) && poly.Count >= 4)
                        {
                            // The enclosed region: a signed triangle fan from the first vertex. Exact for a simple outline;
                            // the overall sign is made positive (the fitted plane's normal direction is arbitrary).
                            Point3d o = poly[0];
                            var area = new double[poly.Count];
                            double total = 0;
                            for (int i = 1; i + 1 < poly.Count; i++)
                            {
                                area[i] = 0.5 * (Vector3d.CrossProduct(poly[i] - o, poly[i + 1] - o) * plane.ZAxis);
                                total += area[i];
                            }
                            if (Math.Abs(total) > 0)
                            {
                                double sign = Math.Sign(total);
                                for (int i = 1; i + 1 < poly.Count; i++)
                                {
                                    Point3d a = poly[i], b = poly[i + 1];
                                    m.AddTriangle(o.X, o.Y, o.Z, a.X, a.Y, a.Z, b.X, b.Y, b.Z, sign * area[i]);
                                }
                                break;
                            }
                        }
                        for (int i = 0; i + 1 < poly.Count; i++)
                            m.AddSegment(poly[i].X, poly[i].Y, poly[i].Z, poly[i + 1].X, poly[i + 1].Y, poly[i + 1].Z);
                        if (m.Weight == 0) foreach (Point3d p in samples) m.AddPoint(p.X, p.Y, p.Z);
                        break;
                    }
                default:
                    foreach (Point3d p in samples) m.AddPoint(p.X, p.Y, p.Z);
                    outline = new List<Point3d>(samples);
                    break;
            }
            return m;
        }

        private static void AddMesh(SecondMoments m, Mesh mesh)
        {
            Point3d[] v = mesh.Vertices.ToPoint3dArray();
            int[] t = mesh.Faces.ToIntArray(true);
            for (int k = 0; k + 2 < t.Length; k += 3)
            {
                Point3d a = v[t[k]], b = v[t[k + 1]], c = v[t[k + 2]];
                m.AddTriangle(a.X, a.Y, a.Z, b.X, b.Y, b.Z, c.X, c.Y, c.Z);
            }
            if (m.Weight == 0) foreach (Point3d p in v) m.AddPoint(p.X, p.Y, p.Z);
        }

        /// <summary>A polyline within 1e-4 of the curve's size (no document tolerance in the geometry layer).</summary>
        private static Polyline CurvePolyline(Curve curve)
        {
            if (curve.TryGetPolyline(out Polyline exact)) return exact;
            double size = curve.GetBoundingBox(false).Diagonal.Length;
            PolylineCurve pc = curve.ToPolyline(size * 1e-4, 0.02, 0, 0);
            if (pc != null && pc.TryGetPolyline(out Polyline p)) return p;
            return new Polyline(Discretizer.DiscretizeCurve(curve, 200));
        }

        /// <summary>
        /// Axes from the minimum-area rectangle of <paramref name="points"/> projected onto the plane through
        /// <paramref name="origin"/> with normal <paramref name="normal"/> (<paramref name="u"/> lies in it).
        /// With <paramref name="xFirst"/> the rectangle gives X (long side) and Y, and the normal is Z (ambiguous X/Y);
        /// otherwise the normal is X and the rectangle gives Y (long side) and Z (ambiguous cross-section). When the
        /// rectangle is square (sides within <see cref="SquareTolerance"/>), the side closest to <paramref name="preferred"/> (or <paramref name="fallback"/>
        /// when that is normal to the plane) comes first: a deterministic choice among equivalent sides.
        /// </summary>
        /// <summary>Relative difference below which the sides of a minimum-area rectangle count as equal (0.1%).</summary>
        public const double SquareTolerance = 1e-3;

        private static Vector3d[] InPlane(List<Point3d> points, Point3d origin, Vector3d u, Vector3d normal, double tolerance,
            Vector3d preferred, Vector3d fallback, bool xFirst, out bool square)
        {
            normal.Unitize();
            u.Unitize();
            Vector3d v = Vector3d.CrossProduct(normal, u);
            v.Unitize();
            var xs = new double[points.Count];
            var ys = new double[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                Vector3d d = points[i] - origin;
                xs[i] = d * u;
                ys[i] = d * v;
            }
            Rectangle2D r = MinAreaRectangle.Compute(xs, ys);
            Vector3d side = u * r.Ux + v * r.Uy;
            Vector3d other = Vector3d.CrossProduct(normal, side);
            // Side lengths are measured, not estimated like variances, so "square" is much stricter than τ: a 1000 × 1010
            // slab has a long side.
            square = r.Length <= 0 || r.Width >= r.Length * (1 - SquareTolerance);
            if (square) return Pick(side, other, normal, preferred, fallback, xFirst);
            return xFirst ? new[] { side, other, normal } : new[] { normal, side, Vector3d.CrossProduct(normal, side) };
        }

        /// <summary>
        /// Two interchangeable axes a, b (normal n): the one (either sign) closest to <paramref name="preferred"/>
        /// (<paramref name="fallback"/> when preferred is along n) comes first. xFirst: [first, second, n]; otherwise
        /// [n, first, n × first].
        /// </summary>
        private static Vector3d[] Pick(Vector3d a, Vector3d b, Vector3d n, Vector3d preferred, Vector3d fallback, bool xFirst)
        {
            Vector3d target = Math.Abs(n * preferred) > 0.999 ? fallback : preferred;
            Vector3d first = Math.Abs(b * target) > Math.Abs(a * target) + 1e-9 ? b : a;
            if (first * target < 0) first = -first;
            return xFirst
                ? new[] { first, Vector3d.CrossProduct(n, first), n }
                : new[] { n, first, Vector3d.CrossProduct(n, first) };
        }

        /// <summary>Three interchangeable axes (a cube): X closest to world X, then Y closest to world Y.</summary>
        private static Vector3d[] ClosestToWorld(Vector3d[] axes)
        {
            int bestX = 0;
            for (int k = 1; k < 3; k++)
                if (Math.Abs(axes[k] * Vector3d.XAxis) > Math.Abs(axes[bestX] * Vector3d.XAxis) + 1e-9) bestX = k;
            int bestY = -1;
            for (int k = 0; k < 3; k++)
            {
                if (k == bestX) continue;
                if (bestY < 0 || Math.Abs(axes[k] * Vector3d.YAxis) > Math.Abs(axes[bestY] * Vector3d.YAxis) + 1e-9) bestY = k;
            }
            Vector3d x = axes[bestX] * Vector3d.XAxis < 0 ? -axes[bestX] : axes[bestX];
            Vector3d y = axes[bestY] * Vector3d.YAxis < 0 ? -axes[bestY] : axes[bestY];
            return new[] { x, y, Vector3d.CrossProduct(x, y) };
        }

        /// <summary>The three edge directions of a box Brep, longest first; ties flag equal lengths (relative 1e-6).</summary>
        private static bool BoxAxes(Brep box, out Vector3d[] axes, out bool tieXY, out bool tieYZ)
        {
            axes = null;
            tieXY = tieYZ = false;
            var dirs = new List<Vector3d>();
            var lengths = new List<double>();
            foreach (BrepEdge edge in box.Edges)
            {
                Vector3d d = edge.PointAtEnd - edge.PointAtStart;
                double len = d.Length;
                if (len <= 0) continue;
                d /= len;
                int k = dirs.FindIndex(r => Math.Abs(r * d) > 0.999);
                if (k < 0) { dirs.Add(d); lengths.Add(len); }
                else lengths[k] = Math.Max(lengths[k], len);
            }
            if (dirs.Count != 3) return false;
            int[] order = { 0, 1, 2 };
            Array.Sort(order, (a, b) => lengths[b].CompareTo(lengths[a]));
            axes = new[] { dirs[order[0]], dirs[order[1]], dirs[order[2]] };
            double l0 = lengths[order[0]], l1 = lengths[order[1]], l2 = lengths[order[2]];
            tieXY = l1 >= l0 * (1 - 1e-6);
            tieYZ = l2 >= l1 * (1 - 1e-6);
            return true;
        }

        /// <summary>X from the override (plane or vector), Y from the principal axis most perpendicular to it.</summary>
        private static Vector3d[] OverrideAxes(AnalysisOptions options, Vector3d[] e)
        {
            if (options.OverridePlane is Plane p && p.IsValid)
                return new[] { p.XAxis, p.YAxis, p.ZAxis };
            Vector3d x = options.OverrideX ?? e[0];
            x.Unitize();
            Vector3d y = Vector3d.Unset;
            foreach (Vector3d cand in e)
            {
                Vector3d proj = cand - (cand * x) * x;
                if (proj.Length > 0.5) { y = proj; break; }
            }
            if (!y.IsValid) y = Vector3d.CrossProduct(x, Math.Abs(x.Z) < 0.9 ? Vector3d.ZAxis : Vector3d.XAxis);
            y.Unitize();
            return new[] { x, y, Vector3d.CrossProduct(x, y) };
        }
    }
}
