#! python3
"""Profile the native (RhinoCommon) steps of DCT's object-alignment pipeline, Brep vs Mesh.

Run inside Rhino 8+ (no Grasshopper needed):
    Rhino command:  _-RunPythonScript "<repo>/tools/rhino-profile/profile_alignment.py"
or headless-ish from a shell (Rhino must not already be running):
    "/Applications/Rhino 8.app/Contents/MacOS/Rhinoceros" \
        -runscript='_-RunPythonScript "<repo>/tools/rhino-profile/profile_alignment.py" _-Exit'

Results are printed and written as JSON next to this file (profile_results.json).
Geometry is generated in memory; nothing is added to the document.
"""
import json
import math
import os
import random
import time

import Rhino
import Rhino.Geometry as rg

random.seed(7)
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "profile_results.json")
results = {"rhino": str(Rhino.RhinoApp.Version), "steps": {}, "orientation": {}, "notes": []}


def timed(label, fn, items, reps=1):
    """Run fn over every item, return per-item mean ms; store under label."""
    out = None
    t0 = time.perf_counter()
    for _ in range(reps):
        out = [fn(x) for x in items]
    ms = (time.perf_counter() - t0) * 1000.0 / (reps * len(items))
    results["steps"][label] = round(ms, 3)
    return out


def rand_plane():
    v = rg.Vector3d(random.uniform(-1, 1), random.uniform(-1, 1), random.uniform(-1, 1))
    v.Unitize()
    pl = rg.Plane(rg.Point3d(random.uniform(0, 100), random.uniform(0, 100), 0), v)
    pl.Rotate(random.uniform(0, math.pi), pl.ZAxis)
    return pl


# ---------------- test geometry ----------------
boxes = []
for _ in range(40):
    pl = rand_plane()
    L, W, H = random.uniform(2, 4), random.uniform(0.5, 1.5), random.uniform(0.1, 0.5)
    boxes.append(rg.Brep.CreateFromBox(rg.Box(pl, rg.Interval(-L / 2, L / 2), rg.Interval(-W / 2, W / 2), rg.Interval(-H / 2, H / 2))))

rocks = []  # freeform closed NURBS breps
for _ in range(10):
    ns = rg.Sphere(rg.Point3d.Origin, 1.0).ToNurbsSurface()
    ns.IncreaseDegreeU(3)
    ns.IncreaseDegreeV(3)
    ns = ns.Rebuild(3, 3, 12, 12)
    for u in range(ns.Points.CountU):
        for v in range(ns.Points.CountV):
            cp = ns.Points.GetControlPoint(u, v)
            p = cp.Location
            k = 1 + random.uniform(-0.15, 0.15)
            ns.Points.SetPoint(u, v, rg.Point3d(p.X * 3 * k, p.Y * 1.5 * k, p.Z * k), cp.Weight)
    b = rg.Brep.CreateFromSurface(ns)
    b.Transform(rg.Transform.PlaneToPlane(rg.Plane.WorldXY, rand_plane()))
    rocks.append(b)

heavy = []  # heavy meshes ~160k vertices (scan-like)
for _ in range(4):
    m = rg.Mesh.CreateFromSphere(rg.Sphere(rg.Point3d.Origin, 1.0), 400, 400)
    m.Transform(rg.Transform.Scale(rg.Plane.WorldXY, 3, 1.5, 1))
    m.Transform(rg.Transform.PlaneToPlane(rg.Plane.WorldXY, rand_plane()))
    heavy.append(m)
results["notes"].append("heavy mesh vertex count: %d" % heavy[0].Vertices.Count)


# ---------------- current Brep pipeline (Discretizer.DiscretizeBrep + PCA.SolvePCA(Brep) + Populate) ----------------
def current_sampling(brep, n=50):
    wires = brep.GetWireframe(1)
    lengths = [w.GetLength() for w in wires]
    total = sum(lengths)
    pts = list(brep.DuplicateVertices())
    for w, ln in zip(wires, lengths):
        cnt = max(2, int(math.ceil(n / total * ln)))
        res = w.DivideByCount(cnt, False)
        if res is not None:
            pts.extend(w.PointAt(t) for t in res)
    return pts


def plane_box(geo, plane):
    try:
        bb, box = geo.GetBoundingBox(plane, rg.Box.Unset)
        return box
    except Exception:
        return geo.GetBoundingBox(plane)


def dup_transform(brep):
    d = brep.DuplicateBrep()
    d.Transform(rg.Transform.PlaneToPlane(rand_plane(), rg.Plane.WorldXY))
    return d


def principal_axes(mp):
    """Return (moments, axes) from a MassProperties object, trying the pythonnet out-param conventions."""
    try:
        r = mp.CentroidCoordinatesPrincipalMoments()
    except TypeError:
        r = mp.CentroidCoordinatesPrincipalMoments(0.0, rg.Vector3d.Unset, 0.0, rg.Vector3d.Unset, 0.0, rg.Vector3d.Unset)
    _, x, xa, y, ya, z, za = r
    return (x, y, z), (xa, ya, za)


fast_mp = rg.MeshingParameters.FastRenderMesh

for name, breps in (("box_brep", boxes), ("rock_brep", rocks)):
    plane = rg.Plane.WorldXY
    timed(name + ".current.sampling(wireframe+DivideByCount)", current_sampling, breps)
    timed(name + ".current.VolumeMassProperties(centroid only)", lambda b: rg.VolumeMassProperties.Compute(b), breps)
    timed(name + ".current.DuplicateBrep+Transform", dup_transform, breps)
    timed(name + ".current.GetBoundingBox(plane,out box)", lambda b: plane_box(b, rand_plane()), breps)
    # alternatives
    meshes = timed(name + ".alt.Mesh.CreateFromBrep(FastRenderMesh)", lambda b: rg.Mesh.CreateFromBrep(b, fast_mp), breps)
    timed(name + ".alt.AreaMassProperties(2nd moments)+principal axes",
          lambda b: principal_axes(rg.AreaMassProperties.Compute(b, False, True, True, True)), breps)
    timed(name + ".alt.VolumeMassProperties(2nd moments)+principal axes",
          lambda b: principal_axes(rg.VolumeMassProperties.Compute(b, False, True, True, True)), breps)
    if hasattr(breps[0], "IsBox"):
        timed(name + ".alt.Brep.IsBox()", lambda b: b.IsBox(), breps)
    joined = []
    for ms in meshes:
        j = rg.Mesh()
        for m in ms or []:
            j.Append(m)
        joined.append(j)
    results["notes"].append("%s FastRenderMesh vertex count (first): %d" % (name, joined[0].Vertices.Count))
    timed(name + ".alt.mesh.Vertices.ToPoint3dArray", lambda m: m.Vertices.ToPoint3dArray(), joined)
    timed(name + ".alt.mesh.GetBoundingBox(plane,out box)", lambda m: plane_box(m, rand_plane()), joined)

# ---------------- heavy mesh pipeline (PCA.SolvePCA(Mesh) + Populate) ----------------
timed("heavy_mesh.current.Vertices.ToPoint3dArray", lambda m: m.Vertices.ToPoint3dArray(), heavy)


def dup_mesh(m):
    d = m.DuplicateMesh()
    d.Transform(rg.Transform.PlaneToPlane(rand_plane(), rg.Plane.WorldXY))
    return d


timed("heavy_mesh.current.DuplicateMesh+Transform", dup_mesh, heavy)
timed("heavy_mesh.current.GetBoundingBox(plane,out box)", lambda m: plane_box(m, rand_plane()), heavy)
timed("heavy_mesh.alt.AreaMassProperties(2nd moments)+principal axes",
      lambda m: principal_axes(rg.AreaMassProperties.Compute(m, False, True, True, True)), heavy)
timed("heavy_mesh.alt.VolumeMassProperties(2nd moments)+principal axes",
      lambda m: principal_axes(rg.VolumeMassProperties.Compute(m, False, True, True, True)), heavy)


# ---------------- square-ish slab orientation ----------------
def pca_axes(pts):
    n = float(len(pts))
    mx = sum(p.X for p in pts) / n
    my = sum(p.Y for p in pts) / n
    mz = sum(p.Z for p in pts) / n
    c = [[0.0] * 3 for _ in range(3)]
    for p in pts:
        d = (p.X - mx, p.Y - my, p.Z - mz)
        for i in range(3):
            for j in range(3):
                c[i][j] += d[i] * d[j] / n
    # power iteration for the dominant axis is enough here
    v = [1.0, 0.3, 0.1]
    for _ in range(200):
        w = [sum(c[i][j] * v[j] for j in range(3)) for i in range(3)]
        nn = math.sqrt(sum(x * x for x in w))
        v = [x / nn for x in w]
    return rg.Vector3d(*v)


def deg_off_side(vec):
    a = math.degrees(math.atan2(abs(vec.Y), abs(vec.X)))
    return round(min(a, 90 - a), 1)  # 0 = aligned with a side, 45 = diagonal


for e in (0.0, 0.01, 0.05):
    b = rg.Brep.CreateFromBox(rg.Box(rg.Plane.WorldXY, rg.Interval(-(1 + e) / 2, (1 + e) / 2), rg.Interval(-0.5, 0.5), rg.Interval(-0.05, 0.05)))
    entry = {"sampled_pca": deg_off_side(pca_axes(current_sampling(b)))}
    try:
        _, axes = principal_axes(rg.AreaMassProperties.Compute(b, False, True, True, True))
        entry["area_mass_axes"] = [deg_off_side(a) for a in axes]
    except Exception as ex:  # noqa: BLE001
        entry["area_mass_axes"] = "error: %s" % ex
    results["orientation"]["1x%.2fx0.1 slab: deg off nearest side (0=side, 45=diagonal)" % (1 + e)] = entry

for k, v in sorted(results["steps"].items()):
    print("%-75s %10.3f ms/object" % (k, v))
print(json.dumps(results["orientation"], indent=1))
print("\n".join(results["notes"]))
with open(OUT, "w") as f:
    json.dump(results, f, indent=1)
print("wrote", OUT)
