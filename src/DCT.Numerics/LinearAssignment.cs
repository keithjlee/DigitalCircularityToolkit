using System;

namespace DigitalCircularityToolkit.Numerics
{
    /// <summary>
    /// Result of an assignment. <see cref="Assignment"/>[i] is the column (supply) given to row (demand) i, or −1.
    /// <see cref="Cost"/> sums the assigned entries only.
    /// </summary>
    public sealed record AssignmentResult(int[] Assignment, double Cost, int Unassigned, bool UsedMonotonePath, bool UsedAuction = false);

    /// <summary>
    /// Which general solver to use; both return an optimal assignment. Auto: the auction for matrices of at least
    /// <see cref="LinearAssignment.AuctionThreshold"/> entries (much faster on feature-space distances), shortest
    /// augmenting paths below that, or when the auction exceeds its bid budget (tied, wide-range integer matrices).
    /// </summary>
    public enum AssignmentMethod { Auto, Dense, Auction }

    /// <summary>
    /// Linear assignment on a <see cref="CostMatrix"/> (COST_MATRIX_SPEC §5), any shape:
    /// <list type="bullet">
    /// <item>rows ≤ cols: every row is assigned; rows &gt; cols: every column is used and the leftover rows get −1;</item>
    /// <item>with forbidden (+∞) entries: as many rows as possible are assigned, then the cost is minimized; rows that
    /// cannot be assigned get −1 (never an exception);</item>
    /// <item>matrices marked <see cref="CostMatrix.Monotone"/> (one feature, convex cost) are solved by sorting
    /// (square) or an O(n·m) dynamic program, with the same objective.</item>
    /// </list>
    /// General solvers: shortest augmenting paths (Crouse 2016, the algorithm of SciPy's <c>linear_sum_assignment</c>)
    /// for small matrices, and an ε-scaling auction with an exact polish (<see cref="AuctionAssignment"/>) for large
    /// ones, which is 5–20× faster on feature-space distance matrices. Both are exact and deterministic: the same matrix
    /// always gives the same assignment.
    /// </summary>
    public static class LinearAssignment
    {
        /// <summary>Matrices with at least this many entries use the auction under <see cref="AssignmentMethod.Auto"/> (the measured crossover is about 500×500).</summary>
        public const long AuctionThreshold = 250_000;

        /// <param name="cost">Rows = demand, columns = supply.</param>
        /// <param name="allowMonotonePath">False forces the general solver (for tests and comparisons).</param>
        /// <param name="method">General solver; both give an optimal assignment.</param>
        public static AssignmentResult Solve(CostMatrix cost, bool allowMonotonePath = true, AssignmentMethod method = AssignmentMethod.Auto)
        {
            if (allowMonotonePath && cost.Monotone != null) return SolveMonotone(cost.Monotone);

            int n = cost.Rows, m = cost.Cols;
            bool transpose = n > m;
            int nr = Math.Min(n, m), nc = Math.Max(n, m);
            ReadOnlySpan<double> values = cost.Values;

            double[]? transposed = null;
            if (transpose)
            {
                transposed = new double[nr * nc];
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < m; j++)
                        transposed[j * nc + i] = values[i * m + j];
            }
            ReadOnlySpan<double> a = transposed ?? values;

            bool forbidden = cost.ForbiddenCount > 0;
            bool auction = method == AssignmentMethod.Auction || (method == AssignmentMethod.Auto && (long)n * m >= AuctionThreshold);
            int[]? x = auction ? AuctionAssignment.Solve(transposed ?? cost.Array, nr, nc, forbidden, forbidden ? DummyCost(a, nr) : 0, out _) : null;
            auction = x != null;
            x ??= Augment(a, nr, nc, forbidden);

            var assignment = new int[n];
            Array.Fill(assignment, -1);
            for (int r = 0; r < nr; r++)
            {
                if (x[r] < 0) continue;
                if (transpose) assignment[x[r]] = r;
                else assignment[r] = x[r];
            }

            double total = 0;
            int unassigned = 0;
            for (int i = 0; i < n; i++)
            {
                if (assignment[i] < 0) unassigned++;
                else total += values[i * m + assignment[i]];
            }
            return new AssignmentResult(assignment, total, unassigned, false, auction);
        }

        /// <summary>
        /// Shortest augmenting paths for an nr×nc matrix, nr ≤ nc. Returns the column of each row, or −1.
        /// With forbidden entries, each row also gets a private "unassigned" column of cost D, larger than any change
        /// in finite cost one augmentation can make, so the number of assigned rows is maximized first. Every row then
        /// always has an augmenting path.
        /// </summary>
        private static int[] Augment(ReadOnlySpan<double> a, int nr, int nc, bool forbidden)
        {
            int total = forbidden ? nc + nr : nc;
            double dummyCost = forbidden ? DummyCost(a, nr) : double.PositiveInfinity;

            var u = new double[nr];
            var v = new double[total];
            var shortest = new double[total];
            var path = new int[total];
            var x = new int[nr];
            var y = new int[total];
            var sr = new bool[nr];
            var sc = new bool[total];
            var remaining = new int[total];
            Array.Fill(path, -1);
            Array.Fill(x, -1);
            Array.Fill(y, -1);

            for (int curRow = 0; curRow < nr; curRow++)
            {
                double minVal = 0;
                int i = curRow;
                int numRemaining = total;
                for (int jp = 0; jp < total; jp++)
                {
                    remaining[jp] = jp;
                    shortest[jp] = double.PositiveInfinity;
                }
                Array.Clear(sr);
                Array.Clear(sc);

                int sink = -1;
                while (sink == -1)
                {
                    sr[i] = true;
                    int indexLowest = -1;
                    double lowest = double.PositiveInfinity;
                    double ui = u[i];
                    int rowStart = i * nc;
                    for (int jk = 0; jk < numRemaining; jk++)
                    {
                        int j = remaining[jk];
                        double cij = j < nc ? a[rowStart + j] : (j - nc == i ? dummyCost : double.PositiveInfinity);
                        double r = minVal + cij - ui - v[j];
                        if (r < shortest[j])
                        {
                            path[j] = i;
                            shortest[j] = r;
                        }
                        if (shortest[j] < lowest || (shortest[j] == lowest && y[j] == -1))
                        {
                            lowest = shortest[j];
                            indexLowest = jk;
                        }
                    }

                    // Only reachable with a forbidden entry and no dummy columns, which cannot happen; kept as a guard.
                    if (indexLowest < 0 || double.IsPositiveInfinity(lowest))
                        throw new InvalidOperationException("No feasible assignment");

                    minVal = lowest;
                    int jSel = remaining[indexLowest];
                    if (y[jSel] == -1) sink = jSel;
                    else i = y[jSel];
                    sc[jSel] = true;
                    remaining[indexLowest] = remaining[--numRemaining];
                }

                u[curRow] += minVal;
                for (int ip = 0; ip < nr; ip++)
                    if (sr[ip] && ip != curRow)
                        u[ip] += minVal - shortest[x[ip]];
                for (int jp = 0; jp < total; jp++)
                    if (sc[jp])
                        v[jp] -= minVal - shortest[jp];

                int jAug = sink;
                while (true)
                {
                    int ip = path[jAug];
                    y[jAug] = ip;
                    (jAug, x[ip]) = (x[ip], jAug);
                    if (ip == curRow) break;
                }
            }

            for (int r = 0; r < nr; r++)
                if (x[r] >= nc) x[r] = -1;
            return x;
        }

        /// <summary>A cost larger than any finite total difference: (2·rows + 2)·max|c| + 1.</summary>
        private static double DummyCost(ReadOnlySpan<double> a, int nr)
        {
            double max = 0;
            foreach (double c in a)
                if (!double.IsPositiveInfinity(c) && Math.Abs(c) > max) max = Math.Abs(c);
            double d = (2.0 * nr + 2.0) * max + 1.0;
            return double.IsFinite(d) ? d : double.MaxValue / 4;
        }

        // ---------------------------------------------------------------- monotone (1-D convex) fast path

        /// <summary>
        /// For costs F(s − d) with convex F, an optimal matching never crosses: sorted demands are matched to sorted
        /// supplies in order. Square and all-finite: match in sorted order, O(n log n). Otherwise a dynamic program over
        /// (demands so far, supplies so far) that minimizes (unassigned rows, cost) lexicographically, O(n·m).
        /// </summary>
        public static AssignmentResult SolveMonotone(MonotoneCost1D problem)
        {
            double[] d = problem.Demand, s = problem.Supply;
            Func<double, double> f = problem.F;
            int n = d.Length, m = s.Length;
            int[] di = SortedIndices(d), sj = SortedIndices(s);
            var assignment = new int[n];
            Array.Fill(assignment, -1);

            bool done = false;
            if (n == m)
            {
                done = true;
                for (int k = 0; k < n && done; k++)
                    if (double.IsPositiveInfinity(f(s[sj[k]] - d[di[k]]))) done = false;
                if (done)
                    for (int k = 0; k < n; k++) assignment[di[k]] = sj[k];
            }

            if (!done)
            {
                // choice[i, j]: 0 = supply j unused, 1 = demand i unassigned, 2 = demand i takes supply j (1-based i, j).
                var choice = new byte[(n + 1) * (m + 1)];
                var prevU = new int[m + 1];
                var prevC = new double[m + 1];
                var curU = new int[m + 1];
                var curC = new double[m + 1];
                for (int i = 1; i <= n; i++)
                {
                    double di_ = d[di[i - 1]];
                    curU[0] = i;
                    curC[0] = 0;
                    choice[i * (m + 1)] = 1;
                    for (int j = 1; j <= m; j++)
                    {
                        int bu = curU[j - 1];
                        double bc = curC[j - 1];
                        byte ch = 0;
                        if (Better(prevU[j] + 1, prevC[j], bu, bc))
                        {
                            bu = prevU[j] + 1; bc = prevC[j]; ch = 1;
                        }
                        double fij = f(s[sj[j - 1]] - di_);
                        if (!double.IsPositiveInfinity(fij) && Better(prevU[j - 1], prevC[j - 1] + fij, bu, bc))
                        {
                            bu = prevU[j - 1]; bc = prevC[j - 1] + fij; ch = 2;
                        }
                        curU[j] = bu;
                        curC[j] = bc;
                        choice[i * (m + 1) + j] = ch;
                    }
                    (prevU, curU) = (curU, prevU);
                    (prevC, curC) = (curC, prevC);
                }

                for (int i = n, j = m; i > 0;)
                {
                    byte ch = choice[i * (m + 1) + j];
                    if (ch == 0) j--;
                    else if (ch == 1) i--;
                    else
                    {
                        assignment[di[i - 1]] = sj[j - 1];
                        i--;
                        j--;
                    }
                }
            }

            double total = 0;
            int unassigned = 0;
            for (int i = 0; i < n; i++)
            {
                if (assignment[i] < 0) unassigned++;
                else total += f(s[assignment[i]] - d[i]);
            }
            return new AssignmentResult(assignment, total, unassigned, true);
        }

        private static bool Better(int u1, double c1, int u2, double c2) => u1 < u2 || (u1 == u2 && c1 < c2);

        /// <summary>Indices ordered by value, ties by index (deterministic).</summary>
        private static int[] SortedIndices(double[] values)
        {
            var idx = new int[values.Length];
            for (int i = 0; i < idx.Length; i++) idx[i] = i;
            Array.Sort(idx, (p, q) => { int c = values[p].CompareTo(values[q]); return c != 0 ? c : p.CompareTo(q); });
            return idx;
        }
    }
}
