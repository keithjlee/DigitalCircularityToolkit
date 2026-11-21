using System.Collections.Generic;
using Complex = System.Numerics.Complex;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
    /// <summary>
    /// Shared code for RadialSignature and the two Harmonics components. The Fourier math is in
    /// <see cref="FourierDescriptors"/>, which is Rhino-free.
    /// </summary>
    internal static class HarmonicAnalysis
    {
        /// <summary>
        /// Projects the sample points into the plane, whose origin is the analysis centre. Outputs the complex signature
        /// u + iv, the planar distances |u + iv|, and rays from the centre to each projected point.
        /// </summary>
        public static Complex[] RadialSignature2DComplex(Point3d[] points, Rhino.Geometry.Plane plane, out Line[] rays, out double[] lengths)
        {
            int n = points.Length;
            var signature = new Complex[n];
            rays = new Line[n];
            lengths = new double[n];

            for (int i = 0; i < n; i++)
            {
                plane.RemapToPlaneSpace(points[i], out Point3d uv);
                signature[i] = new Complex(uv.X, uv.Y);
                lengths[i] = signature[i].Magnitude;
                rays[i] = new Line(plane.Origin, plane.PointAt(uv.X, uv.Y));
            }
            return signature;
        }

        /// <summary>
        /// Reads a list of complex numbers. Accepts System.Numerics.Complex (from RadialSignature), Grasshopper's own
        /// complex numbers, and plain numbers (imaginary part 0). Returns null and sets <paramref name="error"/> on
        /// anything else.
        /// </summary>
        public static List<Complex> ToComplex(List<IGH_Goo> items, out string error)
        {
            error = null;
            var result = new List<Complex>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                IGH_Goo goo = items[i];
                if (goo is GH_ComplexNumber ghc)
                    result.Add(new Complex(ghc.Value.Real, ghc.Value.Imaginary));
                else if (goo is GH_ObjectWrapper w && w.Value is Complex c)
                    result.Add(c);
                else if (goo is GH_Number num)
                    result.Add(new Complex(num.Value, 0));
                else
                {
                    error = string.Format("Signature item {0} is not a complex number ({1})", i, goo == null ? "null" : goo.TypeName);
                    return null;
                }
            }
            return result;
        }

        /// <summary>
        /// Writes the first FVLength values of the descriptor to output 0 (all of them when FVLength is not supplied).
        /// Adds a Remark when FVLength is larger than the descriptor.
        /// </summary>
        public static void SetFeatureVector(GH_Component component, IGH_DataAccess DA, double[] descriptor)
        {
            int n = descriptor.Length;
            if (DA.GetData(1, ref n))
            {
                if (n < 1)
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "FVLength must be at least 1");
                    return;
                }
                if (n > descriptor.Length)
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, string.Format(
                        "FVLength {0} is more than the {1} values this signature gives; output has {1}. Use more samples for more harmonics.",
                        n, descriptor.Length));
                    n = descriptor.Length;
                }
            }
            var fv = new double[n];
            System.Array.Copy(descriptor, fv, n);
            DA.SetDataList(0, fv);
        }
    }
}
