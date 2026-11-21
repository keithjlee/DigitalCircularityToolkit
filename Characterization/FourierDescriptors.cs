using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace DigitalCircularityToolkit.Characterization
{
    /// <summary>
    /// Fourier shape descriptors of a sampled curve signature (Rhino-free).
    /// <para>
    /// The coefficients are Fourier-series coefficients c_k = (1/N) Σ_j s_j e^(-2πi·jk/N), j = 0..N-1. Index k counts
    /// cycles per trip around the curve. Bins k and N-k are the frequencies +k and -k. Because of the 1/N, values are
    /// in model units and do not change with the number of samples. Example: a circle of radius R sampled
    /// counter-clockwise has c_1 = R and every other c_k (k ≠ 0) = 0, whatever N is.
    /// </para>
    /// </summary>
    public static class FourierDescriptors
    {
        /// <summary>
        /// Descriptor of a real signature, such as centroid distances r_j.
        /// <para>
        /// Returns A_1..A_⌊N/2⌋, the amplitude of each harmonic in r(t) = A_0 + Σ A_k cos(k·t + φ_k). That is 2|c_k|,
        /// or |c_k| at the Nyquist bin k = N/2 when N is even. A real signal has |c_-k| = |c_k|, so the bins above
        /// N/2 repeat these values and are left out. The mean A_0 is left out too.
        /// </para>
        /// Invariant to start point, traversal direction, in-plane rotation and mirroring. With
        /// <paramref name="normalize"/>, every amplitude is divided by the mean A_0 (for a distance signature: the
        /// mean radius), which makes it scale-invariant and dimensionless. <paramref name="scale"/> is A_0 either way;
        /// it is 0 when negligible, and then nothing is divided.
        /// </summary>
        public static double[] FromReal(IReadOnlyList<double> signature, bool normalize, out double scale)
        {
            int n = signature.Count;
            var c = new Complex[n];
            for (int j = 0; j < n; j++) c[j] = new Complex(signature[j], 0);
            Fourier.Forward(c, FourierOptions.NoScaling);

            var amplitudes = new double[n / 2];
            for (int k = 1; k <= n / 2; k++)
            {
                double ck = c[k].Magnitude / n;
                amplitudes[k - 1] = 2 * k == n ? ck : 2 * ck;
            }
            scale = Scale(c[0].Magnitude / n, amplitudes);
            if (normalize) Divide(amplitudes, scale);
            return amplitudes;
        }

        /// <summary>
        /// Descriptor of a complex signature z_j = u_j + i·v_j, the sample points in plane coordinates.
        /// <para>
        /// The samples are first made counter-clockwise: if the polygon's signed area is negative, they are
        /// conjugated. Then the result is |c_1|, |c_-1|, |c_2|, |c_-2|, …, which is N-1 values; when N is even the
        /// Nyquist bin appears once, at the end. The order is by |k|, so truncating keeps the lowest frequencies on
        /// both sides. c_0 (position) is left out.
        /// </para>
        /// Invariant to translation, in-plane rotation, start point, traversal direction and mirroring. The last three
        /// matter because a fitted plane's normal, and so the curve's apparent direction, is arbitrary. With
        /// <paramref name="normalize"/>, every value is divided by |c_1| (the radius of the best-fitting circle traced
        /// counter-clockwise), which makes the descriptor scale-invariant; the first value is then always 1.
        /// <paramref name="scale"/> is |c_1| either way; it is 0 when negligible, and then nothing is divided.
        /// </summary>
        public static double[] FromComplex(IReadOnlyList<Complex> signature, bool normalize, out double scale)
        {
            int n = signature.Count;
            bool flip = SignedArea(signature) < 0;
            var c = new Complex[n];
            for (int j = 0; j < n; j++) c[j] = flip ? Complex.Conjugate(signature[j]) : signature[j];
            Fourier.Forward(c, FourierOptions.NoScaling);

            var descriptor = new double[n - 1];
            int i = 0;
            for (int k = 1; 2 * k <= n; k++)
            {
                descriptor[i++] = c[k].Magnitude / n;
                if (2 * k < n) descriptor[i++] = c[n - k].Magnitude / n;
            }
            scale = Scale(descriptor.Length > 0 ? descriptor[0] : 0, descriptor);
            if (normalize) Divide(descriptor, scale);
            return descriptor;
        }

        // The normalizing scale, or 0 when it is negligible next to the values it would divide (relative 1e-12).
        private static double Scale(double scale, double[] values)
        {
            double max = 0;
            foreach (double v in values) if (v > max) max = v;
            return scale > 1e-12 * max ? scale : 0;
        }

        private static void Divide(double[] values, double scale)
        {
            if (!(scale > 0)) return;
            for (int i = 0; i < values.Length; i++) values[i] /= scale;
        }

        /// <summary>Signed area of the closed polygon through the points (shoelace). Positive means counter-clockwise.</summary>
        public static double SignedArea(IReadOnlyList<Complex> z)
        {
            double twice = 0;
            for (int j = 0; j < z.Count; j++)
            {
                Complex a = z[j], b = z[(j + 1) % z.Count];
                twice += a.Real * b.Imaginary - b.Real * a.Imaginary;
            }
            return twice / 2;
        }
    }
}
