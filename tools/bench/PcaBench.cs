using System.Diagnostics;
using Accord.Statistics.Analysis;
using MIConvexHull;

// Managed-side cost of the per-object PCA pipeline for heavy point sets (e.g. mesh vertices).
// Run: dotnet run -c Release --project tools/bench -- pca
static class PcaBench
{
    // Synthetic "rock": noisy rotated ellipsoid surface, semi-axes 3 : 1.5 : 1.
    static double[] Rock(int n, int seed)
    {
        var rng = new Random(seed);
        var p = new double[3 * n];
        double c = Math.Cos(0.7), s = Math.Sin(0.7);
        for (int i = 0; i < n; i++)
        {
            double u = rng.NextDouble() * 2 - 1, t = rng.NextDouble() * 2 * Math.PI, r = Math.Sqrt(1 - u * u);
            double k = 1 + 0.05 * (rng.NextDouble() - 0.5);
            double x = 3 * r * Math.Cos(t) * k, y = 1.5 * r * Math.Sin(t) * k, z = u * k;
            p[3 * i] = c * x - s * y + 100; p[3 * i + 1] = s * x + c * y + 50; p[3 * i + 2] = z + 10;
        }
        return p;
    }

    // ---- current approach: double[n][] + Accord PCA (Method = Center) ----
    static double[][] Accord(double[] p)
    {
        int n = p.Length / 3;
        var pos = new double[n][];
        for (int i = 0; i < n; i++) pos[i] = new[] { p[3 * i], p[3 * i + 1], p[3 * i + 2] };
        var pca = new PrincipalComponentAnalysis { Method = PrincipalComponentMethod.Center };
        var t = pca.Learn(pos);
        var w = new double[3][];
        for (int j = 0; j < 3; j++) w[j] = new[] { t.Weights[0][j], t.Weights[1][j], t.Weights[2][j] };
        return w;
    }

    // ---- proposed: one pass over flat coordinates -> 3x3 covariance -> Jacobi eigensolve ----
    static void Accumulate(double[] p, int from, int to, double ox, double oy, double oz, double[] acc)
    {
        double sx = 0, sy = 0, sz = 0, xx = 0, yy = 0, zz = 0, xy = 0, xz = 0, yz = 0;
        for (int i = from; i < to; i++)
        {
            double x = p[3 * i] - ox, y = p[3 * i + 1] - oy, z = p[3 * i + 2] - oz; // shift for numerical stability
            sx += x; sy += y; sz += z; xx += x * x; yy += y * y; zz += z * z; xy += x * y; xz += x * z; yz += y * z;
        }
        lock (acc) { acc[0] += sx; acc[1] += sy; acc[2] += sz; acc[3] += xx; acc[4] += yy; acc[5] += zz; acc[6] += xy; acc[7] += xz; acc[8] += yz; }
    }

    public static (double[] values, double[][] vectors) Covariance(double[] p, bool parallel)
    {
        int n = p.Length / 3;
        var acc = new double[9];
        double ox = p[0], oy = p[1], oz = p[2];
        if (parallel && n > 50_000)
        {
            int chunks = Environment.ProcessorCount * 2, size = (n + chunks - 1) / chunks;
            Parallel.For(0, chunks, c => Accumulate(p, c * size, Math.Min(n, (c + 1) * size), ox, oy, oz, acc));
        }
        else Accumulate(p, 0, n, ox, oy, oz, acc);
        double mx = acc[0] / n, my = acc[1] / n, mz = acc[2] / n;
        var C = new double[3, 3];
        C[0, 0] = acc[3] / n - mx * mx; C[1, 1] = acc[4] / n - my * my; C[2, 2] = acc[5] / n - mz * mz;
        C[0, 1] = C[1, 0] = acc[6] / n - mx * my; C[0, 2] = C[2, 0] = acc[7] / n - mx * mz; C[1, 2] = C[2, 1] = acc[8] / n - my * mz;
        return Jacobi(C);
    }

    // Cyclic Jacobi for a symmetric 3x3; eigenpairs sorted by descending eigenvalue.
    public static (double[] values, double[][] vectors) Jacobi(double[,] A)
    {
        var a = (double[,])A.Clone();
        var V = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
        for (int sweep = 0; sweep < 50; sweep++)
        {
            double off = a[0, 1] * a[0, 1] + a[0, 2] * a[0, 2] + a[1, 2] * a[1, 2];
            if (off < 1e-30) break;
            foreach (var (pI, q) in new[] { (0, 1), (0, 2), (1, 2) })
            {
                if (Math.Abs(a[pI, q]) < 1e-300) continue;
                double theta = (a[q, q] - a[pI, pI]) / (2 * a[pI, q]);
                double t = Math.Sign(theta) / (Math.Abs(theta) + Math.Sqrt(theta * theta + 1));
                if (theta == 0) t = 1;
                double c = 1 / Math.Sqrt(t * t + 1), s = t * c;
                for (int k = 0; k < 3; k++) { double akp = a[k, pI], akq = a[k, q]; a[k, pI] = c * akp - s * akq; a[k, q] = s * akp + c * akq; }
                for (int k = 0; k < 3; k++) { double apk = a[pI, k], aqk = a[q, k]; a[pI, k] = c * apk - s * aqk; a[q, k] = s * apk + c * aqk; }
                for (int k = 0; k < 3; k++) { double vkp = V[k, pI], vkq = V[k, q]; V[k, pI] = c * vkp - s * vkq; V[k, q] = s * vkp + c * vkq; }
            }
        }
        var idx = new[] { 0, 1, 2 }.OrderByDescending(i => a[i, i]).ToArray();
        return (idx.Select(i => a[i, i]).ToArray(), idx.Select(i => new[] { V[0, i], V[1, i], V[2, i] }).ToArray());
    }

    static double T(Action a, int reps)
    {
        a();
        var sw = Stopwatch.StartNew();
        for (int r = 0; r < reps; r++) a();
        return sw.Elapsed.TotalMilliseconds / reps;
    }

    public static void Run()
    {
        Console.WriteLine("PCA per object (managed part only)");
        foreach (int n in new[] { 1_000, 10_000, 160_000, 1_000_000 })
        {
            var p = Rock(n, n);
            int reps = n >= 1_000_000 ? 2 : 5;
            double[][] wa = null; (double[] v, double[][] w) wb = default;
            double ta = T(() => wa = Accord(p), reps);
            double tb = T(() => wb = Covariance(p, false), reps);
            double tc = T(() => Covariance(p, true), reps);
            double agree = Enumerable.Range(0, 3).Min(k => Math.Abs(wa[k][0] * wb.w[k][0] + wa[k][1] * wb.w[k][1] + wa[k][2] * wb.w[k][2]));
            long alloc0 = GC.GetTotalAllocatedBytes(true); Accord(p); long allocA = GC.GetTotalAllocatedBytes(true) - alloc0;
            alloc0 = GC.GetTotalAllocatedBytes(true); Covariance(p, false); long allocB = GC.GetTotalAllocatedBytes(true) - alloc0;
            Console.WriteLine($"  n={n,9:N0}: Accord {ta,8:F2} ms ({allocA / 1e6,6:F1} MB) | one-pass {tb,7:F3} ms ({allocB / 1e3:F1} kB) {ta / tb,5:F0}x | one-pass parallel {tc,7:F3} ms {ta / tc,5:F0}x | min |cos| axes = {agree:F6}");
        }

        Console.WriteLine("3D convex hull per object (MIConvexHull 1.2, current: hull of ALL sample points)");
        foreach (int n in new[] { 1_000, 10_000, 160_000 })
        {
            var p = Rock(n, n + 1);
            var pts = Enumerable.Range(0, n).Select(i => new[] { p[3 * i], p[3 * i + 1], p[3 * i + 2] }).ToList();
            int faces = 0;
            double th = T(() => faces = ConvexHull.Create(pts).Result.Faces.Count(), 2);
            Console.WriteLine($"  n={n,9:N0}: {th,8:F1} ms ({faces} faces)");
        }

        Console.WriteLine("Square-ish slab orientation (in-plane eigen-gap): PCA1 angle vs true side for 1 x (1+e) x 0.1 boxes, corner-sampled + edge noise");
        var rng = new Random(3);
        foreach (double e in new[] { 0.0, 0.01, 0.05, 0.2 })
        {
            // sample the 12 edges of the box uniformly with slight jitter (mimics wireframe sampling)
            var pts = new List<double>();
            double L = 1 + e, W = 1, H = 0.1;
            var corners = new List<double[]>();
            foreach (var sx in new[] { -1, 1 }) foreach (var sy in new[] { -1, 1 }) foreach (var sz in new[] { -1, 1 }) corners.Add(new[] { sx * L / 2, sy * W / 2, sz * H / 2 });
            for (int i = 0; i < 8; i++) for (int j = i + 1; j < 8; j++)
            {
                int diff = 0; for (int k = 0; k < 3; k++) if (corners[i][k] != corners[j][k]) diff++;
                if (diff != 1) continue;
                for (int t = 0; t <= 10; t++) { double f = t / 10.0; for (int k = 0; k < 3; k++) pts.Add(corners[i][k] + f * (corners[j][k] - corners[i][k]) + 1e-4 * (rng.NextDouble() - 0.5)); }
            }
            var (vals, vecs) = Covariance(pts.ToArray(), false);
            double angle = Math.Atan2(Math.Abs(vecs[0][1]), Math.Abs(vecs[0][0])) * 180 / Math.PI;
            Console.WriteLine($"  e={e,4}: lambda1/lambda2 = {vals[0] / vals[1]:F4}, PCA1 is {angle,5:F1} deg off the long side");
        }
    }
}
