using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    /// <summary>The sphere about the box centre that encloses every sample, radius scaled by RadiusBuffer.</summary>
    public sealed record SphericalObject : DesignObject
    {
        private SphericalObject(ShapeAnalysis analysis) : base(analysis) { }

        /// <param name="geometry">A Curve, Brep, Mesh or PointCloud.</param>
        /// <param name="n">Target number of samples (curves and Breps).</param>
        /// <param name="options">Orientation settings; null = Legacy (the frames of v0.6 and earlier).</param>
        public static SphericalObject Create(GeometryBase geometry, int n, int quantity = 1, double radiusBuffer = 1, string id = "NoID", AnalysisOptions options = null) =>
            new SphericalObject(ShapeAnalysis.Analyze(geometry, n, options))
            {
                Quantity = quantity,
                RadiusBuffer = Positive(radiusBuffer, nameof(radiusBuffer)),
                ID = id,
            };

        public double RadiusBuffer { get; init; } = 1;

        private Memo<double> _radius;
        /// <summary>Largest distance from the box centre to a sample (computed once per frame).</summary>
        public double Radius => _radius.Get(Frame, SampledPoints, null, GetTrueRadius);

        public double EffectiveRadius => Radius * RadiusBuffer;

        public Sphere EffectiveSphere => new Sphere(Localbox.Center, EffectiveRadius);

        public Plane Plane => new Plane(Localbox.Center, PCA1, PCA2);

        public Mesh Hull => Hull3D;

        /// <summary>Largest distance from the box centre to any sample (every sample: open curves have n + 1).</summary>
        public double GetTrueRadius()
        {
            if (SampledPoints == null) return 0;
            Point3d center = Localbox.Center;
            double r = 0;
            for (int i = 0; i < SampledPoints.Length; i++)
            {
                double dist = center.DistanceTo(SampledPoints[i]);
                if (dist > r) r = dist;
            }
            return r;
        }
    }
}
