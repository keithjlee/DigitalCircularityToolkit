using System;

namespace DigitalCircularityToolkit.Numerics
{
    /// <summary>Cost of serving one demand feature vector with one supply feature vector.</summary>
    public delegate double CellCost(ReadOnlySpan<double> demand, ReadOnlySpan<double> supply);

    /// <summary>
    /// The metrics of the cost-matrix components (COST_MATRIX_SPEC §4), in double precision with no rounding.
    /// A result that overflows is +∞ (forbidden), never a wrapped or saturated integer (B3).
    /// </summary>
    public static class CostFunctions
    {
        /// <summary>‖s − d‖₂^p.</summary>
        public static double Euclidean(ReadOnlySpan<double> d, ReadOnlySpan<double> s, double p)
        {
            double sum = 0;
            for (int k = 0; k < d.Length; k++)
            {
                double diff = s[k] - d[k];
                sum += diff * diff;
            }
            return p == 2 ? sum : p == 1 ? Math.Sqrt(sum) : Math.Pow(Math.Sqrt(sum), p);
        }

        /// <summary>
        /// Asymmetric Euclidean cost: ‖s − d‖₂^p when the supply is at least the demand in every feature (it can be cut
        /// down), otherwise ‖s − d‖₂^q, or +∞ (forbidden) when <paramref name="forbidShortfall"/> is set.
        /// </summary>
        public static double AsymmetricEuclidean(ReadOnlySpan<double> d, ReadOnlySpan<double> s, double p, double q, bool forbidShortfall)
        {
            double sum = 0;
            bool shortfall = false;
            for (int k = 0; k < d.Length; k++)
            {
                double diff = s[k] - d[k];
                if (diff < 0) shortfall = true;
                sum += diff * diff;
            }
            if (shortfall && forbidShortfall) return double.PositiveInfinity;
            double power = shortfall ? q : p;
            return power == 2 ? sum : power == 1 ? Math.Sqrt(sum) : Math.Pow(Math.Sqrt(sum), power);
        }

        /// <summary>Σ |s − d|.</summary>
        public static double Manhattan(ReadOnlySpan<double> d, ReadOnlySpan<double> s)
        {
            double sum = 0;
            for (int k = 0; k < d.Length; k++) sum += Math.Abs(s[k] - d[k]);
            return sum;
        }

        /// <summary>
        /// Null when the feature sets are usable: both non-empty, every vector the same length, all values finite.
        /// Otherwise an error message naming the first problem.
        /// </summary>
        public static string? CheckFeatures(double[][] demand, double[][] supply)
        {
            string? dims = Metrics.CheckDimensions(demand, supply);
            if (dims != null) return dims;
            string? bad = FirstNonFinite(demand, "Demand") ?? FirstNonFinite(supply, "Supply");
            return bad;
        }

        private static string? FirstNonFinite(double[][] set, string name)
        {
            for (int i = 0; i < set.Length; i++)
                for (int k = 0; k < set[i].Length; k++)
                    if (!double.IsFinite(set[i][k]))
                        return string.Format("{0} feature vector {1} has a non-finite value ({2}) at position {3}", name, i, set[i][k], k);
            return null;
        }

        /// <summary>The n×m matrix cost(demand[i], supply[j]). Validate the features first (<see cref="CheckFeatures"/>).</summary>
        public static CostMatrix Matrix(double[][] demand, double[][] supply, CellCost cost, MonotoneCost1D? monotone = null) =>
            CostMatrix.Build(demand.Length, supply.Length, (i, row) =>
            {
                double[] d = demand[i];
                for (int j = 0; j < row.Length; j++) row[j] = cost(d, supply[j]);
            }, monotone);

        /// <summary>Euclidean cost matrix. For one feature and p ≥ 1 it is marked for the monotone fast path.</summary>
        public static CostMatrix EuclideanMatrix(double[][] demand, double[][] supply, double p)
        {
            CellCost cost = (d, s) => Euclidean(d, s, p);
            return Matrix(demand, supply, cost, Monotone(demand, supply, cost, p >= 1));
        }

        /// <summary>Asymmetric Euclidean cost matrix. For one feature and p, q ≥ 1 (or a forbidden shortfall) it is marked for the monotone fast path.</summary>
        public static CostMatrix AsymmetricEuclideanMatrix(double[][] demand, double[][] supply, double p, double q, bool forbidShortfall)
        {
            CellCost cost = (d, s) => AsymmetricEuclidean(d, s, p, q, forbidShortfall);
            return Matrix(demand, supply, cost, Monotone(demand, supply, cost, p >= 1 && (forbidShortfall || q >= 1)));
        }

        /// <summary>Manhattan cost matrix (monotone fast path for one feature).</summary>
        public static CostMatrix ManhattanMatrix(double[][] demand, double[][] supply)
        {
            CellCost cost = (d, s) => Manhattan(d, s);
            return Matrix(demand, supply, cost, Monotone(demand, supply, cost, true));
        }

        /// <summary>
        /// The 1-D descriptor when there is one feature and the cost, as a function of x = s − d, is convex. F evaluates
        /// the same <paramref name="cost"/> on (0, x), which is bit-identical to cost(d, s) because x = s − d − 0.
        /// </summary>
        private static MonotoneCost1D? Monotone(double[][] demand, double[][] supply, CellCost cost, bool convex)
        {
            if (!convex || demand[0].Length != 1) return null;
            var d = new double[demand.Length];
            var s = new double[supply.Length];
            for (int i = 0; i < d.Length; i++) d[i] = demand[i][0];
            for (int j = 0; j < s.Length; j++) s[j] = supply[j][0];
            return new MonotoneCost1D(d, s, x =>
            {
                ReadOnlySpan<double> zero = stackalloc double[] { 0.0 };
                ReadOnlySpan<double> xs = stackalloc double[] { x };
                return cost(zero, xs);
            });
        }
    }
}
