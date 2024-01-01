using DigitalCircularityToolkit.Objects;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.IntegralTransforms;
using Accord;
using Grasshopper.Kernel.Types;
using System.Numerics;

namespace DigitalCircularityToolkit.Characterization
{
    internal static class HarmonicAnalysis
    {
        public static double[] Harmonic2DReal (DesignObject obj, int n)
        {
            double[] signature = HullSignature2D(obj, n);

            int ns = n % 2 == 0 ? n - 2 : n - 1;

            Fourier.ForwardReal(signature, ns);
            
            return signature;
        }

        public static double[] Harmonic2D(DesignObject obj, int n)
        {
            System.Numerics.Complex[] signature = HullSignature2DComplex(obj, n);

            Fourier.Forward(signature);

            double[] magnitudes = new double[signature.Length];

            for (int i = 0; i < signature.Length; i++)
            {
                magnitudes[i] = signature[i].Magnitude;
            }

            return magnitudes;
        }

        public static double[] HullSignature2D(DesignObject obj, int n, out PolylineCurve hull, out List<Point3d> sample_points, out Point3d start_point)
        {
            // extract planar hull
            hull = obj.Hull2D.ToPolylineCurve();

            // orient the start of curve to intersect with the local plane positive X axis
            Line local_x = new Line(obj.LocalPlane.Origin, obj.LocalPlane.XAxis);

            // find intersection
            CurveIntersections intersections = Intersection.CurveLine(hull, local_x, 1e-6, 1e-6);
            start_point = intersections[0].PointA;

            // set curve start to start_point
            hull.SetStartPoint(start_point);

            // Iterate over hull
            sample_points = new List<Point3d>();
            double[] radial_distances = new double[n];
            double increment = 1 / (double)n;

            for (int i = 0; i < n; i++)
            {
                var point = hull.PointAtNormalizedLength(increment * i);
                radial_distances[i] = point.DistanceTo(obj.LocalPlane.Origin);
                sample_points.Add(point);
            }

            return radial_distances;
        }

        public static double[] HullSignature2D(DesignObject obj, int n)
        {
            // extract planar hull
            PolylineCurve hull = obj.Hull2D.ToPolylineCurve();

            // Iterate over hull
            double[] radial_distances = new double[n];
            double increment = 1 / (double)n;

            for (int i = 0; i < n; i++)
            {
                var point = hull.PointAtNormalizedLength(increment * i);
                radial_distances[i] = point.DistanceTo(obj.LocalPlane.Origin);
            }

            return radial_distances;
        }

        public static double[] HullSignature2D(Curve curve, int n, out Line[] rays)
        {
            // divide curve and extract points
            curve.DivideByCount(n, true, out Point3d[] points);

            // centroid
            var centroid = LengthMassProperties.Compute(curve).Centroid;

            double[] signature = new double[points.Length];
            rays = new Line[points.Length];

            for (int i = 0; i < signature.Length; i++)
            {
                rays[i] = new Line(centroid, points[i]);
                signature[i] = rays[i].Length;
            }

            return signature;
        }


        public static System.Numerics.Complex[] HullSignature2DComplex(DesignObject obj, int n)
        {
            // extract planar hull
            PolylineCurve hull = obj.Hull2D.ToPolylineCurve();

            // Iterate over hull
            System.Numerics.Complex[] radial_distances = new System.Numerics.Complex[n];
            double increment = 1 / (double)n;

            for (int i = 0; i < n; i++)
            {
                var point = hull.PointAtNormalizedLength(increment * i);
                radial_distances[i] = new System.Numerics.Complex(point.DistanceTo(obj.LocalPlane.Origin), 0);
            }

            return radial_distances;
        }

        public static System.Numerics.Complex[] HullSignature2DComplex(Point3d[] points, Point3d centroid, Rhino.Geometry.Plane plane, out Line[] rays, out double[] lengths)
        {
            int n = points.Length;

            System.Numerics.Complex[] signature = new System.Numerics.Complex[n];
            rays = new Line[n];
            lengths = new double[n];
            

            // extract distance on plane
            for (int i = 0; i < n; i++)
            {
                plane.RemapToPlaneSpace(points[i], out Point3d planar_point);

                rays[i] = new Line(centroid, points[i]);
                lengths[i] = rays[i].Length;

                var diff = planar_point - centroid;
                signature[i] = new System.Numerics.Complex(diff.X, diff.Y);
            }

            return signature;
        }

        public static double PlanarDistance(Point3d p1, Point3d p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;

            return Math.Sqrt(dy* dy + dx * dx);
        }

    }
}
