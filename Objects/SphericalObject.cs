using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalCircularityToolkit.Objects
{
    internal class SphericalObject : DesignObject
    {
        public int Quantity;
        public double RadiusBuffer;
        public double Radius;
        public double EffectiveRadius
        {
            get
            {
                return Radius * RadiusBuffer;
            }
        }
        public Sphere EffectiveSphere
        {
            get
            {
                return new Sphere(Localbox.Center, EffectiveRadius);
            }
        }
        public Plane Plane
        {
            get
            {
                return GetPlane();
            }
        }

        public SphericalObject() { }

        public SphericalObject(Curve curve, int n_samples, int qty, double radius_buffer, Vector3d pca_override)
        {
            Populate(curve, n_samples);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            Radius = GetTrueRadius();
            RadiusBuffer = radius_buffer;
        }

        public SphericalObject(Brep brep, int n_samples, int qty, double radius_buffer, Vector3d pca_override)
        {
            Populate(brep, n_samples);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            Radius = GetTrueRadius();
            RadiusBuffer = radius_buffer;
        }

        public SphericalObject(PointCloud points, int qty, double radius_buffer, Vector3d pca_override)
        {
            Populate(points.GetPoints().ToList());
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            Radius = GetTrueRadius();
            RadiusBuffer = radius_buffer;
        }

        public SphericalObject(Mesh mesh, int qty, double radius_buffer, Vector3d pca_override)
        {
            Populate(mesh);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            Radius = GetTrueRadius();
            RadiusBuffer = radius_buffer;
        }


        public double GetTrueRadius()
        {
            double r = 0;
            for (int i = 0; i < NSamples; i++)
            {
                double dist = Localbox.Center.DistanceTo(SampledPoints[i]);
                if (dist > r) r = dist;
            }

            return r;
        }

        public Plane GetPlane()
        {
            return new Plane(Localbox.Center, PCA1, PCA2);
        }
    }
}
