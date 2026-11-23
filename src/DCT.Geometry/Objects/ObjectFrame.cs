using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    /// <summary>
    /// Where a design object is, how it is oriented and how big it is (DESIGN_OBJECT_SPEC §3). Immutable.
    /// <see cref="LocalPlane"/> has the axes <see cref="PCA1"/>, <see cref="PCA2"/> and its origin at the centre of
    /// <see cref="Localbox"/>, the box of the geometry aligned to those axes. <see cref="Boundingbox"/> is the same box in
    /// plane coordinates. <see cref="Centroid"/> is the analysis centroid (samples, or the area centroid of a closed
    /// planar curve), which generally differs from the box centre.
    /// </summary>
    public readonly record struct ObjectFrame
    {
        public Point3d Centroid { get; init; }
        public Vector3d PCA1 { get; init; }
        public Vector3d PCA2 { get; init; }
        public Vector3d PCA3 { get; init; }
        public Plane LocalPlane { get; init; }
        public Box Localbox { get; init; }
        public BoundingBox Boundingbox { get; init; }

        /// <summary>How the axes were chosen (Legacy for frames built as before v0.7; Override after <c>WithFrame</c>).</summary>
        public OrientationMode Mode { get; init; }
        /// <summary>Which ambiguity rule set the axes, if any.</summary>
        public OrientationResolution Resolved { get; init; }
        /// <summary>λ₁/λ₂ of the shape's principal variances (∞ when λ₂ is 0); near 1 = PCA1/PCA2 ambiguous. NaN for Legacy frames.</summary>
        public double Gap12 { get; init; }
        /// <summary>λ₂/λ₃; near 1 = PCA2/PCA3 ambiguous (square or round cross-section). NaN for Legacy frames.</summary>
        public double Gap23 { get; init; }

        /// <summary>Extent along PCA1.</summary>
        public double Length => Boundingbox.Corner(true, true, true).DistanceTo(Boundingbox.Corner(false, true, true));
        /// <summary>Extent along PCA2.</summary>
        public double Width => Boundingbox.Corner(true, true, true).DistanceTo(Boundingbox.Corner(true, false, true));
        /// <summary>Extent along PCA3.</summary>
        public double Height => Boundingbox.Corner(true, true, true).DistanceTo(Boundingbox.Corner(true, true, false));

        /// <summary>
        /// The frame of <paramref name="geometry"/> for the given axes: the box is measured in <paramref name="plane"/>
        /// (whose origin only affects the plane coordinates of <see cref="Boundingbox"/>), then the plane origin moves to
        /// the box centre. The axes are passed separately so analysed PCA vectors are stored exactly (the Plane
        /// constructor re-orthonormalizes).
        /// </summary>
        public static ObjectFrame FromPlane(GeometryBase geometry, Point3d centroid, Plane plane, Vector3d pca1, Vector3d pca2, Vector3d pca3)
        {
            BoundingBox bbox = geometry.GetBoundingBox(plane, out Box box);
            plane.Origin = box.Center;
            return new ObjectFrame
            {
                Mode = OrientationMode.Legacy,
                Gap12 = double.NaN,
                Gap23 = double.NaN,
                Centroid = centroid,
                PCA1 = pca1,
                PCA2 = pca2,
                PCA3 = pca3,
                LocalPlane = plane,
                Localbox = box,
                Boundingbox = bbox,
            };
        }

        /// <summary>The frame moved by a rigid transform. <see cref="Boundingbox"/> is in plane coordinates, so it is unchanged.</summary>
        public ObjectFrame Transformed(Transform t)
        {
            Point3d c = Centroid; c.Transform(t);
            Vector3d v1 = PCA1, v2 = PCA2, v3 = PCA3;
            v1.Transform(t); v2.Transform(t); v3.Transform(t);
            Plane plane = LocalPlane; plane.Transform(t);
            Box box = Localbox; box.Transform(t);
            return this with { Centroid = c, PCA1 = v1, PCA2 = v2, PCA3 = v3, LocalPlane = plane, Localbox = box };
        }
    }
}
