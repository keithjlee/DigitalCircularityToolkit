"""In-Rhino conformance check for the orientation modes (ALIGNMENT_SPEC §7). Writes JSON to $DCT_ALIGN_OUT.

    DCT_ALIGN_OUT=/tmp/align.json tools/rhino-profile/run.sh tools/rhino-profile/alignment_check.py [--no-build]

Each check records pass/fail and the numbers behind it; "ok" is true only if every check passes.
"""
import json
import math
import os
import random
import sys
import time
import traceback

import Rhino
Rhino.RhinoApp.GetPlugInObject("Grasshopper").LoadEditor()
import clr  # noqa: E402
for _a in ("DigitalCircularityToolkit.Numerics", "DigitalCircularityToolkit.Geometry"):
    clr.AddReference(_a)
import Rhino.Geometry as rg  # noqa: E402
from DigitalCircularityToolkit.Objects import AnalysisOptions, DesignObject, OrientationMode, ShapeAnalysis  # noqa: E402

sys.path.insert(0, os.path.join(os.environ.get("DCT_ROOT", os.getcwd()), "tests", "rhino"))
import make_fixtures as mf  # noqa: E402  (rock_mesh, rand_plane)

AUTO = AnalysisOptions(OrientationMode.Auto, 0.05, None, None)
results = {}


def check(name, fn):
    try:
        ok, info = fn()
        results[name] = {"ok": bool(ok), "info": info}
    except Exception:  # noqa: BLE001
        results[name] = {"ok": False, "info": traceback.format_exc()[-800:]}


def frame(g, opts=AUTO, n=50):
    return ShapeAnalysis.Analyze(g, n, opts).Frame


def deg(a, b):
    """Angle between two lines (sign ignored), degrees."""
    c = abs(a * b) / (a.Length * b.Length)
    return math.degrees(math.acos(min(1.0, c)))


def slab(plane, l, w, h):
    return rg.Box(plane, rg.Interval(-l / 2, l / 2), rg.Interval(-w / 2, w / 2), rg.Interval(-h / 2, h / 2))


def mesh_of(box, n=(10, 10, 2)):
    return rg.Mesh.CreateFromBox(box, n[0], n[1], n[2])


rng = random.Random(42)


def square_slabs():
    worst = 0.0
    modes = set()
    for k in range(20):
        pl = mf.rand_plane(rng, rg.Point3d(rng.uniform(-1e4, 1e4), rng.uniform(-1e4, 1e4), rng.uniform(-1e3, 1e3)))
        b = slab(pl, 1000, 1000, 50)
        for g in (b.ToBrep(), mesh_of(b)):
            f = frame(g)
            modes.add(str(f.Resolved))
            worst = max(worst, min(deg(f.PCA1, pl.XAxis), deg(f.PCA1, pl.YAxis)))
            worst = max(worst, deg(f.PCA3, pl.ZAxis))
    return worst < 0.1, {"worst_deg_off_side": worst, "resolved": sorted(modes)}


def near_square_stability():
    pl = mf.rand_plane(rng, rg.Point3d(100, 200, 300))
    b = slab(pl, 1000, 1010, 50)
    m = mesh_of(b, (20, 20, 2))
    base = None
    worst = 0.0
    for k in range(100):
        mm = m.DuplicateMesh()
        for i in range(mm.Vertices.Count):
            v = mm.Vertices[i]
            mm.Vertices.SetVertex(i, v.X + rng.gauss(0, 1e-4), v.Y + rng.gauss(0, 1e-4), v.Z + rng.gauss(0, 1e-4))
        f = frame(mm)
        if base is None:
            base = f.PCA1
        worst = max(worst, deg(f.PCA1, base))
    side = min(deg(base, pl.XAxis), deg(base, pl.YAxis))
    return worst < 0.1 and side < 0.1, {"max_change_deg": worst, "deg_off_side": side}


def box_breps():
    out = []
    ok = True
    for dims in ((3000, 200, 100), (3000, 150, 150), (500, 500, 500), (1200, 1200, 40)):
        pl = mf.rand_plane(rng, rg.Point3d(rng.uniform(-1e3, 1e3), 0, 0))
        b = slab(pl, *dims)
        f = frame(b.ToBrep())
        L, W, H = f.Length, f.Width, f.Height
        ax = [deg(f.PCA1, v) for v in (pl.XAxis, pl.YAxis, pl.ZAxis)]
        good = L >= W - 1e-6 and W >= H - 1e-6 and min(ax) < 1e-6 and abs(L - max(dims)) < 1e-6
        ok &= good
        out.append({"dims": dims, "LWH": [round(L, 6), round(W, 6), round(H, 6)], "resolved": str(f.Resolved), "pca1_vs_edges": [round(a, 6) for a in ax]})
    return ok, out


def cylinder_round_section():
    pl = mf.rand_plane(rng, rg.Point3d(0, 0, 0))
    cyl = rg.Cylinder(rg.Circle(pl, 100), 2000).ToBrep(True, True)
    f = frame(cyl)
    axis = deg(f.PCA1, pl.ZAxis)
    return axis < 0.5 and str(f.Resolved) in ("MinBoxYZ", "Symmetric"), {"axis_deg": axis, "resolved": str(f.Resolved), "gap23": f.Gap23}


def tessellation_independence():
    rock = mf.rock_mesh(random.Random(5), rg.Point3d(0, 0, 0), (1200, 700, 450), res=60)
    coarse = rock.DuplicateMesh()
    dense = rock.DuplicateMesh()
    # subdivide part of the dense mesh unevenly: split every triangle on one half of the rock several times
    for _ in range(2):
        faces = list(range(dense.Faces.Count))
        new = rg.Mesh()
        new.Vertices.AddVertices(dense.Vertices)
        for fi in faces:
            f = dense.Faces[fi]
            a, b, c = f.A, f.B, f.C
            pa, pb, pc = dense.Vertices[a], dense.Vertices[b], dense.Vertices[c]
            if pa.X > 0:
                m = new.Vertices.Add((pa.X + pb.X + pc.X) / 3, (pa.Y + pb.Y + pc.Y) / 3, (pa.Z + pb.Z + pc.Z) / 3)
                new.Faces.AddFace(a, b, m); new.Faces.AddFace(b, c, m); new.Faces.AddFace(c, a, m)
            else:
                new.Faces.AddFace(a, b, c)
        dense = new
    fa, fb = frame(coarse), frame(dense)
    la, lb = frame(coarse, AnalysisOptions.Legacy), frame(dense, AnalysisOptions.Legacy)
    d_new = max(deg(fa.PCA1, fb.PCA1), deg(fa.PCA3, fb.PCA3))
    d_old = max(deg(la.PCA1, lb.PCA1), deg(la.PCA3, lb.PCA3))
    return d_new < 0.5, {"auto_axis_change_deg": d_new, "legacy_axis_change_deg": d_old,
                         "vertices": [coarse.Vertices.Count, dense.Vertices.Count]}


def rotation_equivariance():
    worst = 0.0
    shapes = [mf.rock_mesh(random.Random(7), rg.Point3d(0, 0, 0), (1500, 800, 400), res=40),
              slab(rg.Plane.WorldXY, 2000, 600, 80).ToBrep(),
              rg.Polyline([rg.Point3d(0, 0, 0), rg.Point3d(3000, 0, 0), rg.Point3d(3000, 400, 0), rg.Point3d(800, 900, 0), rg.Point3d(0, 0, 0)]).ToPolylineCurve()]
    for g in shapes:
        f0 = frame(g)
        for _ in range(5):
            t = rg.Transform.PlaneToPlane(rg.Plane.WorldXY, mf.rand_plane(rng, rg.Point3d(rng.uniform(-5e3, 5e3), rng.uniform(-5e3, 5e3), 0)))
            g2 = g.Duplicate()
            g2.Transform(t)
            f1 = frame(g2)
            for v0, v1 in ((f0.PCA1, f1.PCA1), (f0.PCA2, f1.PCA2), (f0.PCA3, f1.PCA3)):
                v = rg.Vector3d(v0)
                v.Transform(t)
                worst = max(worst, deg(v, v1))
            worst = max(worst, abs(f0.Length - f1.Length) / f0.Length * 180)  # extents (scaled into the same budget)
    return worst < 1e-4, {"worst_deg": worst}


def override_plane():
    pl = mf.rand_plane(rng, rg.Point3d(1, 2, 3))
    g = mf.rock_mesh(random.Random(9), rg.Point3d(0, 0, 0), (900, 500, 300), res=30)
    f = frame(g, AnalysisOptions(OrientationMode.Override, 0.05, pl, None))
    err = max(deg(f.PCA1, pl.XAxis), deg(f.PCA2, pl.YAxis), deg(f.PCA3, pl.ZAxis))
    same_dir = f.PCA1 * pl.XAxis > 0 and f.PCA2 * pl.YAxis > 0
    return err < 1e-9 and same_dir, {"err_deg": err, "mode": str(f.Mode)}


def planar_outlines():
    # an L-shaped outline: region moments; frames must align with its legs' bounding box diagonal symmetry
    pts = [rg.Point3d(0, 0, 0), rg.Point3d(3000, 0, 0), rg.Point3d(3000, 500, 0), rg.Point3d(500, 500, 0),
           rg.Point3d(500, 1200, 0), rg.Point3d(0, 1200, 0), rg.Point3d(0, 0, 0)]
    crv = rg.Polyline(pts).ToPolylineCurve()
    rev = rg.Polyline(list(reversed(pts))).ToPolylineCurve()
    f, fr = frame(crv), frame(rev)
    d = max(deg(f.PCA1, fr.PCA1), deg(f.PCA3, fr.PCA3))
    centroid = (f.Centroid.X, f.Centroid.Y)
    # region centroid of the L: (3000*500*(1500,250) + 500*700*(250,850)) / (1.5e6 + 3.5e5)
    ex = ((1.5e6 * 1500 + 3.5e5 * 250) / 1.85e6, (1.5e6 * 250 + 3.5e5 * 850) / 1.85e6)
    cerr = math.hypot(centroid[0] - ex[0], centroid[1] - ex[1])
    return d < 1e-6 and cerr < 1e-6, {"direction_invariance_deg": d, "centroid_err": cerr, "normal_vs_Z_deg": deg(f.PCA3, rg.Vector3d.ZAxis)}


def timings():
    out = {}
    rock = mf.rock_mesh(random.Random(3), rg.Point3d(0, 0, 0), (1200, 700, 450), res=400)
    box = slab(mf.rand_plane(rng, rg.Point3d(0, 0, 0)), 2000, 300, 200).ToBrep()
    fil = rg.Brep.CreateFilletEdges(box, [e.EdgeIndex for e in box.Edges], [20.0] * box.Edges.Count, [20.0] * box.Edges.Count,
                                    rg.BlendType.Fillet, rg.RailType.RollingBall, 0.01)
    fil = fil[0] if fil else box
    for name, g in (("rock_mesh_%dv" % rock.Vertices.Count, rock), ("box_brep", box), ("filleted_brep", fil)):
        for label, opts in (("legacy", AnalysisOptions.Legacy), ("auto", AUTO)):
            DesignObject.Create(g, 50, "w", opts)
            t = time.perf_counter()
            for _ in range(5):
                DesignObject.Create(g, 50, "t", opts)
            out["%s %s ms" % (name, label)] = round((time.perf_counter() - t) / 5 * 1000, 2)
    ok = out.get("rock_mesh_%dv auto ms" % rock.Vertices.Count, 1e9) < 50
    return ok, out


for name, fn in [("square_slabs", square_slabs), ("near_square_stability", near_square_stability), ("box_breps", box_breps),
                 ("cylinder_round_section", cylinder_round_section), ("tessellation_independence", tessellation_independence),
                 ("rotation_equivariance", rotation_equivariance), ("override_plane", override_plane),
                 ("planar_outlines", planar_outlines), ("timings", timings)]:
    check(name, fn)

results_all = {"ok": all(r["ok"] for r in results.values()), "checks": results}
with open(os.environ["DCT_ALIGN_OUT"], "w") as fh:
    json.dump(results_all, fh, indent=1, default=str)
