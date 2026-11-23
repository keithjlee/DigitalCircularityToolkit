using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    /// <summary>The aligned box, each dimension scaled by its buffer.</summary>
    public sealed record BoxObject : DesignObject
    {
        private BoxObject(ShapeAnalysis analysis) : base(analysis) { }

        /// <param name="geometry">A Curve, Brep, Mesh or PointCloud.</param>
        /// <param name="n">Target number of samples (curves and Breps).</param>
        public static BoxObject Create(GeometryBase geometry, int n, int quantity = 1, double lengthBuffer = 1, double widthBuffer = 1, double heightBuffer = 1, string id = "NoID") =>
            new BoxObject(ShapeAnalysis.Analyze(geometry, n))
            {
                Quantity = quantity,
                LengthBuffer = Positive(lengthBuffer, nameof(lengthBuffer)),
                WidthBuffer = Positive(widthBuffer, nameof(widthBuffer)),
                HeightBuffer = Positive(heightBuffer, nameof(heightBuffer)),
                ID = id,
            };

        public double LengthBuffer { get; init; } = 1;
        public double WidthBuffer { get; init; } = 1;
        public double HeightBuffer { get; init; } = 1;

        public double EffectiveLength => LengthBuffer * Length;
        public double EffectiveWidth => WidthBuffer * Width;
        public double EffectiveHeight => HeightBuffer * Height;
        public double EffectiveVolume => EffectiveLength * EffectiveWidth * EffectiveHeight;

        public Box EffectiveBox
        {
            get
            {
                Box box = Localbox;
                box.Transform(Transform.Scale(LocalPlane, LengthBuffer, WidthBuffer, HeightBuffer));
                return box;
            }
        }

        public Plane Plane => new Plane(Localbox.Center, PCA1, PCA2);

        public Mesh Hull => Hull3D;
    }
}
