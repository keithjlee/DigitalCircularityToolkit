using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalCircularityToolkit.Objects
{
    public class PlanarObject : DesignObject
    {
        public int Quantity;
        public double LengthBuffer;
        public double WidthBuffer;
        public double Thickness
        {
            get { return Height; }
            set { }
        }
        public double Dimension1
        {
            get { return LengthBuffer * Length; }
        }
        public double Dimension2
        {
            get { return WidthBuffer * Width; }
        }
        public Plane Plane
        {
            get { return GetPlane(); }
        }
        public PlaneSurface EffectivePlane
        {
            get { return GetEffectivePlane(); }
        }

        public PlanarObject() { }

        public PlanarObject(DesignObject obj)
        {
            this.Centroid = obj.Centroid;
            this.NSamples = obj.NSamples;
            this.SampledPoints = obj.SampledPoints;
            this.PCA1 = obj.PCA1;
            this.PCA2 = obj.PCA2;
            this.PCA3 = obj.PCA3;
            this.Localbox = obj.Localbox;
            this.LocalPlane = obj.LocalPlane;
            this.Boundingbox = obj.Boundingbox;
            this.Geometry = obj.Geometry;
            this.TransformedGeometry = obj.TransformedGeometry;

            Quantity = 1;
            LengthBuffer = 1;
            WidthBuffer = 1;
            Thickness = 1;
        }

        public PlanarObject (Curve curve, int n_samples, int qty, double length_buffer, double width_buffer, double thickness, Vector3d pca_override)
        {
            Populate(curve, n_samples);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            LengthBuffer = length_buffer;
            WidthBuffer = width_buffer;

            // Override thickness if value supplied
            if (thickness > 0)
            {
                Thickness = thickness;
            }
        }
        
        public PlanarObject(Brep brep, int n_samples, int qty, double length_buffer, double width_buffer, double thickness, Vector3d pca_override)
        {
            Populate(brep, n_samples);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            LengthBuffer = length_buffer;
            WidthBuffer = width_buffer;

            // Override thickness if value supplied
            if (thickness > 0)
            {
                Thickness = thickness;
            }
        }

        public PlanarObject(PointCloud points, int qty, double length_buffer, double width_buffer, double thickness, Vector3d pca_override)
        {
            Populate(points.GetPoints().ToList());
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            LengthBuffer = length_buffer;
            WidthBuffer = width_buffer;

            // Override thickness if value supplied
            if (thickness > 0)
            {
                Thickness = thickness;
            }
        }

        public PlanarObject(Mesh mesh, int qty, double length_buffer, double width_buffer, double thickness, Vector3d pca_override)
        {
            Populate(mesh);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            LengthBuffer = length_buffer;
            WidthBuffer = width_buffer;

            // Override thickness if value supplied
            if (thickness > 0)
            {
                Thickness = thickness;
            }
        }

        public Plane GetPlane()
        {
            return new Plane(Localbox.Center, PCA1, PCA2);
        }

        public PlaneSurface GetEffectivePlane()
        {
            Vector3d offset_vector = new Vector3d(Dimension1 / 2 * PCA1 + Dimension2 / 2 * PCA2);

            return new PlaneSurface(Plane, new Interval(-Dimension1/2, Dimension1/2), new Interval(-Dimension2/2, Dimension2/2));
        }
    }
}
