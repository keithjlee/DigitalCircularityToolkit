using System;
using System.Globalization;
using System.Threading.Tasks;

namespace DigitalCircularityToolkit.Numerics
{
    /// <summary>Fills one row of a cost matrix (<paramref name="values"/> has one entry per column).</summary>
    public delegate void RowFiller(int row, Span<double> values);

    /// <summary>
    /// Marks a cost matrix whose entries are exactly <c>F(Supply[j] − Demand[i])</c> for a convex F (one feature per
    /// object). Such problems have an optimal monotone matching (both sides sorted, no crossings), which
    /// <see cref="LinearAssignment"/> finds in O(n log n) or O(n·m) instead of running the general solver.
    /// F may return +∞ (forbidden).
    /// </summary>
    public sealed record MonotoneCost1D(double[] Demand, double[] Supply, Func<double, double> F);

    /// <summary>
    /// Dense cost matrix, row-major; rows = demand, columns = supply (COST_MATRIX_SPEC §3). Finite values are costs
    /// (any sign); +∞ marks a forbidden pair; NaN and −∞ are rejected. Immutable. Values built by a
    /// <see cref="RowFiller"/> are computed on first access, rows in parallel.
    /// </summary>
    public sealed class CostMatrix
    {
        private readonly Lazy<double[]> _values;
        private int _forbidden = -1;

        public int Rows { get; }
        public int Cols { get; }

        /// <summary>Set when the matrix is a convex function of a 1-D difference (the fast path of <see cref="LinearAssignment"/>).</summary>
        public MonotoneCost1D? Monotone { get; }

        /// <summary>A matrix from row-major values (copied and validated).</summary>
        public CostMatrix(int rows, int cols, double[] values)
        {
            CheckShape(rows, cols);
            if (values.Length != rows * cols)
                throw new ArgumentException(string.Format("Expected {0}×{1} = {2} values, got {3}", rows, cols, rows * cols, values.Length));
            var copy = (double[])values.Clone();
            Validate(copy, cols);
            Rows = rows;
            Cols = cols;
            _values = new Lazy<double[]>(() => copy);
        }

        private CostMatrix(int rows, int cols, Func<double[]> build, MonotoneCost1D? monotone)
        {
            Rows = rows;
            Cols = cols;
            Monotone = monotone;
            _values = new Lazy<double[]>(build);
        }

        /// <summary>
        /// A matrix whose rows are produced by <paramref name="fill"/> on first access (in parallel for large matrices).
        /// The filler must be a pure function of the row index. Throws on first access if it produces NaN or −∞.
        /// </summary>
        public static CostMatrix Build(int rows, int cols, RowFiller fill, MonotoneCost1D? monotone = null)
        {
            CheckShape(rows, cols);
            return new CostMatrix(rows, cols, () =>
            {
                var v = new double[rows * cols];
                if ((long)rows * cols < 20_000)
                {
                    for (int i = 0; i < rows; i++) fill(i, v.AsSpan(i * cols, cols));
                }
                else
                {
                    Parallel.For(0, rows, i => fill(i, v.AsSpan(i * cols, cols)));
                }
                Validate(v, cols);
                return v;
            }, monotone);
        }

        public double this[int i, int j] => _values.Value[i * Cols + j];

        public ReadOnlySpan<double> Row(int i) => _values.Value.AsSpan(i * Cols, Cols);

        /// <summary>The value array itself (row-major), for solvers that need an array. Never modify it.</summary>
        internal double[] Array => _values.Value;

        /// <summary>All values, row-major.</summary>
        public ReadOnlySpan<double> Values => _values.Value;

        /// <summary>Number of forbidden (+∞) entries.</summary>
        public int ForbiddenCount
        {
            get
            {
                if (_forbidden < 0)
                {
                    int n = 0;
                    foreach (double x in _values.Value) if (double.IsPositiveInfinity(x)) n++;
                    _forbidden = n;
                }
                return _forbidden;
            }
        }

        public CostMatrix Transpose()
        {
            var t = new double[Rows * Cols];
            ReadOnlySpan<double> v = Values;
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Cols; j++)
                    t[j * Rows + i] = v[i * Cols + j];
            return new CostMatrix(Cols, Rows, () => t, null);
        }

        /// <summary>e.g. <c>CostMatrix 120×340 (12% forbidden)</c>.</summary>
        public override string ToString()
        {
            int f = ForbiddenCount;
            string forbidden = f == 0 ? "" : string.Format(CultureInfo.InvariantCulture, " ({0:0.#}% forbidden)", 100.0 * f / ((double)Rows * Cols));
            return string.Format(CultureInfo.InvariantCulture, "CostMatrix {0}×{1}{2}", Rows, Cols, forbidden);
        }

        private static void CheckShape(int rows, int cols)
        {
            if (rows < 1 || cols < 1) throw new ArgumentException("A cost matrix needs at least one row and one column");
            if ((long)rows * cols > int.MaxValue) throw new ArgumentException(string.Format("A {0}×{1} cost matrix is too large", rows, cols));
        }

        private static void Validate(double[] v, int cols)
        {
            for (int k = 0; k < v.Length; k++)
            {
                double x = v[k];
                if (double.IsNaN(x) || double.IsNegativeInfinity(x))
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Cost [{0}, {1}] is {2}; costs must be finite or +∞ (forbidden)", k / cols, k % cols, x));
            }
        }
    }
}
