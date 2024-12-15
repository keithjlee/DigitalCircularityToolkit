using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalCircularityToolkit.Matching
{
    public class ShortestPathSolver
    {
        private void Fill<T>(T[] arr, T val){
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = val;
            }
        }
        public int[] Solve(double[,] cost)
        {
            var nr = cost.GetLength(0);
            var nc = cost.GetLength(1);

            // Initialize working arrays
            var u = new double[nr];
            var v = new double[nc];
            var shortestPathCosts = new double[nc];
            var path = new int[nc];
            var x = new int[nr];
            var y = new int[nc];
            var sr = new bool[nr];
            var sc = new bool[nc];
            this.Fill(path, -1);
            this.Fill(x, -1);
            this.Fill(y, -1);

            // Find a matching one vertex at a time
            for (var curRow = 0; curRow < nr; curRow++)
            {
                double minVal = 0;
                var i = curRow;
                // Reset working arrays
                var remaining = new int[nc].ToList();
                var numRemaining = nc;
                for (var jp = 0; jp < nc; jp++)
                {
                    remaining[jp] = jp;
                    shortestPathCosts[jp] = double.PositiveInfinity;
                }
                Array.Clear(sr, 0, sr.Length);
                Array.Clear(sc, 0, sc.Length);

                // Start finding augmenting path
                var sink = -1;
                while (sink == -1)
                {
                    sr[i] = true;
                    var indexLowest = -1;
                    var lowest = double.PositiveInfinity;
                    for (var jk = 0; jk < numRemaining; jk++)
                    {
                        var jl = remaining[jk];
                        // Note that this is the main bottleneck of this method; looking up the cost array
                        // is costly. Some obvious attempts to improve performance include swapping rows and
                        // columns, and disabling CLR bounds checking by using pointers to access the elements
                        // instead. We do not seem to get any significant improvements over the simpler
                        // approach below though.
                        var r = minVal + cost[i, jl] - u[i] - v[jl];
                        if (r < shortestPathCosts[jl])
                        {
                            path[jl] = i;
                            shortestPathCosts[jl] = r;
                        }

                        if (shortestPathCosts[jl] < lowest || shortestPathCosts[jl] == lowest && y[jl] == -1)
                        {
                            lowest = shortestPathCosts[jl];
                            indexLowest = jk;
                        }
                    }

                    minVal = lowest;
                    var jp = remaining[indexLowest];
                    if (double.IsPositiveInfinity(minVal))
                        throw new InvalidOperationException("No feasible solution.");
                    if (y[jp] == -1)
                        sink = jp;
                    else
                        i = y[jp];

                    sc[jp] = true;
                    remaining[indexLowest] = remaining[--numRemaining];
                    remaining.RemoveAt(numRemaining);
                }

                if (sink < 0)
                    throw new InvalidOperationException("No feasible solution.");

                // Update dual variables
                u[curRow] += minVal;
                for (var ip = 0; ip < nr; ip++)
                    if (sr[ip] && ip != curRow)
                        u[ip] += minVal - shortestPathCosts[x[ip]];

                for (var jp = 0; jp < nc; jp++)
                    if (sc[jp])
                        v[jp] -= minVal - shortestPathCosts[jp];

                // Augment previous solution
                var j = sink;
                while (true)
                {
                    var ip = path[j];
                    y[j] = ip;
                    (j, x[ip]) = (x[ip], j);
                    if (ip == curRow)
                        break;
                }
            }

            return x;
        }

        public int[] Solve(int[,] cost)
        {
            // Note that it would be possible to reimplement the above method using only
            // integer arithmetic. Doing so does provide a very slight performance improvement
            // but there's no nice way of implementing the method for ints and doubles at once
            // without duplicating code or moving to something like T4 templates. This would
            // work but would also increase the maintenance load, so for now we just keep this
            // simple and use the floating-point version directly.
            var nr = cost.GetLength(0);
            var nc = cost.GetLength(1);
            var doubleCost = new double[nr, nc];
            for (var i = 0; i < nr; i++)
            for (var j = 0; j < nc; j++)
            {
                doubleCost[i, j] = cost[i, j] == int.MaxValue ? double.PositiveInfinity : cost[i, j];
            }

            return Solve(doubleCost);
        }

        private int SolveForOneL(int l, int nc, double[] d, bool[] ok, int[] free,
            List<int> first, List<int> kk,
            List<double> cc, double[] v, int[] lab, int[] todo, int[] y, int[] x, int td1)
        {
            for (var jp = 0; jp < nc; jp++)
            {
                d[jp] = double.PositiveInfinity;
                ok[jp] = false;
            }

            var min = double.PositiveInfinity;
            var i0 = free[l];
            int j;
            for (var t = first[i0]; t < first[i0 + 1]; t++)
            {
                j = kk[t];
                var dj = cc[t] - v[j];
                d[j] = dj;
                lab[j] = i0;
                if (dj <= min)
                {
                    if (dj < min)
                    {
                        td1 = -1;
                        min = dj;
                    }

                    todo[++td1] = j;
                }
            }

            for (var hp = 0; hp <= td1; hp++)
            {
                j = todo[hp];
                if (y[j] == -1)
                {
                    UpdateAssignments(lab, y, x, j, i0);
                    return td1;
                }
                ok[j] = true;
            }

            var td2 = nc - 1;
            var last = nc;
            while (true)
            {
                if (td1 < 0)
                    throw new InvalidOperationException("No feasible solution.");
                var j0 = todo[td1--];
                var i = y[j0];
                todo[td2--] = j0;
                var tp = first[i];
                while (kk[tp] != j0) tp++;
                var h = cc[tp] - v[j0] - min;
                for (var t = first[i]; t < first[i + 1]; t++)
                {
                    j = kk[t];
                    if (!ok[j])
                    {
                        var vj = cc[t] - v[j] - h;
                        if (vj < d[j])
                        {
                            d[j] = vj;
                            lab[j] = i;
                            if (vj == min)
                            {
                                if (y[j] == -1)
                                {
                                    UpdateDual(nc, d, v, todo, last, min);
                                    UpdateAssignments(lab, y, x, j, i0);
                                    return td1;
                                }
                                todo[++td1] = j;
                                ok[j] = true;
                            }
                        }
                    }
                }

                if (td1 == -1)
                {
                    // The original Pascal code uses finite numbers instead of double.PositiveInfinity
                    // so we need to adjust slightly here.
                    min = double.PositiveInfinity;
                    last = td2 + 1;
                    for (var jp = 0; jp < nc; jp++)
                    {
                        if (!double.IsPositiveInfinity(d[jp]) && d[jp] <= min && !ok[jp])
                        {
                            if (d[jp] < min)
                            {
                                td1 = -1;
                                min = d[jp];
                            }

                            todo[++td1] = jp;
                        }
                    }

                    for (var hp = 0; hp <= td1; hp++)
                    {
                        j = todo[hp];
                        if (y[j] == -1)
                        {
                            UpdateDual(nc, d, v, todo, last, min);
                            UpdateAssignments(lab, y, x, j, i0);
                            return td1;
                        }
                        ok[j] = true;
                    }
                }
            }
        }

        private static void UpdateDual(int nc, double[] d, double[] v, int[] todo, int last, double min)
        {
            for (var k = last; k < nc; k++)
            {
                var j0 = todo[k];
                v[j0] += d[j0] - min;
            }
        }

        private static void UpdateAssignments(int[] lab, int[] y, int[] x, int j, int i0)
        {
            while (true)
            {
                var i = lab[j];
                y[j] = i;
                (j, x[i]) = (x[i], j);
                if (i == i0)
                    return;
            }
        }
    }
    
}