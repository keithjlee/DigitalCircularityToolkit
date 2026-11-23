"""Optimality check of the bench's rectangular matrices with scipy: `dotnet run -c Release --project tools/bench -- rectcheck DIR && python3 tools/bench/rectcheck.py DIR`."""
import glob, os, sys
import numpy as np
from scipy.optimize import linear_sum_assignment


def cost(C, a):
    a = np.array(a)
    rows = np.nonzero(a >= 0)[0]
    cols = a[rows]
    assert len(set(cols.tolist())) == len(cols), "duplicate columns"
    return C[rows, cols].sum(), len(rows)


d = sys.argv[1]
for f in sorted(glob.glob(os.path.join(d, "m*.csv")), key=lambda p: int(os.path.basename(p)[1:-4])):
    C = np.loadtxt(f, delimiter=",", ndmin=2)
    r, c = linear_sum_assignment(C)
    best = C[r, c].sum()
    base = f[:-4]
    lap = [int(x) for x in open(base + "_lapjv.txt").read().split(",")]
    ours = [int(x) for x in open(base + "_ours.txt").read().split(",")]
    cl, nl = cost(C, lap)
    co, no = cost(C, ours)
    print("%-8s %4dx%-4d scipy %12.0f | LapjvCSharp %12.0f (%d rows) %s | LinearAssignment %12.0f (%d rows) %s" % (
        os.path.basename(base), C.shape[0], C.shape[1], best, cl, nl, "OPT" if cl == best else "SUBOPTIMAL",
        co, no, "OPT" if co == best else "SUBOPTIMAL"))
