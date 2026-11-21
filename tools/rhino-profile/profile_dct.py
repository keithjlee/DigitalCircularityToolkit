#! python3
"""End-to-end timing of the *actual* DCT object constructors inside Rhino (no Grasshopper canvas).

Loads bin/Debug/net8.0/DigitalCircularityToolkit.gha (build first) and times DesignObject construction,
plus its main sub-steps, for box breps, filleted breps, freeform NURBS breps and meshes of several sizes.

    "/Applications/Rhino 8.app/Contents/MacOS/Rhinoceros" \
        -runscript='_-RunPythonScript "<repo>/tools/rhino-profile/profile_dct.py" _-Exit'
"""
import json
import math
import os
import random
import time

import clr
import Rhino
import Rhino.Geometry as rg

HERE = os.path.dirname(os.path.abspath(__file__))
BIN = os.path.normpath(os.path.join(HERE, "..", "..", "bin", "Debug", "net8.0"))
OUT = os.path.join(HERE, "profile_dct_results.json")

clr.AddReference("Grasshopper")
from System.Reflection import Assembly  # noqa: E402

# Assembly.LoadFrom (not clr.AddReference, which takes names, not paths); pythonnet picks up loaded assemblies.
for dll in ("MathNet.Numerics.dll", "Accord.dll", "Accord.Math.Core.dll", "Accord.Math.dll", "Accord.Statistics.dll",
            "MIConvexHull.dll", "DigitalCircularityToolkit.gha"):
    Assembly.LoadFrom(os.path.join(BIN, dll))

from System import Array  # noqa: E402
from System.Collections.Generic import List  # noqa: E402
from DigitalCircularityToolkit.Objects import DesignObject  # noqa: E402
from DigitalCircularityToolkit.Orientation import Discretizer, PCA  # noqa: E402
from DigitalCircularityToolkit.GeometryProcessing import Hulls  # noqa: E402

random.seed(11)
res = {"rhino": str(Rhino.RhinoApp.Version), "per_object_ms": {}, "notes": []}


def rand_plane():
    v = rg.Vector3d(random.uniform(-1, 1), random.uniform(-1, 1), random.uniform(-1, 1))
    v.Unitize()
    pl = rg.Plane(rg.Point3d(random.uniform(0, 100), random.uniform(0, 100), 0), v)
    pl.Rotate(random.uniform(0, math.pi), pl.ZAxis)
    return pl


def timed(label, fn, items):
    fn(items[0])  # warm-up (JIT)
    t0 = time.perf_counter()
    out = [fn(x) for x in items]
    res["per_object_ms"][label] = round((time.perf_counter() - t0) * 1000.0 / len(items), 3)
    return out


def make_box():
    L, W, H = random.uniform(2, 4), random.uniform(0.5, 1.5), random.uniform(0.1, 0.5)
    return rg.Brep.CreateFromBox(rg.Box(rand_plane(), rg.Interval(-L / 2, L / 2), rg.Interval(-W / 2, W / 2), rg.Interval(-H / 2, H / 2)))


def make_filleted():
    b = make_box()
    ids = List[int]([e.EdgeIndex for e in b.Edges])
    radii = List[float]([0.05] * len(ids))
    f = rg.Brep.CreateFilletEdges(b, ids, radii, radii, rg.BlendType.Fillet, rg.RailType.RollingBall, 0.001)
    return f[0] if f else b


def make_rock():
    ns = rg.Sphere(rg.Point3d.Origin, 1.0).ToNurbsSurface().Rebuild(3, 3, 12, 12)
    for u in range(ns.Points.CountU):
        for v in range(ns.Points.CountV):
            cp = ns.Points.GetControlPoint(u, v)
            k = 1 + random.uniform(-0.15, 0.15)
            ns.Points.SetPoint(u, v, rg.Point3d(cp.Location.X * 3 * k, cp.Location.Y * 1.5 * k, cp.Location.Z * k), cp.Weight)
    b = rg.Brep.CreateFromSurface(ns)
    b.Transform(rg.Transform.PlaneToPlane(rg.Plane.WorldXY, rand_plane()))
    return b


def make_mesh(res_uv):
    m = rg.Mesh.CreateFromSphere(rg.Sphere(rg.Point3d.Origin, 1.0), res_uv, res_uv)
    m.Transform(rg.Transform.Scale(rg.Plane.WorldXY, 3, 1.5, 1))
    m.Transform(rg.Transform.PlaneToPlane(rg.Plane.WorldXY, rand_plane()))
    return m


sets = {
    "box_brep": [make_box() for _ in range(40)],
    "filleted_box_brep": [make_filleted() for _ in range(20)],
    "rock_brep": [make_rock() for _ in range(10)],
}
res["notes"].append("filleted box faces: %d" % sets["filleted_box_brep"][0].Faces.Count)
meshes = {"mesh_10k": [make_mesh(100) for _ in range(10)], "mesh_160k": [make_mesh(400) for _ in range(4)]}

for name, breps in sets.items():
    timed(name + ".DesignObject(brep, 50) TOTAL", lambda b: DesignObject(b, 50), breps)
    pts = timed(name + ".  Discretizer.DiscretizeBrep", lambda b: Discretizer.DiscretizeBrep(b, 50), breps)
    res["notes"].append("%s sample count: %d" % (name, pts[0].Count))
    arrs = [Array[rg.Point3d](list(p)) for p in pts]
    timed(name + ".  PCA.PCAvectors (Accord)", lambda a: PCA.PCAvectors(Discretizer.PositionMatrix(a), True), arrs)
    timed(name + ".  VolumeMassProperties (centroid for transform)", lambda b: rg.VolumeMassProperties.Compute(b), breps)
    timed(name + ".  Hulls.MakeHull (3D)", lambda a: Hulls.MakeHull(a), arrs)
    timed(name + ".  Hulls.MakeHull2d", lambda a: Hulls.MakeHull2d(a, rg.Plane.WorldXY), arrs)

for name, ms in meshes.items():
    timed(name + ".DesignObject(mesh) TOTAL", lambda m: DesignObject(m, 0), ms)
    arrs = [m.Vertices.ToPoint3dArray() for m in ms]
    timed(name + ".  PCA.PCAvectors (Accord)", lambda a: PCA.PCAvectors(Discretizer.PositionMatrix(a), True), arrs)
    timed(name + ".  Hulls.MakeHull (3D)", lambda a: Hulls.MakeHull(a), arrs)
    timed(name + ".  Hulls.MakeHull2d", lambda a: Hulls.MakeHull2d(a, rg.Plane.WorldXY), arrs)

for k, v in res["per_object_ms"].items():
    print("%-60s %10.3f ms/object" % (k, v))
print("\n".join(res["notes"]))
with open(OUT, "w") as f:
    json.dump(res, f, indent=1)
