using System;
using System.Collections.Generic;

namespace DigitalCircularityToolkit.Numerics
{
    /// <summary>
    /// Exact linear assignment for large dense matrices, fast on feature-space distance matrices where shortest
    /// augmenting paths are slow (long augmenting paths). Three stages:
    /// <list type="number">
    /// <item>Square the problem: rows = the nr real rows plus (N − nr) virtual rows of cost 0 to every column, where
    /// N is the number of columns (real, plus a private dummy column of cost D per row when there are forbidden
    /// entries). Virtual rows absorb the unused columns, so the square problem's optimum is the rectangular one's, and
    /// duals need no sign constraints.</item>
    /// <item>Auction with ε-scaling (Bertsekas) on costs rounded to integers and multiplied by N + 1: with ε = 1 the
    /// result is optimal for the integer problem. Real rows bid in turn (Gauss-Seidel); virtual rows take the cheapest free
    /// or real-held column from a price heap. Sequential and deterministic.</item>
    /// <item>Polish to the exact optimum of the double-precision costs: prices give column duals v; each row's dual is
    /// u_i = min_j (c_ij − v_j), which makes every reduced cost ≥ 0; rows whose assigned column is not that minimum are
    /// released and re-assigned by shortest augmenting paths. Only rows affected by the rounding move.</item>
    /// </list>
    /// </summary>
    internal static class AuctionAssignment
    {
        private const long Inf = long.MaxValue / 4;

        /// <summary>Real-row bids allowed per real row before giving up (geometric matrices need about 250; tied, wide-range
        /// integer matrices thousands, where shortest augmenting paths are faster).</summary>
        public const int BidBudgetPerRow = 600;


        /// <summary>
        /// Row → column for an nr×nc matrix (nr ≤ nc); −1 = unassigned (ended on its dummy). Null when the bid budget is
        /// exhausted: the caller then uses the dense solver (a count, not a timer, so the outcome is deterministic).
        /// </summary>
        public static int[]? Solve(double[] a, int nr, int nc, bool forbidden, double dummyCost, out int polished)
        {
            polished = 0;
            int n = forbidden ? nc + nr : nc;      // columns = rows of the square problem

            double maxAbs = forbidden ? Math.Abs(dummyCost) : 0;
            foreach (double c in a) if (!double.IsPositiveInfinity(c) && Math.Abs(c) > maxAbs) maxAbs = Math.Abs(c);
            if (maxAbs == 0) maxAbs = 1;
            long mult = n + 1L;
            // Integer costs up to ~2^40·(n+1)/(n+1) = 2^40 after multiplying: prices stay far below long.MaxValue.
            double scale = Math.Pow(2, 40) / (maxAbs * mult);

            long[] ci = new long[nr * nc];
            for (int k = 0; k < ci.Length; k++)
                ci[k] = double.IsPositiveInfinity(a[k]) ? Inf : (long)Math.Round(a[k] * scale) * mult;
            long dummyInt = forbidden ? (long)Math.Round(dummyCost * scale) * mult : Inf;
            long maxInt = (long)Math.Round(maxAbs * scale) * mult;

            var price = new long[n];
            var owner = new int[n];
            var col = new int[n];                 // column of each square row

            // Unassigned real rows bid in turn (Gauss-Seidel); rows evicted meanwhile bid in the next round.
            var bidders = new List<int>(nr);
            var nextBidders = new List<int>(nr);
            var bidCol = new int[nr];
            var bidPrice = new long[nr];
            var virtualQueue = new Queue<int>();
            var cheapest = new PriorityQueue<int, long>(n);

            // Column j goes to row r at the given price; its previous holder becomes unassigned.
            void Award(int j, int r, long newPrice)
            {
                price[j] = newPrice;
                cheapest.Enqueue(j, newPrice);
                int prev = owner[j];
                owner[j] = r;
                col[r] = j;
                if (prev >= 0)
                {
                    col[prev] = -1;
                    if (prev < nr) nextBidders.Add(prev);
                    else virtualQueue.Enqueue(prev);
                }
            }

            // Virtual rows (cost 0 everywhere) only ever want the cheapest column: a lazy min-heap of column prices
            // (entries are stale when the price has changed since) finds it in O(log n) instead of a scan.
            long eps = InitialEps(ci, nr, nc, maxInt);
            long bids = 0, budget = (long)BidBudgetPerRow * nr;
            while (true)
            {
                Array.Fill(owner, -1);
                Array.Fill(col, -1);
                cheapest.Clear();
                for (int j = 0; j < n; j++) cheapest.Enqueue(j, price[j]);
                bidders.Clear();
                for (int i = 0; i < nr; i++) bidders.Add(i);
                virtualQueue.Clear();
                for (int i = nr; i < n; i++) virtualQueue.Enqueue(i);

                while (bidders.Count > 0 || virtualQueue.Count > 0)
                {
                    if (bidders.Count > 0)
                    {
                        bids += bidders.Count;
                        if (bids > budget) return null;
                        // Gauss-Seidel: each bid is applied at once. (Parallel Jacobi rounds were tried: faster in a
                        // console benchmark, but slower and erratic inside Rhino, where each round waits for its slowest worker.)
                        foreach (int r in bidders)
                        {
                            RealBid(r, ci, nc, forbidden, dummyInt, price, maxInt, eps, bidCol, bidPrice);
                            Award(bidCol[r], r, bidPrice[r]);
                        }
                        (bidders, nextBidders) = (nextBidders, bidders);
                        nextBidders.Clear();
                    }

                    while (virtualQueue.Count > 0)
                    {
                        int i = virtualQueue.Dequeue();
                        int bj = PopValid(cheapest, price, owner, nr);   // always one: unassigned rows = unowned columns
                        long best = price[bj], second = Inf;
                        int j2 = PopValid(cheapest, price, owner, nr);
                        if (j2 >= 0)
                        {
                            second = price[j2];
                            cheapest.Enqueue(j2, second);
                        }
                        price[bj] += (second >= Inf ? maxInt + eps : second - best) + eps;
                        cheapest.Enqueue(bj, price[bj]);
                        int prev = owner[bj];
                        owner[bj] = i;
                        col[i] = bj;
                        if (prev >= 0)
                        {
                            col[prev] = -1;
                            bidders.Add(prev);          // virtual rows never evict virtual rows
                        }
                    }
                }

                if (eps == 1) break;
                eps = Math.Max(1, eps / 6);
            }

            // ---- Polish: exact duals for the double costs, release rows whose column is not their minimum.
            double toCost = 1.0 / (scale * mult);
            // Columns held by virtual rows get one common price, the lowest price of any held column, so every virtual
            // row is tight (it sits on a column of maximal v) and only rows that real rounding affects are released.
            long level = long.MaxValue;
            for (int j = 0; j < n; j++) if (owner[j] >= 0 && price[j] < level) level = price[j];
            var v = new double[n];
            for (int j = 0; j < n; j++) v[j] = -(owner[j] >= nr ? level : price[j]) * toCost;
            var u = new double[n];
            var x = new int[n];
            var y = new int[n];
            Array.Fill(y, -1);
            double maxV = double.NegativeInfinity;
            foreach (double vj in v) if (vj > maxV) maxV = vj;
            for (int i = 0; i < n; i++)
            {
                int j0 = col[i];
                double min = double.PositiveInfinity;
                if (i < nr)
                {
                    int start = i * nc;
                    for (int j = 0; j < nc; j++)
                    {
                        double c = a[start + j];
                        if (double.IsPositiveInfinity(c)) continue;
                        double r = c - v[j];
                        if (r < min) min = r;
                    }
                    if (forbidden) min = Math.Min(min, dummyCost - v[nc + i]);
                }
                else
                {
                    min = -maxV;
                }
                u[i] = min;
                if (Cost(a, nr, nc, forbidden, dummyCost, i, j0) - v[j0] == min)
                {
                    x[i] = j0;
                    y[j0] = i;
                }
                else
                {
                    x[i] = -1;
                    polished++;
                }
            }

            if (polished > 0) AugmentFree(a, nr, nc, n, forbidden, dummyCost, x, y, u, v);

            var result = new int[nr];
            for (int i = 0; i < nr; i++) result[i] = x[i] < nc ? x[i] : -1;
            return result;
        }

        /// <summary>
        /// First ε: an eighth of the largest row minimum (the scale of the costs an optimal assignment uses), not of the
        /// largest cost, which can be a penalty no optimal pair uses (e.g. squared shortfalls) and makes early phases
        /// inflate prices.
        /// </summary>
        private static long InitialEps(long[] ci, int nr, int nc, long maxInt)
        {
            long hi = 0;
            for (int i = 0; i < nr; i++)
            {
                long mn = Inf;
                int start = i * nc;
                for (int j = 0; j < nc; j++) if (ci[start + j] < mn) mn = ci[start + j];
                if (mn < Inf && Math.Abs(mn) > hi) hi = Math.Abs(mn);
            }
            return Math.Max(1, Math.Min(hi / 8, maxInt / 8));
        }

        /// <summary>Best column of real row i and its bid: the price that makes it as attractive as its second choice, plus ε.</summary>
        private static void RealBid(int i, long[] ci, int nc, bool forbidden, long dummyInt, long[] price, long maxInt, long eps, int[] bidCol, long[] bidPrice)
        {
            long best = Inf, second = Inf;
            int bj = -1;
            int start = i * nc;
            for (int j = 0; j < nc; j++)
            {
                long c = ci[start + j];
                if (c >= Inf) continue;
                long t = c + price[j];
                if (t < best) { second = best; best = t; bj = j; }
                else if (t < second) second = t;
            }
            if (forbidden)
            {
                long t = dummyInt + price[nc + i];
                if (t < best) { second = best; best = t; bj = nc + i; }
                else if (t < second) second = t;
            }
            bidCol[i] = bj;
            bidPrice[i] = price[bj] + (second >= Inf ? maxInt + eps : second - best) + eps;
        }

        /// <summary>
        /// The cheapest column a virtual row may take, removed from the heap; −1 if none. Stale entries (price changed
        /// since) are skipped. Columns held by another virtual row are skipped and dropped: virtual rows are identical, so
        /// evicting one only moves a price. Such a column re-enters the heap when a real row bids on it (new price) or at
        /// the next phase. This only affects speed: the polish makes the result exact.
        /// </summary>
        private static int PopValid(PriorityQueue<int, long> heap, long[] price, int[] owner, int nr)
        {
            while (heap.TryDequeue(out int j, out long p))
                if (p == price[j] && owner[j] < nr) return j;
            return -1;
        }

        private static double Cost(double[] a, int nr, int nc, bool forbidden, double dummyCost, int i, int j)
        {
            if (i >= nr) return 0;
            if (j < nc) return a[i * nc + j];
            return forbidden && j - nc == i ? dummyCost : double.PositiveInfinity;
        }

        /// <summary>Shortest augmenting paths (Crouse) for the unassigned rows of the square problem, from feasible duals.</summary>
        private static void AugmentFree(double[] a, int nr, int nc, int n, bool forbidden, double dummyCost,
            int[] x, int[] y, double[] u, double[] v)
        {
            var shortest = new double[n];
            var path = new int[n];
            var sr = new bool[n];
            var sc = new bool[n];
            var remaining = new int[n];

            for (int curRow = 0; curRow < n; curRow++)
            {
                if (x[curRow] != -1) continue;
                double minVal = 0;
                int i = curRow;
                int numRemaining = n;
                for (int jp = 0; jp < n; jp++)
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
                    for (int jk = 0; jk < numRemaining; jk++)
                    {
                        int j = remaining[jk];
                        double r = minVal + Cost(a, nr, nc, forbidden, dummyCost, i, j) - ui - v[j];
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
                for (int ip = 0; ip < n; ip++)
                    if (sr[ip] && ip != curRow)
                        u[ip] += minVal - shortest[x[ip]];
                for (int jp = 0; jp < n; jp++)
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
        }
    }
}
