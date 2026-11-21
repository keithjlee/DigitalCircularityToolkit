using DigitalCircularityToolkit.GeometryProcessing;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalCircularityToolkit.Objects
{
    public class SphericalObject : DesignObject
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
        public Sphere EffectiveSphere;
        public Plane Plane
        {
            get
            {
                return GetPlane();
            }
        }
        public Mesh Hull
        {
            get
            {
                return Hull3D;
            }
        }

        public SphericalObject() { }

        protected override void RepopulateDerived()
        {
            // the box center moved with the new frame: recompute the enclosing sphere about it
            Radius = GetTrueRadius();
            EffectiveSphere = new Sphere(Localbox.Center, Radius * RadiusBuffer);
        }

        protected override void TransformDerived(Transform t)
        {
            Sphere sphere = EffectiveSphere;
            sphere.Transform(t);
            EffectiveSphere = sphere;
        }

        public SphericalObject(DesignObject obj)
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
            this.ID = obj.ID;

            Quantity = 1;
            Radius = GetTrueRadius();
            RadiusBuffer = 1;
        }

        public SphericalObject(Curve curve, int n_samples, int qty, double radius_buffer, Vector3d pca_override)
        {
            Populate(curve, n_samples);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            Radius = GetTrueRadius();
            RadiusBuffer = radius_buffer;
            EffectiveSphere = new Sphere(Localbox.Center, Radius * RadiusBuffer);
        }

        public SphericalObject(Brep brep, int n_samples, int qty, double radius_buffer, Vector3d pca_override)
        {
            Populate(brep, n_samples);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            Radius = GetTrueRadius();
            RadiusBuffer = radius_buffer;
            EffectiveSphere = new Sphere(Localbox.Center, Radius * RadiusBuffer);
        }

        public SphericalObject(PointCloud points, int qty, double radius_buffer, Vector3d pca_override)
        {
            Populate(points.GetPoints().ToList());
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            Radius = GetTrueRadius();
            RadiusBuffer = radius_buffer;
            EffectiveSphere = new Sphere(Localbox.Center, Radius * RadiusBuffer);
        }

        public SphericalObject(Mesh mesh, int qty, double radius_buffer, Vector3d pca_override)
        {
            Populate(mesh);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            Radius = GetTrueRadius();
            RadiusBuffer = radius_buffer;
            EffectiveSphere = new Sphere(Localbox.Center, Radius * RadiusBuffer);
        }


        public double GetTrueRadius()
        {
            double r = 0;
            // every sample point (NSamples undercounts open curves, which have n + 1 points)
            for (int i = 0; i < SampledPoints.Length; i++)
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
