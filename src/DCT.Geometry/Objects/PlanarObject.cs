using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    /// <summary>A rectangle in the PCA1-PCA2 plane: Dimension1 × Dimension2 (buffered), with a thickness.</summary>
    public sealed record PlanarObject : DesignObject
    {
        private PlanarObject(ShapeAnalysis analysis) : base(analysis) { }

        /// <param name="geometry">A Curve, Brep, Mesh or PointCloud.</param>
        /// <param name="n">Target number of samples (curves and Breps).</param>
        /// <param name="options">Orientation settings; null = Legacy (the frames of v0.6 and earlier).</param>
        /// <param name="thickness">Thickness; ≤ 0 uses the box height.</param>
        public static PlanarObject Create(GeometryBase geometry, int n, int quantity = 1, double lengthBuffer = 1, double widthBuffer = 1, double thickness = -1, string id = "NoID", AnalysisOptions options = null) =>
            new PlanarObject(ShapeAnalysis.Analyze(geometry, n, options))
            {
                Quantity = quantity,
                LengthBuffer = Positive(lengthBuffer, nameof(lengthBuffer)),
                WidthBuffer = Positive(widthBuffer, nameof(widthBuffer)),
                Thickness = thickness,
                ID = id,
            };

        public double LengthBuffer { get; init; } = 1;
        public double WidthBuffer { get; init; } = 1;

        private readonly double _thickness = -1;
        /// <summary>Thickness. Set ≤ 0 to use the box height (B7: the user value used to be ignored).</summary>
        public double Thickness { get => _thickness > 0 ? _thickness : Height; init => _thickness = value; }

        public double Dimension1 => LengthBuffer * Length;
        public double Dimension2 => WidthBuffer * Width;

        public Plane Plane => new Plane(Localbox.Center, PCA1, PCA2);

        public PlaneSurface EffectivePlane =>
            new PlaneSurface(Plane, new Interval(-Dimension1 / 2, Dimension1 / 2), new Interval(-Dimension2 / 2, Dimension2 / 2));

        public Polyline Hull => Hull2D;
    }
}
