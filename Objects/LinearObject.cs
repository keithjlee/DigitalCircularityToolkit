using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
	public class LinearObject : Object
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

        public LinearObject(Curve curve, int n_samples, int qty, double buffer)
        {
            Populate(curve, n_samples);
            Quantity = qty;
            LengthBuffer = buffer;

            GetCrossSectionPlane();
            GetEffectiveLine();
            GetApproxArea();
        }

        public LinearObject(Curve curve, int n_samples, int qty, double buffer, double area)
        {
            Populate(curve, n_samples);
            Quantity = qty;
            LengthBuffer = buffer;
            Area = area;

            GetCrossSectionPlane();
            GetEffectiveLine();
        }

        public LinearObject(List<Point3d> points, int n_samples, int qty, double buffer)
        {
            Populate(points);
            Quantity = qty;
            LengthBuffer = buffer;

            GetCrossSectionPlane();
            GetEffectiveLine();
            GetApproxArea();
        }

        public LinearObject(List<Point3d> points, int n_samples, int qty, double buffer, double area)
        {
            Populate(points);
            Quantity = qty;
            LengthBuffer = buffer;
            Area = area;

            GetCrossSectionPlane();
            GetEffectiveLine();
        }

        private void GetCrossSectionPlane()
        {
            CrossSectionPlane = new Plane(Centroid, PCA2, PCA3);
        }

        private void GetApproxArea()
        {
            Area = Width * Height;
        }

        private void GetEffectiveLine()
        {
            Point3d point_at_start = Centroid - PCA1 * EffectiveLength / 2;
            Point3d point_at_end = Centroid + PCA1 * EffectiveLength / 2;

            EffectiveLine = new Line(point_at_start, point_at_end);
        }
	}
}