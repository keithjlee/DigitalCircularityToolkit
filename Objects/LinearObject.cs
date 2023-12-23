using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
	public class LinearObject : DesignObject
	{
		public int Quantity;
        public double LengthBuffer;
        public Line EffectiveLine;
        public double EffectiveLength
        {
            get
            {
                return Length * LengthBuffer;
            }
        }
        public double Area;
        public Plane CrossSectionPlane;

		public LinearObject()
		{
		}

        public LinearObject(DesignObject obj)
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
            GetApproxArea();
            GetCrossSectionPlane();
            GetEffectiveLine();
        }

        public LinearObject(Curve curve, int n_samples, int qty, double buffer, double area, Vector3d pca_override)
        {
            Populate(curve, n_samples);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            LengthBuffer = buffer;
            
            if (area <= 0)
            {
                GetApproxArea() ;
            }
            else
            {
                Area = area;
            }

            GetCrossSectionPlane();
            GetEffectiveLine();
        }

        public LinearObject(Brep brep, int n_samples, int qty, double buffer, double area, Vector3d pca_override)
        {
            Populate(brep, n_samples);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            LengthBuffer = buffer;

            if (area <= 0)
            {
                GetApproxArea();
            }
            else
            {
                Area = area;
            }

            GetCrossSectionPlane();
            GetEffectiveLine();
        }

        public LinearObject(PointCloud points, int qty, double buffer, double area, Vector3d pca_override)
        {
            Populate(points.GetPoints().ToList());
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            LengthBuffer = buffer;

            if (area <= 0)
            {
                GetApproxArea();
            }
            else
            {
                Area = area;
            }

            GetCrossSectionPlane();
            GetEffectiveLine();
        }

        public LinearObject(Mesh mesh, int qty, double buffer, double area, Vector3d pca_override)
        {
            Populate(mesh);
            if (pca_override.Length > 0) ObjectAnalysis.OverridePCA(pca_override, this);

            Quantity = qty;
            LengthBuffer = buffer;

            if (area <= 0)
            {
                GetApproxArea();
            }
            else
            {
                Area = area;
            }

            GetCrossSectionPlane();
            GetEffectiveLine();

        }

        private void GetCrossSectionPlane()
        {
            CrossSectionPlane = new Plane(Localbox.Center, PCA2, PCA3);
        }

        private void GetApproxArea()
        {
            Area = Width * Height;
        }

        private void GetEffectiveLine()
        {
            Point3d point_at_start = Localbox.Center - PCA1 * EffectiveLength / 2;
            Point3d point_at_end = Localbox.Center + PCA1 * EffectiveLength / 2;

            EffectiveLine = new Line(point_at_start, point_at_end);
        }
	}
}