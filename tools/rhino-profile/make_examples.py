#! python3
"""Build the documentation's example definitions inside Rhino, solve them, render their canvases, and save them.

    tools/rhino-profile/run.sh tools/rhino-profile/make_examples.py [--no-build]

Writes docs/src/assets/examples/<name>.gh (downloadable from the Workflows pages), <name>.png (canvas image) and
report.json (runtime warnings/errors per component; every example must solve cleanly). Geometry is synthetic and
seeded; units are millimetres. Uses the Canvas builder from tests/rhino/make_fixtures.py.
"""
import json
import math
import os
import random
import sys

import clr
clr.AddReference("Grasshopper")
clr.AddReference("System.Drawing")
import Rhino
import Rhino.Geometry as rg
import Grasshopper as gh
from System.Drawing import Color, PointF
from System.Drawing.Imaging import ImageFormat
from Grasshopper.GUI.Canvas import GH_Viewport
from Grasshopper.Kernel import GH_RuntimeMessageLevel, IGH_ActiveObject
from Grasshopper.Kernel import Parameters as P
from Grasshopper.Kernel import Types as T
from Grasshopper.Kernel.Special import GH_Group

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.environ.get("DCT_PROFILE_SCRIPT", __file__)), "..", ".."))
sys.path.insert(0, os.path.join(ROOT, "tests", "rhino"))
from make_fixtures import Canvas, box_from_dims, rock_mesh  # noqa: E402

OUT = os.path.join(ROOT, "docs", "src", "assets", "examples")
COL = 230  # horizontal spacing between pipeline columns


class Example(Canvas):
    def group(self, name, objs, colour):
        g = GH_Group()
        g.NickName = name
        g.Colour = colour
        for o in objs:
            g.AddObject(o.InstanceGuid)
        g.CreateAttributes()
        self.doc.AddObject(g, False)
        return g

    def list_item(self, nick, x, y, items, index):
        pick = self.comp("List Item", nick, x, y, category="Sets")
        pick.Params.Input[0].AddSource(items)
        pick.Params.Input[1].AddSource(index)
        return pick


BLUE = Color.FromArgb(60, 120, 170, 230)
GREEN = Color.FromArgb(60, 120, 210, 140)
ORANGE = Color.FromArgb(60, 240, 170, 90)
GREY = Color.FromArgb(50, 160, 160, 160)


# ----------------------------------------------------------------------------------------------- 1 linear
def warren_truss(span=12000.0, height=1500.0, panels=8):
    """Bottom chord, top chord and diagonals of a planar Warren truss in the XZ plane."""
    dx = span / panels
    bottom = [rg.Point3d(i * dx, 0, 0) for i in range(panels + 1)]
    top = [rg.Point3d((i + 0.5) * dx, 0, height) for i in range(panels)]
    lines = [rg.Line(bottom[i], bottom[i + 1]) for i in range(panels)]
    lines += [rg.Line(top[i], top[i + 1]) for i in range(panels - 1)]
    for i in range(panels):
        lines += [rg.Line(bottom[i], top[i]), rg.Line(top[i], bottom[i + 1])]
    return [T.GH_Curve(rg.LineCurve(l)) for l in lines]


def stock_pieces(rng, n, y0, lengths, section):
    """Reclaimed pieces laid side by side on the ground: (Brep goo, quantities, IDs)."""
    goo, qty, ids = [], [], []
    for i in range(n):
        l = rng.uniform(*lengths)
        w, h = section
        plane = rg.Plane(rg.Point3d(l / 2, y0 + i * 3 * w, h / 2), rg.Vector3d.XAxis, rg.Vector3d.YAxis)
        goo.append(T.GH_Brep(box_from_dims(plane, l, w, h).ToBrep()))
        qty.append(rng.choice([1, 1, 2, 3]))
        ids.append("T%02d" % (i + 1))
    return goo, qty, ids


def matching_tail(c, x, d_set, s_set, cost):
    """Match -> MatchLines, and List Item -> AlignToObject to place the matched supply on the demand."""
    m = c.comp("Match (DCT)", "Match", x, 0)
    c.wire((cost, "DistanceMatrix"), m, "DistanceMatrix")
    lines = c.comp("MatchLines (DCT)", "MatchLines", x + COL, -60)
    c.wire((d_set, "Centroids"), lines, "DemandPoints")
    c.wire((s_set, "Centroids"), lines, "SupplyPoints")
    c.wire((m, "Assignment"), lines, "AssignmentIndices")
    pick = c.list_item("MatchedSupply", x + COL, 100, c.out(s_set, "Objects"), c.out(m, "Assignment"))
    align = c.comp("AlignToObject (DCT)", "AlignToDemand", x + 2 * COL, 100)
    c.wire(pick.Params.Output[0], align, "SourceObject")
    c.wire((d_set, "Objects"), align, "TargetObject")
    c.group("4  Match and place", [m, lines, pick, align], ORANGE)
    return m


def ex_linear():
    rng = random.Random(11)
    c = Example()
    dem = c.data(P.Param_Curve, lambda g: g, warren_truss(), "TrussLines", 0, 0)
    sup_goo, qty, ids = stock_pieces(rng, 24, -6000, (1200, 4500), (90, 140))
    sup = c.data(P.Param_Brep, lambda g: g, sup_goo, "TimberStock", 0, 300)
    q = c.data(P.Param_Integer, T.GH_Integer, qty, "Quantity", 0, 360)
    i = c.data(P.Param_String, T.GH_String, ids, "StockID", 0, 420)

    d_obj = c.comp("LinearObject (DCT)", "Demand", COL, 0)
    c.wire(dem, d_obj, "Geometry")
    s_obj = c.comp("LinearObject (DCT)", "Supply", COL, 300)
    c.wire(sup, s_obj, "Geometry")
    c.wire(q, s_obj, "Quantity")
    c.wire(i, s_obj, "ID")
    d_set = c.comp("LinearSet (DCT)", "DemandSet", 2 * COL, 0)
    c.wire((d_obj, "LinearObject"), d_set, "Objects")
    s_set = c.comp("LinearSet (DCT)", "SupplySet", 2 * COL, 300)
    c.wire((s_obj, "LinearObject"), s_set, "Objects")
    c.group("1  Demand: truss members", [dem, d_obj, d_set], BLUE)
    c.group("2  Supply: reclaimed timber", [sup, q, i, s_obj, s_set], GREEN)

    d_fv = c.feature_vector("DemandFV", 3 * COL, 0, [(d_set, "EffectiveLength")])
    s_fv = c.feature_vector("SupplyFV", 3 * COL, 300, [(s_set, "EffectiveLength")])
    cost = c.comp("AsymmEuclideanDistance (DCT)", "LengthCost", 4 * COL, 0)
    c.wire((d_fv, "FeatureVector"), cost, "Demand")
    c.wire((s_fv, "FeatureVector"), cost, "Supply")
    c.group("3  Feature vectors and cost", [d_fv, s_fv, cost], GREY)
    matching_tail(c, 5 * COL, d_set, s_set, cost)
    return c


# ----------------------------------------------------------------------------------------------- 2 planar
def ex_planar():
    rng = random.Random(12)
    c = Example()
    panels, x = [], 0.0
    for col in range(5):
        w = rng.choice([900.0, 1200.0, 1500.0])
        for row in range(3):
            h = 1000.0
            rect = rg.Rectangle3d(rg.Plane(rg.Point3d(x, 0, row * h), rg.Vector3d.XAxis, rg.Vector3d.ZAxis), w, h)
            panels.append(T.GH_Curve(rect.ToNurbsCurve()))
        x += w
    dem = c.data(P.Param_Curve, lambda g: g, panels, "FacadePanels", 0, 0)
    plates, qty, ids = [], [], []
    for k in range(18):
        l, w, t = rng.uniform(900, 2200), rng.uniform(800, 1400), rng.choice([18.0, 25.0, 30.0])
        plane = rg.Plane(rg.Point3d(k % 6 * 2600 + l / 2, -4000 - k // 6 * 2000, t / 2), rg.Vector3d.XAxis, rg.Vector3d.YAxis)
        plates.append(T.GH_Brep(box_from_dims(plane, l, w, t).ToBrep()))
        qty.append(rng.choice([1, 2]))
        ids.append("P%02d" % (k + 1))
    sup = c.data(P.Param_Brep, lambda g: g, plates, "ReclaimedPlates", 0, 300)
    q = c.data(P.Param_Integer, T.GH_Integer, qty, "Quantity", 0, 360)
    i = c.data(P.Param_String, T.GH_String, ids, "PlateID", 0, 420)

    d_obj = c.comp("PlanarObject (DCT)", "Demand", COL, 0)
    c.wire(dem, d_obj, "Geometry")
    s_obj = c.comp("PlanarObject (DCT)", "Supply", COL, 300)
    c.wire(sup, s_obj, "Geometry")
    c.wire(q, s_obj, "Quantity")
    c.wire(i, s_obj, "ID")
    d_set = c.comp("PlanarSet (DCT)", "DemandSet", 2 * COL, 0)
    c.wire((d_obj, "PlanarObject"), d_set, "Objects")
    s_set = c.comp("PlanarSet (DCT)", "SupplySet", 2 * COL, 300)
    c.wire((s_obj, "PlanarObject"), s_set, "Objects")
    c.group("1  Demand: facade panels", [dem, d_obj, d_set], BLUE)
    c.group("2  Supply: reclaimed plates", [sup, q, i, s_obj, s_set], GREEN)

    feats = ["EffectiveLength", "EffectiveWidth"]
    d_fv = c.feature_vector("DemandFV", 3 * COL, 0, [(d_set, f) for f in feats])
    s_fv = c.feature_vector("SupplyFV", 3 * COL, 300, [(s_set, f) for f in feats])
    cost = c.comp("AsymmEuclideanDistance (DCT)", "SizeCost", 4 * COL, 0)
    c.wire((d_fv, "FeatureVector"), cost, "Demand")
    c.wire((s_fv, "FeatureVector"), cost, "Supply")
    c.group("3  Feature vectors and cost", [d_fv, s_fv, cost], GREY)
    matching_tail(c, 5 * COL, d_set, s_set, cost)
    return c


# ----------------------------------------------------------------------------------------------- 3 shapes
def convex_cell(rng, centre, r_lo, r_hi):
    n = rng.randint(5, 8)
    angles = sorted(rng.uniform(0, 2 * math.pi) for _ in range(n))
    pts = [centre + rg.Vector3d(math.cos(a) * rng.uniform(r_lo, r_hi), math.sin(a) * rng.uniform(r_lo, r_hi), 0)
           for a in angles]
    pts.append(pts[0])
    return rg.PolylineCurve(pts)


def ex_shapes():
    rng = random.Random(13)
    c = Example()
    cells = [T.GH_Curve(convex_cell(rng, rg.Point3d((k % 3) * 1400, (k // 3) * 1400, 0), 350, 600)) for k in range(9)]
    dem = c.data(P.Param_Curve, lambda g: g, cells, "TargetOutlines", 0, 0)
    stones = []
    for k in range(20):
        dims = (rng.uniform(380, 650), rng.uniform(330, 560), rng.uniform(60, 110))
        stones.append(T.GH_Mesh(flat_stone(rng, rg.Point3d(5000 + (k % 5) * 1500, (k // 5) * 1500, 0), dims)))
    sup = c.data(P.Param_Mesh, lambda g: g, stones, "Stones", 0, 300)

    d_obj = c.comp("PlanarObject (DCT)", "Demand", COL, 0)
    c.wire(dem, d_obj, "Geometry")
    d_set = c.comp("PlanarSet (DCT)", "DemandSet", 2 * COL, 0)
    c.wire((d_obj, "PlanarObject"), d_set, "Objects")
    d_sig = c.comp("RadialSignature (DCT)", "DemandSignature", 2 * COL, -270)
    c.wire(dem, d_sig, "Curve")
    c.value(d_sig, "NumSamples", 64)
    d_h = c.comp("HarmonicAnalysisComplex (DCT)", "DemandHarmonics", 3 * COL, -270)
    c.wire((d_sig, "ComplexSignature"), d_h, "Signature")
    c.value(d_h, "FVLength", 8)
    c.group("1  Demand: target outlines", [dem, d_obj, d_set, d_sig, d_h], BLUE)

    s_obj = c.comp("PlanarObject (DCT)", "Supply", COL, 300)
    c.wire(sup, s_obj, "Geometry")
    s_set = c.comp("PlanarSet (DCT)", "SupplySet", 2 * COL, 300)
    c.wire((s_obj, "PlanarObject"), s_set, "Objects")
    outline = c.comp("PlanarOutline (DCT)", "StoneOutline", 2 * COL, 600)
    c.wire((s_set, "Objects"), outline, "Object")
    s_sig = c.comp("RadialSignature (DCT)", "SupplySignature", 3 * COL, 600)
    c.wire((outline, "ProjectedCurve"), s_sig, "Curve")
    c.value(s_sig, "NumSamples", 64)
    s_h = c.comp("HarmonicAnalysisComplex (DCT)", "SupplyHarmonics", 4 * COL, 600)
    c.wire((s_sig, "ComplexSignature"), s_h, "Signature")
    c.value(s_h, "FVLength", 8)
    c.group("2  Supply: stone outlines", [sup, s_obj, s_set, outline, s_sig, s_h], GREEN)

    cost = c.comp("EuclideanDistance (DCT)", "ShapeCost", 4 * COL, 0)
    c.wire((d_h, "HarmonicFeatureVector"), cost, "Demand")
    c.wire((s_h, "HarmonicFeatureVector"), cost, "Supply")
    c.value(cost, "Power", 1)
    c.group("3  Cost", [cost], GREY)
    matching_tail(c, 5 * COL, d_set, s_set, cost)
    return c


def flat_stone(rng, centre, dims, res=48, bumps=0.12):
    """A flat stone lying on the ground: rock_mesh's bumpy ellipsoid, rotated only about Z."""
    m = rg.Mesh.CreateFromSphere(rg.Sphere(rg.Point3d.Origin, 1.0), res, res)
    k1, k2, ph = rng.randint(2, 4), rng.randint(2, 5), rng.uniform(0, 2 * math.pi)
    for i in range(m.Vertices.Count):
        v = m.Vertices[i]
        theta, phi = math.atan2(v.Y, v.X), math.acos(max(-1.0, min(1.0, v.Z)))
        r = 1.0 + bumps * math.sin(k1 * theta + ph) * math.cos(k2 * phi)
        m.Vertices.SetVertex(i, v.X * r * dims[0], v.Y * r * dims[1], v.Z * r * dims[2])
    m.Transform(rg.Transform.Rotation(rng.uniform(0, 2 * math.pi), rg.Vector3d.ZAxis, rg.Point3d.Origin))
    m.Translate(rg.Vector3d(centre) + rg.Vector3d(0, 0, dims[2]))
    m.Normals.ComputeNormals()
    m.Compact()
    return m


# ----------------------------------------------------------------------------------------------- 4 scans
def ex_scans():
    rng = random.Random(14)
    c = Example()
    path = rg.Curve.CreateInterpolatedCurve([rg.Point3d(0, 0, 0), rg.Point3d(2500, 1500, 0), rg.Point3d(5000, -500, 0),
                                             rg.Point3d(7500, 1200, 0)], 3)
    segs, t_len, total = [], 0.0, path.GetLength()
    while True:
        step = rng.uniform(280, 520)
        if t_len + step > total:
            break
        ok0, t0 = path.LengthParameter(t_len)
        ok1, t1 = path.LengthParameter(t_len + step)
        segs.append(T.GH_Curve(rg.LineCurve(path.PointAt(t0), path.PointAt(t1))))
        t_len += step + 40
    dem = c.data(P.Param_Curve, lambda g: g, segs, "BeadSegments", 0, 0)
    rocks = []
    for k in range(30):
        dims = (rng.uniform(140, 320), rng.uniform(90, 150), rng.uniform(70, 120))
        rocks.append(T.GH_Mesh(rock_mesh(rng, rg.Point3d((k % 6) * 900, -3000 - (k // 6) * 900, 0), dims, res=40, bumps=0.1)))
    sup = c.data(P.Param_Mesh, lambda g: g, rocks, "ScannedRocks", 0, 300)

    d_obj = c.comp("LinearObject (DCT)", "Demand", COL, 0)
    c.wire(dem, d_obj, "Geometry")
    d_set = c.comp("LinearSet (DCT)", "DemandSet", 2 * COL, 0)
    c.wire((d_obj, "LinearObject"), d_set, "Objects")
    c.group("1  Demand: segments along a path", [dem, d_obj, d_set], BLUE)

    s_obj = c.comp("LinearObject (DCT)", "Supply", COL, 300)
    c.wire(sup, s_obj, "Geometry")
    s_set = c.comp("LinearSet (DCT)", "SupplySet", 2 * COL, 300)
    c.wire((s_obj, "LinearObject"), s_set, "Objects")
    box = c.comp("BoxObject (DCT)", "RockBoxes", COL, 500)
    c.wire(sup, box, "Geometry")
    score = c.comp("BoxScore (DCT)", "RockBoxScore", 2 * COL, 500)
    c.wire((box, "BoxObject"), score, "BoxObject")
    knoll = c.comp("Knoll (DCT)", "InventoryLayout", 2 * COL, 620)
    c.wire((box, "BoxObject"), knoll, "Objects")
    c.value(knoll, "OriginPlane", rg.Plane(rg.Point3d(0, -9000, 0), rg.Vector3d.ZAxis))
    c.group("2  Supply: scanned rocks", [sup, s_obj, s_set, box, score, knoll], GREEN)

    d_fv = c.feature_vector("DemandFV", 3 * COL, 0, [(d_set, "EffectiveLength")])
    s_fv = c.feature_vector("SupplyFV", 3 * COL, 300, [(s_set, "EffectiveLength")])
    cost = c.comp("EuclideanDistance (DCT)", "LengthCost", 4 * COL, 0)
    c.wire((d_fv, "FeatureVector"), cost, "Demand")
    c.wire((s_fv, "FeatureVector"), cost, "Supply")
    c.value(cost, "Power", 1)
    c.group("3  Feature vectors and cost", [d_fv, s_fv, cost], GREY)
    matching_tail(c, 5 * COL, d_set, s_set, cost)
    return c


EXAMPLES = [("01_linear_truss", ex_linear), ("02_planar_panels", ex_planar),
            ("03_shape_tiling", ex_shapes), ("04_scanned_rocks", ex_scans)]


# ----------------------------------------------------------------------------------------------- solve / render
def solve_report(doc):
    doc.Enabled = True
    doc.NewSolution(True)
    rep = {}
    for obj in doc.Objects:
        if isinstance(obj, IGH_ActiveObject):
            msgs = []
            for lvl in ("Error", "Warning"):
                msgs += ["%s: %s" % (lvl, m) for m in obj.RuntimeMessages(getattr(GH_RuntimeMessageLevel, lvl))]
            if msgs:
                rep[obj.NickName] = msgs
    return rep


def render(doc, path, zoom=1.5, margin=30):
    canvas = gh.Instances.ActiveCanvas
    canvas.Document = doc
    for o in doc.Objects:
        o.Attributes.ExpireLayout()
        o.Attributes.PerformLayout()
    b = doc.BoundingBox(False)
    b.Inflate(margin, margin)
    vp = GH_Viewport()
    vp.Width = int(math.ceil(b.Width * zoom))
    vp.Height = int(math.ceil(b.Height * zoom))
    vp.Zoom = zoom

    def corner(tx, ty):
        vp.Tx, vp.Ty = tx, ty
        vp.ComputeProjection()
        return vp.ProjectPoint(PointF(b.Left, b.Top))

    # Solve Tx, Ty so the bounds' top-left corner lands on the image origin (projection is linear in Tx, Ty).
    p0, p1 = corner(0, 0), corner(100, 100)
    sx, sy = (p1.X - p0.X) / 100.0, (p1.Y - p0.Y) / 100.0
    corner(int(round(-p0.X / sx)), int(round(-p0.Y / sy)))
    bmp = canvas.GenerateHiResImageTile(vp, Color.White)
    bmp.Save(path, ImageFormat.Png)
    canvas.Document = None
    return [bmp.Width, bmp.Height]


def main():
    gh_if = Rhino.RhinoApp.GetPlugInObject("Grasshopper")
    gh_if.LoadEditor()
    os.makedirs(OUT, exist_ok=True)
    report = {}
    for name, build in EXAMPLES:
        c = build()
        msgs = solve_report(c.doc)
        size = render(c.doc, os.path.join(OUT, name + ".png"))
        c.save(os.path.join(OUT, name + ".gh"))
        report[name] = {"messages": msgs, "image": size, "objects": c.doc.ObjectCount}
        c.doc.Enabled = False
    with open(os.path.join(OUT, "report.json"), "w") as f:
        json.dump(report, f, indent=1)


main()
