#! python3
"""Open each fixture, solve it, and record every DCT component's outputs, runtime messages and solve time.

Env: DCT_FIXTURE_DIR (folder of .gh files), DCT_RECORD_DIR (output folder, one <fixture>.json each),
     DCT_FIXTURES (optional ';'-separated subset).
Output values are summarized to be comparable but small: numbers stay numbers; geometry becomes its type,
bounding box and counts; design objects become their type plus every public field; big lists become a digest.
tests/compare.py compares two recordings.
"""
import hashlib
import json
import math
import os
import time

import clr
clr.AddReference("Grasshopper")
import System
import Rhino.Geometry as rg
from GH_IO.Serialization import GH_Archive
from Grasshopper.Kernel import GH_Document, GH_RuntimeMessageLevel, IGH_Component

DCT_CATEGORY = "DigitalCircularityToolkit"
FULL_LIMIT = 4000          # outputs with more items than this are stored as a digest
SIG = 10                   # significant digits kept for floats
MAX_OUTPUT_BYTES = 150000  # outputs whose JSON is larger than this are also stored as a digest


def num(x):
    x = float(x)
    if x != x or x in (float("inf"), float("-inf")):
        return str(x)
    return float("%.*g" % (SIG, x))


def pt(p):
    return [num(p.X), num(p.Y), num(p.Z)]


def bbox(b):
    return [pt(b.Min), pt(b.Max)] if b.IsValid else None


def plane(p):
    return {"o": pt(p.Origin), "x": pt(p.XAxis), "y": pt(p.YAxis)}


SKIP_MEMBERS = {"Frame", "AreaEstimated"}
# Extra members to leave out, for comparing two builds across an intended change: "Member" or "Type.Member",
# comma-separated, e.g. DCT_RECORD_SKIP=TransformedGeometry,DesignObject.Quantity
EXTRA_SKIP = {m.strip() for m in os.environ.get("DCT_RECORD_SKIP", "").split(",") if m.strip()}


def skipped(type_name, member):
    return member in SKIP_MEMBERS or member in EXTRA_SKIP or (type_name + "." + member) in EXTRA_SKIP


def ser(v, depth=0):
    """Summarize a .NET or Python value as JSON-able data."""
    if v is None:
        return None
    if isinstance(v, bool) or isinstance(v, str):
        return v
    if isinstance(v, int):
        return int(v)
    if isinstance(v, float):
        return num(v)
    t = v.GetType() if hasattr(v, "GetType") else None
    tn = t.Name if t is not None else type(v).__name__
    if tn in ("Point3d", "Vector3d", "Point3f", "Vector3f"):
        return pt(v)
    if tn == "Plane":
        return plane(v)
    if tn == "Line":
        return [pt(v.From), pt(v.To)]
    if tn == "BoundingBox":
        return bbox(v)
    if tn == "Box":
        return {"plane": plane(v.Plane), "x": [num(v.X.T0), num(v.X.T1)], "y": [num(v.Y.T0), num(v.Y.T1)],
                "z": [num(v.Z.T0), num(v.Z.T1)]}
    if tn == "Sphere":
        return {"center": pt(v.Center), "r": num(v.Radius)}
    if tn == "Interval":
        return [num(v.T0), num(v.T1)]
    if tn == "Transform":
        return [num(v[i, j]) for i in range(4) for j in range(4)]
    if tn == "Complex":
        return [num(v.Real), num(v.Imaginary)]
    if tn == "Polyline":
        return {"polyline": v.Count, "length": num(v.Length), "bbox": bbox(v.BoundingBox)}
    if isinstance(v, rg.Mesh):
        return {"mesh": [v.Vertices.Count, v.Faces.Count], "bbox": bbox(v.GetBoundingBox(True))}
    if isinstance(v, rg.Brep):
        return {"brep": [v.Faces.Count, v.Edges.Count], "bbox": bbox(v.GetBoundingBox(True))}
    if isinstance(v, rg.Curve):
        return {"curve": tn, "length": num(v.GetLength()), "closed": bool(v.IsClosed),
                "bbox": bbox(v.GetBoundingBox(True))}
    if isinstance(v, rg.GeometryBase):
        return {"geometry": tn, "bbox": bbox(v.GetBoundingBox(True))}
    if tn == "CostMatrix":
        vals = [v[i, j] for i in range(v.Rows) for j in range(v.Cols)]
        finite = [x for x in vals if not math.isinf(x)]
        return {"cost_matrix": [v.Rows, v.Cols], "forbidden": v.ForbiddenCount, "monotone": v.Monotone is not None,
                "sum": num(sum(finite)) if finite else 0, "min": num(min(finite)) if finite else None,
                "max": num(max(finite)) if finite else None, "row0": [num(x) for x in vals[:min(v.Cols, 8)]]}
    if t is not None and t.IsArray or isinstance(v, System.Collections.IList):
        items = [ser(x, depth + 1) for x in v]
        return digest(items) if len(items) > 50 else items
    if t is not None and t.Namespace and t.Namespace.startswith("DigitalCircularityToolkit") and depth < 2:
        # P3 members that repackage data recorded under the original names (Frame = Centroid, PCA*, LocalPlane,
        # Localbox, Boundingbox; AreaEstimated = Area input <= 0). Skipped so recordings stay comparable.
        out = {"__type": tn}
        for f in t.GetFields():
            if not f.IsStatic and not skipped(tn, f.Name):
                out[f.Name] = ser(f.GetValue(v), depth + 1)
        for p in t.GetProperties():
            if p.CanRead and p.GetIndexParameters().Length == 0 and p.Name not in out and not skipped(tn, p.Name):
                try:
                    out[p.Name] = ser(p.GetValue(v), depth + 1)
                except Exception as e:  # noqa: BLE001
                    out[p.Name] = "<%s>" % type(e).__name__
        return out
    return "<%s>" % tn


def digest(items):
    flat = json.dumps(items, sort_keys=True)
    nums = [x for x in items if isinstance(x, (int, float)) and not isinstance(x, bool)]
    d = {"n": len(items), "sha1": hashlib.sha1(flat.encode()).hexdigest()[:16], "head": items[:5]}
    if nums and len(nums) == len(items):
        d.update({"sum": num(sum(nums)), "min": num(min(nums)), "max": num(max(nums))})
    return {"digest": d}


def goo_value(g):
    if g is None:
        return None
    v = g.ScriptVariable() if hasattr(g, "ScriptVariable") else g
    return ser(v)


def record_component(comp):
    rec = {"type": comp.Name, "ms": round(comp.ProcessorTime.TotalMilliseconds, 3), "messages": {}, "outputs": {}}
    for lvl in ("Error", "Warning", "Remark"):
        msgs = list(comp.RuntimeMessages(getattr(GH_RuntimeMessageLevel, lvl)))
        if msgs:
            rec["messages"][lvl] = sorted(set(str(m) for m in msgs))
    for p in comp.Params.Output:
        tree = p.VolatileData
        branches = []
        total = 0
        for path in tree.Paths:
            items = [goo_value(g) for g in tree.get_Branch(path)]
            total += len(items)
            branches.append([str(path), items])
        if total > FULL_LIMIT or len(json.dumps(branches)) > MAX_OUTPUT_BYTES:
            rec["outputs"][p.Name] = {"branches": len(branches),
                                      "items": digest([x for _, b in branches for x in b])}
        else:
            rec["outputs"][p.Name] = branches
    return rec


def record_fixture(path):
    # GH_Archive instead of GH_DocumentIO: no modal "IO generated N messages" dialog, and the messages are recorded
    # (e.g. "Input parameter chunk is missing" when a component's inputs no longer match an old file).
    archive = GH_Archive()
    if not archive.ReadFromFile(path):
        raise IOError("could not read " + path)
    doc = GH_Document()
    if not archive.ExtractObject(doc, "Definition"):
        raise IOError("could not extract the definition from " + path)
    io_msgs = sorted(set("%s: %s" % (m.Type, m.Message) for m in archive.Messages
                         if str(m.Type).lower() in ("warning", "error")))
    doc.Enabled = True
    t0 = time.perf_counter()
    doc.NewSolution(True)
    wall = time.perf_counter() - t0
    comps = {}
    for obj in doc.Objects:
        if isinstance(obj, IGH_Component) and obj.Category == DCT_CATEGORY:
            comps[obj.NickName] = record_component(obj)
    doc.Enabled = False
    doc.Dispose()
    rec = {"fixture": os.path.basename(path), "solve_wall_ms": round(wall * 1000, 1),
           "rhino": str(Rhino.RhinoApp.Version)}
    if io_msgs:
        rec["io_messages"] = io_msgs
    rec["components"] = dict(sorted(comps.items()))
    return rec


def main():
    src = os.environ["DCT_FIXTURE_DIR"]
    dst = os.environ["DCT_RECORD_DIR"]
    os.makedirs(dst, exist_ok=True)
    only = [s for s in os.environ.get("DCT_FIXTURES", "").split(";") if s]
    for name in sorted(os.listdir(src)):
        if not name.endswith(".gh") or (only and name[:-3] not in only):
            continue
        rec = record_fixture(os.path.join(src, name))
        with open(os.path.join(dst, name[:-3] + ".json"), "w") as f:
            json.dump(rec, f, indent=1, sort_keys=False)
            f.write("\n")


import Rhino  # noqa: E402  (used in record_fixture; imported late to keep the header readable)

if __name__ == "__main__":
    main()
