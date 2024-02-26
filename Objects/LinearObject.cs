using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DigitalCircularityToolkit.GeometryProcessing;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
	public class LinearObject : DesignObject
	{
		public int Quantity;
        public double LengthBuffer;
        public Line EffectiveLine
        {
            get { return GetEffectiveLine(); }
        }
        public double EffectiveLength
        {
            get
            {
                return Length * LengthBuffer;
            }
        }
        public double Area;
        public Plane CrossSectionPlane
        {
            get { return GetCrossSectionPlane(); }
        }
        public Polyline Hull
        {
            get
            {
                return Hull2D;
            }
        }

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
            this.ID = obj.ID;

            Quantity = 1;
            LengthBuffer = 1;

            GetApproxArea();
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

        }

        private Plane GetCrossSectionPlane()
        {
            return new Plane(Localbox.Center, PCA2, PCA3);
        }

        private void GetApproxArea()
        {
            Area = Width * Height;
        }

        private Line GetEffectiveLine()
        {
            Point3d point_at_start = Localbox.Center - PCA1 * EffectiveLength / 2;
            Point3d point_at_end = Localbox.Center + PCA1 * EffectiveLength / 2;

            return new Line(point_at_start, point_at_end);
        }
	}
}