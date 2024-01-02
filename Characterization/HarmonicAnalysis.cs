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
using MathNet.Numerics;

namespace DigitalCircularityToolkit.Characterization
{
    internal static class HarmonicAnalysis
    {
        public static double[] Harmonic(double[] signature)
        {
            int n = signature.Length;

            int ns = n % 2 == 0 ? n - 2 : n - 1;

            Fourier.ForwardReal(signature, ns);

            double[] descriptor = new double[ns/2-1];

            for (int i = 1; i < ns/2; i++)
            {
                descriptor[i-1] = Math.Abs(signature[i]);
            }

            return descriptor;
        }
        public static double[] Harmonic(System.Numerics.Complex[] signature)
        {
            Fourier.Forward(signature);

            double[] descriptor = new double[signature.Length-1];

            for (int i = 1; i < signature.Length; i++)
            {
                descriptor[i-1] = signature[i].Magnitude;
            }

            return descriptor;
        }


        public static System.Numerics.Complex[] RadialSignature2DComplex(Point3d[] points, Point3d centroid, Rhino.Geometry.Plane plane, out Line[] rays, out double[] lengths)
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

                signature[i] = new System.Numerics.Complex(planar_point.X, planar_point.Y);
            }

            return signature;
        }

    }
}
