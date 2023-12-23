using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class BoxObject : DesignObject
    {
        public int Quantity;
        public double LengthBuffer;
        public double EffectiveLength
        {
            get
            {
                return LengthBuffer * Length;
            }
        }
        public double WidthBuffer;
        public double EffectiveWidth
        {
            get
            {
                return WidthBuffer * Width;
            }
        }
        public double HeightBuffer;
        public double EffectiveHeight
        {
            get
            {
                return HeightBuffer * Height;
            }
        }
        public Box EffectiveBox;
        public double EffectiveVolume
        {
            get { return LengthBuffer * Length * WidthBuffer * Width * HeightBuffer * Height; }
        }
        public Plane Plane
        {
            get
            {
                return GetPlane();
            }
        }

        public BoxObject() { }

        public BoxObject(Curve curve, int n_samples, int qty, double length_buffer, double width_buffer, double height_buffer, Vector3d pca_override)
        {
            Populate(curve, n_samples);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            LengthBuffer = length_buffer;
            WidthBuffer = width_buffer;
            HeightBuffer = height_buffer;

            EffectiveBox = GetEffectiveBox();
        }

        public BoxObject(Brep brep, int n_samples, int qty, double length_buffer, double width_buffer, double height_buffer, Vector3d pca_override)
        {
            Populate(brep, n_samples);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            LengthBuffer = length_buffer;
            WidthBuffer = width_buffer;
            HeightBuffer = height_buffer;

            EffectiveBox = GetEffectiveBox();
        }

        public BoxObject(PointCloud points, int qty, double length_buffer, double width_buffer, double height_buffer, Vector3d pca_override)
        {
            Populate(points.GetPoints().ToList());
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            LengthBuffer = length_buffer;
            WidthBuffer = width_buffer;
            HeightBuffer = height_buffer;

            EffectiveBox = GetEffectiveBox();
        }

        public BoxObject(Mesh mesh, int qty, double length_buffer, double width_buffer, double height_buffer, Vector3d pca_override)
        {
            Populate(mesh);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            LengthBuffer = length_buffer;
            WidthBuffer = width_buffer;
            HeightBuffer = height_buffer;

            EffectiveBox = GetEffectiveBox();
        }

        public Box GetEffectiveBox()
        {
            var scaler = Transform.Scale(LocalPlane, LengthBuffer, WidthBuffer, HeightBuffer);

            Box box = new Box(Localbox);
            box.Transform(scaler);
            return box;
        }

        public Plane GetPlane()
        {
            return new Plane(Localbox.Center, PCA1, PCA2);
        }
    }
}
