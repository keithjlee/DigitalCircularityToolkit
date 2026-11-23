using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    /// <summary>A line along PCA1: effective length = Length × LengthBuffer, with a cross-section area.</summary>
    public sealed record LinearObject : DesignObject
    {
        private LinearObject(ShapeAnalysis analysis) : base(analysis) { }

        /// <param name="geometry">A Curve, Brep, Mesh or PointCloud.</param>
        /// <param name="n">Target number of samples (curves and Breps).</param>
        /// <param name="options">Orientation settings; null = Legacy (the frames of v0.6 and earlier).</param>
        /// <param name="area">Cross-section area; ≤ 0 estimates it as Width × Height.</param>
        public static LinearObject Create(GeometryBase geometry, int n, int quantity = 1, double lengthBuffer = 1, double area = -1, string id = "NoID", AnalysisOptions options = null) =>
            new LinearObject(ShapeAnalysis.Analyze(geometry, n, options))
            {
                Quantity = quantity,
                LengthBuffer = Positive(lengthBuffer, nameof(lengthBuffer)),
                Area = area,
                ID = id,
            };

        public double LengthBuffer { get; init; } = 1;

        private readonly double _area = -1;
        /// <summary>Cross-section area. Set ≤ 0 to estimate it as Width × Height (then it follows a new frame).</summary>
        public double Area { get => _area > 0 ? _area : Width * Height; init => _area = value; }
        /// <summary>True when <see cref="Area"/> is the Width × Height estimate.</summary>
        public bool AreaEstimated => _area <= 0;

        public double EffectiveLength => Length * LengthBuffer;

        public Line EffectiveLine => new Line(Localbox.Center - PCA1 * EffectiveLength / 2, Localbox.Center + PCA1 * EffectiveLength / 2);

        public Plane CrossSectionPlane => new Plane(Localbox.Center, PCA2, PCA3);

        public Polyline Hull => Hull2D;
    }
}
