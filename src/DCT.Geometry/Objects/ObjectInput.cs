using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    /// <summary>
    /// Input geometry for the object constructors (DESIGN_OBJECT_SPEC §4). The analysis supports Curve, Brep,
    /// Mesh and PointCloud; other types are converted here or rejected with a message (B8).
    /// </summary>
    public static class ObjectInput
    {
        /// <summary>
        /// Returns geometry the analysis can use, or null with <paramref name="error"/> set.
        /// The input is never modified (conversions create new geometry).
        /// </summary>
        /// <summary>
        /// Null when the counts are usable, otherwise an error message. NumSamples only matters for curves and Breps
        /// (meshes and point clouds use all their vertices/points).
        /// </summary>
        public static string CheckCounts(GeometryBase geo, int nSamples, int quantity)
        {
            if ((geo is Curve || geo is Brep) && nSamples < 1) return "NumSamples must be at least 1";
            if (quantity < 0) return "Quantity must be 0 or more";
            return null;
        }

        public static GeometryBase Normalize(GeometryBase geo, out string error)
        {
            error = null;
            switch (geo)
            {
                case null:
                    error = "Geometry is null";
                    return null;
                case Curve _:
                case Brep _:
                case Mesh _:
                case PointCloud _:
                    return geo;
                case Extrusion extrusion:
                    return Converted(extrusion.ToBrep(true), "Extrusion", out error);
                case Surface surface:
                    return Converted(surface.ToBrep(), "Surface", out error);
                case SubD subd:
                    return Converted(Mesh.CreateFromSubD(subd, 1), "SubD", out error);
                default:
                    error = "Unsupported geometry type " + geo.GetType().Name +
                            ". Supported: curves, Breps/surfaces/extrusions, meshes, SubDs and point clouds.";
                    return null;
            }
        }

        private static GeometryBase Converted(GeometryBase converted, string from, out string error)
        {
            error = converted == null ? "Could not convert the " + from + " for analysis" : null;
            return converted;
        }
    }
}
