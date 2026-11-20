using System;

namespace DigitalCircularityToolkit.Orientation
{
    /// <summary>
    /// Eigen-decomposition of a symmetric 3×3 matrix by cyclic Jacobi rotations (Rhino-free, allocation-light).
    /// Replaces Accord's PCA (P1): the principal axes of a point set are the eigenvectors of its covariance.
    /// </summary>
    public static class SymmetricEigen3
    {
        /// <summary>
        /// Eigenvalues sorted descending, with unit eigenvectors as rows of <paramref name="vectors"/>
        /// (vectors[k] belongs to values[k]). Ties keep their Jacobi order. The input is not modified.
        /// </summary>
        public static void Decompose(double[,] matrix, out double[] values, out double[][] vectors)
        {
            var a = (double[,])matrix.Clone();
            var v = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

            double scale = 0;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    scale = Math.Max(scale, Math.Abs(a[i, j]));

            if (scale > 0)
            {
                for (int sweep = 0; sweep < 64; sweep++)
                {
                    double off = Math.Abs(a[0, 1]) + Math.Abs(a[0, 2]) + Math.Abs(a[1, 2]);
                    if (off <= 1e-15 * scale) break;
                    Rotate(a, v, 0, 1);
                    Rotate(a, v, 0, 2);
                    Rotate(a, v, 1, 2);
                }
            }

            // Stable descending order of the diagonal (ties keep index order).
            var order = new[] { 0, 1, 2 };
            for (int i = 1; i < 3; i++)
                for (int j = i; j > 0 && a[order[j], order[j]] > a[order[j - 1], order[j - 1]]; j--)
                {
                    int tmp = order[j]; order[j] = order[j - 1]; order[j - 1] = tmp;
                }
            values = new double[3];
            vectors = new double[3][];
            for (int k = 0; k < 3; k++)
            {
                int c = order[k];
                values[k] = a[c, c];
                vectors[k] = new[] { v[0, c], v[1, c], v[2, c] };
            }
        }

        // One Jacobi rotation zeroing a[p, q]; accumulates the rotation into the columns of v.
        private static void Rotate(double[,] a, double[,] v, int p, int q)
        {
            double apq = a[p, q];
            if (apq == 0) return;
            double theta = (a[q, q] - a[p, p]) / (2 * apq);
            double t = Math.Sign(theta) / (Math.Abs(theta) + Math.Sqrt(theta * theta + 1));
            if (theta == 0) t = 1;
            double c = 1 / Math.Sqrt(t * t + 1), s = t * c;

            for (int k = 0; k < 3; k++)
            {
                double akp = a[k, p], akq = a[k, q];
                a[k, p] = c * akp - s * akq;
                a[k, q] = s * akp + c * akq;
            }
            for (int k = 0; k < 3; k++)
            {
                double apk = a[p, k], aqk = a[q, k];
                a[p, k] = c * apk - s * aqk;
                a[q, k] = s * apk + c * aqk;
            }
            for (int k = 0; k < 3; k++)
            {
                double vkp = v[k, p], vkq = v[k, q];
                v[k, p] = c * vkp - s * vkq;
                v[k, q] = s * vkp + c * vkq;
            }
        }
    }
}
