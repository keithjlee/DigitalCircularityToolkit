using System;
using System.Linq;
using DigitalCircularityToolkit.Orientation;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    /// <summary>
    /// The analysis of one piece of geometry: samples, PCA frame and box (DESIGN_OBJECT_SPEC §4). This is the only place
    /// that dispatches on the geometry type; every design object is built from its result. Convert other input types
    /// first with <see cref="ObjectInput.Normalize"/>.
    /// </summary>
    /// <param name="Geometry">The analysed geometry (for point clouds, a plain copy of the points).</param>
    /// <param name="Frame">PCA axes, centroid, aligned box.</param>
    /// <param name="Samples">Analysis points (curve/Brep samples, mesh vertices, cloud points).</param>
    /// <param name="NSamples">The requested sample count for curves; the point count otherwise.</param>
    public sealed record ShapeAnalysis(GeometryBase Geometry, ObjectFrame Frame, Point3d[] Samples, int NSamples)
    {
        /// <param name="geometry">A Curve, Brep, Mesh or PointCloud.</param>
        /// <param name="n">Target number of samples (curves and Breps only).</param>
        public static ShapeAnalysis Analyze(GeometryBase geometry, int n)
        {
            switch (geometry)
            {
                case Curve curve:
                    {
                        Point3d[] pts = Discretizer.DiscretizeCurve(curve, n);
                        return Make(curve, pts, n, PCA.GetCentroid(curve, pts));
                    }
                case Brep brep:
                    {
                        Point3d[] pts = Discretizer.DiscretizeBrep(brep, n).ToArray();
                        return Make(brep, pts, pts.Length, PCA.GetCentroid(pts));
                    }
                case Mesh mesh:
                    {
                        Point3d[] pts = mesh.Vertices.ToPoint3dArray();
                        return Make(mesh, pts, mesh.Vertices.Count, PCA.GetCentroid(pts));
                    }
                case PointCloud cloud:
                    {
                        Point3d[] pts = cloud.GetPoints();
                        return Make(new PointCloud(pts), pts, pts.Length, PCA.GetCentroid(pts));
                    }
                case null:
                    throw new ArgumentNullException(nameof(geometry));
                default:
                    throw new ArgumentException("Unsupported geometry type " + geometry.GetType().Name +
                                                " (convert it with ObjectInput.Normalize first)", nameof(geometry));
            }
        }

        private static ShapeAnalysis Make(GeometryBase geometry, Point3d[] samples, int nSamples, Point3d centroid)
        {
            Vector3d[] v = PCA.PCAvectors(samples, true);
            ObjectFrame frame = ObjectFrame.FromPlane(geometry, centroid, new Plane(centroid, v[0], v[1]), v[0], v[1], v[2]);
            return new ShapeAnalysis(geometry, frame, samples, nSamples);
        }
    }
}
