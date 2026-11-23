#! python3
"""Generate the synthetic regression fixtures (Grasshopper definitions) inside Rhino.

Every fixture is built from a fixed seed, so regenerating gives the same geometry. The .gh files are NOT
committed (the heavy-mesh file is tens of MB); this script and the recorded outputs in tests/goldens are.
Units: millimetres (the integer cost matrices quantize to whole units, see PLAN B3).

Each DCT component gets a unique NickName; tests/rhino/record.py keys its recorded outputs by it.

Env: DCT_FIXTURE_DIR (output folder), DCT_FIXTURES (optional ';'-separated subset of fixture names),
     DCT_FIXTURES_FORCE=1 to overwrite existing files.
Requires Grasshopper to be loaded (GetPlugInObject("Grasshopper").LoadEditor()).
"""
import math
import os
import random

import clr
clr.AddReference("Grasshopper")
import System
from System.Drawing import PointF
import Rhino.Geometry as rg
import Grasshopper as gh
from Grasshopper.Kernel import GH_Convert, GH_Document, GH_DocumentIO, GH_ParameterSide
from Grasshopper.Kernel.Data import GH_Path
from Grasshopper.Kernel import Parameters as P
from Grasshopper.Kernel import Types as T

DCT_CATEGORY = "DigitalCircularityToolkit"


# ----------------------------------------------------------------------------------------------- canvas builder
class Canvas:
    def __init__(self):
        self.doc = GH_Document()
        self.nicks = set()

    def _place(self, obj, x, y):
        obj.CreateAttributes()
        obj.Attributes.Pivot = PointF(float(x), float(y))
        self.doc.AddObject(obj, False)
        return obj

    def comp(self, name, nick, x, y, category=DCT_CATEGORY):
        proxies = [p for p in gh.Instances.ComponentServer.ObjectProxies
                   if p.Desc.Name == name and p.Desc.Category == category]
        if len(proxies) > 1:
            proxies = [p for p in proxies if not p.Obsolete]
        if len(proxies) != 1:
            raise KeyError("component %r in %r: %d matches" % (name, category, len(proxies)))
        obj = proxies[0].CreateInstance()
        if category == DCT_CATEGORY:
            assert nick not in self.nicks, nick
            self.nicks.add(nick)
        obj.NickName = nick
        return self._place(obj, x, y)

    def data(self, param_type, goo_type, values, nick, x, y):
        """Floating parameter with internalized data. `values` is a list (one branch) or a list of lists."""
        p = param_type()
        p.NickName = nick
        p.Name = nick
        branches = values if values and isinstance(values[0], list) else [values]
        for i, branch in enumerate(branches):
            path = GH_Path(i)
            for v in branch:
                p.PersistentData.Append(goo_type(v), path)
        return self._place(p, x, y)

    @staticmethod
    def set_input(comp, index, goo):
        param = comp.Params.Input[index]
        param.PersistentData.Clear()
        values = goo if isinstance(goo, list) else [goo]
        for v in values:
            param.PersistentData.Append(v, GH_Path(0))

    @staticmethod
    def inp(comp, name):
        for p in comp.Params.Input:
            if p.Name == name:
                return p
        raise KeyError("%s has no input %r (has %s)" % (comp.NickName, name, [p.Name for p in comp.Params.Input]))

    @staticmethod
    def out(comp, name):
        for p in comp.Params.Output:
            if p.Name == name:
                return p
        raise KeyError("%s has no output %r (has %s)" % (comp.NickName, name, [p.Name for p in comp.Params.Output]))

    def wire(self, src, dst_comp, dst_input):
        """src: a floating param or (component, output name)."""
        if isinstance(src, tuple):
            src = self.out(*src)
        self.inp(dst_comp, dst_input).AddSource(src)

    def value(self, comp, input_name, v):
        param = self.inp(comp, input_name)
        param.PersistentData.Clear()
        for item in (v if isinstance(v, list) else [v]):
            param.PersistentData.Append(_goo(item), GH_Path(0))

    def feature_vector(self, nick, x, y, sources):
        """FeatureVector with one input per source (variable parameters)."""
        fv = self.comp("FeatureVector (DCT)", nick, x, y)
        while fv.Params.Input.Count < len(sources):
            i = fv.Params.Input.Count
            fv.Params.RegisterInputParam(fv.CreateParameter(GH_ParameterSide.Input, i), i)
        fv.VariableParameterMaintenance()
        fv.Params.OnParametersChanged()
        for i, s in enumerate(sources):
            fv.Params.Input[i].AddSource(self.out(*s) if isinstance(s, tuple) else s)
        return fv

    def save(self, path):
        if not GH_DocumentIO(self.doc).SaveQuiet(path):
            raise IOError("could not save " + path)


def _goo(v):
    if isinstance(v, bool):
        return T.GH_Boolean(v)
    if isinstance(v, int):
        return T.GH_Integer(v)
    if isinstance(v, float):
        return T.GH_Number(v)
    if isinstance(v, str):
        return T.GH_String(v)
    if isinstance(v, rg.Plane):
        return T.GH_Plane(v)
    raise TypeError(type(v))


# ----------------------------------------------------------------------------------------------- geometry helpers
def rand_unit(rng):
    while True:
        v = rg.Vector3d(rng.gauss(0, 1), rng.gauss(0, 1), rng.gauss(0, 1))
        if v.Length > 1e-6:
            v.Unitize()
            return v


def rand_plane(rng, origin):
    x = rand_unit(rng)
    y = rg.Vector3d.CrossProduct(x, rand_unit(rng))
    while y.Length < 1e-6:
        y = rg.Vector3d.CrossProduct(x, rand_unit(rng))
    y.Unitize()
    return rg.Plane(origin, x, y)


def grid_point(i, cols, spacing, y0=0.0, z=0.0):
    return rg.Point3d((i % cols) * spacing, y0 + (i // cols) * spacing, z)


def box_from_dims(plane, l, w, h):
    return rg.Box(plane, rg.Interval(-l / 2, l / 2), rg.Interval(-w / 2, w / 2), rg.Interval(-h / 2, h / 2))


def noisy_mesh_box(rng, box, counts=(12, 3, 3), noise=1.0):
    m = rg.Mesh.CreateFromBox(box, counts[0], counts[1], counts[2])
    for i in range(m.Vertices.Count):
        v = m.Vertices[i]
        m.Vertices.SetVertex(i, v.X + rng.uniform(-noise, noise), v.Y + rng.uniform(-noise, noise),
                             v.Z + rng.uniform(-noise, noise))
    m.Normals.ComputeNormals()
    return m


def rock_mesh(rng, center, dims, res=400, bumps=0.08, noise=0.01):
    """Scan-like closed mesh: a sphere with smooth bumps and jitter, scaled to `dims` and randomly rotated.
    res=400 gives about 160k vertices."""
    m = rg.Mesh.CreateFromSphere(rg.Sphere(rg.Point3d.Origin, 1.0), res, res)
    k1, k2, ph = rng.randint(2, 4), rng.randint(2, 5), rng.uniform(0, 2 * math.pi)
    verts = m.Vertices
    for i in range(verts.Count):
        v = verts[i]
        x, y, z = v.X, v.Y, v.Z
        theta = math.atan2(y, x)
        phi = math.acos(max(-1.0, min(1.0, z)))
        r = 1.0 + bumps * math.sin(k1 * theta + ph) * math.cos(k2 * phi) + rng.uniform(-noise, noise)
        verts.SetVertex(i, x * r * dims[0], y * r * dims[1], z * r * dims[2])
    plane = rand_plane(rng, center)
    m.Transform(rg.Transform.PlaneToPlane(rg.Plane.WorldXY, plane))
    m.Normals.ComputeNormals()
    m.Compact()
    return m


def timber_supply(rng, n, y0, mesh_every=2):
    """n reclaimed-timber pieces: alternate box Breps and noisy box meshes. Returns (geometry goo list, ids)."""
    goo, ids = [], []
    for i in range(n):
        l = rng.uniform(1500, 6000)
        w, h = rng.choice([(100, 200), (75, 150), (150, 300), (50, 100)])
        w *= rng.uniform(0.95, 1.05)
        h *= rng.uniform(0.95, 1.05)
        box = box_from_dims(rand_plane(rng, grid_point(i, 10, 7000, y0)), l, w, h)
        if i % mesh_every == 0:
            goo.append(T.GH_Brep(box.ToBrep()))
        else:
            goo.append(T.GH_Mesh(noisy_mesh_box(rng, box)))
        ids.append("S%03d" % i)
    return goo, ids


def demand_lines(rng, n, y0=0.0, lo=1000.0, hi=4000.0):
    goo = []
    for i in range(n):
        o = grid_point(i, 10, 5000, y0)
        a = rng.uniform(0, 2 * math.pi)
        l = rng.uniform(lo, hi)
        d = rg.Vector3d(math.cos(a) * l / 2, math.sin(a) * l / 2, 0)
        goo.append(T.GH_Curve(rg.LineCurve(o - d, o + d)))
    return goo


# ----------------------------------------------------------------------------------------------- fixtures
def linear_matching(name, seed, n_demand, n_supply, quantity_choices, with_align=True):
    """Demand lines vs reclaimed timber: LinearObject -> LinearSet -> FeatureVector -> distance -> Match."""
    rng = random.Random(seed)
    c = Canvas()
    dem = c.data(P.Param_Curve, lambda g: g, demand_lines(rng, n_demand), "DemandCurves", 0, 0)
    sup_goo, sup_ids = timber_supply(rng, n_supply, 60000)
    sup = c.data(P.Param_Geometry, lambda g: g, sup_goo, "SupplyGeometry", 0, 300)
    qty = c.data(P.Param_Integer, T.GH_Integer, [rng.choice(quantity_choices) for _ in range(n_supply)],
                 "SupplyQuantity", 0, 400)
    ids = c.data(P.Param_String, T.GH_String, sup_ids, "SupplyIDs", 0, 500)

    d_obj = c.comp("LinearObject (DCT)", "D_LinearObject", 250, 0)
    c.wire(dem, d_obj, "Geometry")
    s_obj = c.comp("LinearObject (DCT)", "S_LinearObject", 250, 300)
    c.wire(sup, s_obj, "Geometry")
    c.wire(qty, s_obj, "Quantity")
    c.wire(ids, s_obj, "ID")

    d_set = c.comp("LinearSet (DCT)", "D_LinearSet", 500, 0)
    c.wire((d_obj, "LinearObject"), d_set, "Objects")
    s_set = c.comp("LinearSet (DCT)", "S_LinearSet", 500, 300)
    c.wire((s_obj, "LinearObject"), s_set, "Objects")

    d_fv = c.feature_vector("D_FV", 750, 0, [(d_set, "EffectiveLength")])
    s_fv = c.feature_vector("S_FV", 750, 300, [(s_set, "EffectiveLength")])
    d_fv2 = c.feature_vector("D_FV2", 750, 100, [(d_set, "EffectiveLength"), (d_set, "Area")])
    s_fv2 = c.feature_vector("S_FV2", 750, 400, [(s_set, "EffectiveLength"), (s_set, "Area")])

    asym = c.comp("AsymmEuclideanDistance (DCT)", "AsymmDist", 1000, 0)
    c.wire((d_fv, "FeatureVector"), asym, "Demand")
    c.wire((s_fv, "FeatureVector"), asym, "Supply")
    c.value(asym, "Power1", 1)
    c.value(asym, "Power2", 2)
    euc = c.comp("EuclideanDistance (DCT)", "EuclidDist", 1000, 150)
    c.wire((d_fv2, "FeatureVector"), euc, "Demand")
    c.wire((s_fv2, "FeatureVector"), euc, "Supply")
    c.value(euc, "Power", 2)
    man = c.comp("ManhattanDistance (DCT)", "ManhattanDist", 1000, 300)
    c.wire((d_fv2, "FeatureVector"), man, "Demand")
    c.wire((s_fv2, "FeatureVector"), man, "Supply")

    m_a = c.comp("Match (DCT)", "Match_Asymm", 1250, 0)
    c.wire((asym, "DistanceMatrix"), m_a, "DistanceMatrix")
    m_e = c.comp("Match (DCT)", "Match_Euclid", 1250, 150)
    c.wire((euc, "DistanceMatrix"), m_e, "DistanceMatrix")

    lines = c.comp("MatchLines (DCT)", "MatchLines", 1500, 0)
    c.wire((d_set, "Centroids"), lines, "DemandPoints")
    c.wire((s_set, "Centroids"), lines, "SupplyPoints")
    c.wire((m_a, "Assignment"), lines, "AssignmentIndices")

    score = c.comp("LineScore (DCT)", "S_LineScore", 500, 600)
    c.wire((s_obj, "LinearObject"), score, "Object")
    props = c.comp("ObjectProperties (DCT)", "S_Props", 500, 750)
    c.wire((s_obj, "LinearObject"), props, "Object")

    if with_align:
        pick = c.comp("List Item", "PickSupply", 1500, 300, category="Sets")
        pick.Params.Input[0].AddSource(c.out(s_set, "Objects"))
        pick.Params.Input[1].AddSource(c.out(m_a, "Assignment"))
        align = c.comp("AlignToObject (DCT)", "AlignToDemand", 1750, 300)
        align.Params.Input[0].AddSource(pick.Params.Output[0])
        c.wire((d_set, "Objects"), align, "TargetObject")
    return c


def fx_linear_small():
    return linear_matching("linear_small", 1, 30, 80, [1, 1, 1, 2, 3])


def fx_linear_shortage():
    # 40 demand vs 25 supply instances: demand > supply exercises B1/B2/B12.
    return linear_matching("linear_shortage", 2, 40, 25, [1])


def fx_match_large():
    """1000 demand lines vs 1000 supply lines x quantity 3 = 3000 instances. Matching-scale timing."""
    rng = random.Random(7)
    c = Canvas()
    dem = c.data(P.Param_Curve, lambda g: g, demand_lines(rng, 1000), "DemandCurves", 0, 0)
    sup = c.data(P.Param_Curve, lambda g: g, demand_lines(rng, 1000, y0=600000, lo=1500, hi=6000),
                 "SupplyCurves", 0, 300)
    d_obj = c.comp("LinearObject (DCT)", "D_LinearObject", 250, 0)
    c.wire(dem, d_obj, "Geometry")
    s_obj = c.comp("LinearObject (DCT)", "S_LinearObject", 250, 300)
    c.wire(sup, s_obj, "Geometry")
    c.value(s_obj, "Quantity", 3)
    d_set = c.comp("LinearSet (DCT)", "D_LinearSet", 500, 0)
    c.wire((d_obj, "LinearObject"), d_set, "Objects")
    s_set = c.comp("LinearSet (DCT)", "S_LinearSet", 500, 300)
    c.wire((s_obj, "LinearObject"), s_set, "Objects")
    d_fv = c.feature_vector("D_FV", 750, 0, [(d_set, "EffectiveLength")])
    s_fv = c.feature_vector("S_FV", 750, 300, [(s_set, "EffectiveLength")])
    asym = c.comp("AsymmEuclideanDistance (DCT)", "AsymmDist", 1000, 0)
    c.wire((d_fv, "FeatureVector"), asym, "Demand")
    c.wire((s_fv, "FeatureVector"), asym, "Supply")
    c.value(asym, "Power1", 1)
    c.value(asym, "Power2", 2)
    m = c.comp("Match (DCT)", "Match_Asymm", 1250, 0)
    c.wire((asym, "DistanceMatrix"), m, "DistanceMatrix")
    lines = c.comp("MatchLines (DCT)", "MatchLines", 1500, 0)
    c.wire((d_set, "Centroids"), lines, "DemandPoints")
    c.wire((s_set, "Centroids"), lines, "SupplyPoints")
    c.wire((m, "Assignment"), lines, "AssignmentIndices")
    return c


def fx_brep_vs_mesh_boxes():
    """The same 60 boxes as Breps and as meshes, plus 20 cylinders (non-box Breps), through every constructor.
    Compares per-object cost of Brep vs mesh input (Keith: 'dozens of brep boxes slow')."""
    rng = random.Random(3)
    c = Canvas()
    breps, meshes = [], []
    for i in range(60):
        box = box_from_dims(rand_plane(rng, grid_point(i, 10, 8000)), rng.uniform(1500, 6000),
                            rng.uniform(80, 400), rng.uniform(50, 300))
        breps.append(T.GH_Brep(box.ToBrep()))
        meshes.append(T.GH_Mesh(rg.Mesh.CreateFromBox(box, 1, 1, 1)))
    cyl = []
    for i in range(20):
        p = rand_plane(rng, grid_point(i, 10, 8000, 100000))
        cyl.append(T.GH_Brep(rg.Cylinder(rg.Circle(p, rng.uniform(50, 200)), rng.uniform(1000, 5000)).ToBrep(True, True)))
    inputs = [("Brep", c.data(P.Param_Brep, lambda g: g, breps, "BoxBreps", 0, 0)),
              ("Mesh", c.data(P.Param_Mesh, lambda g: g, meshes, "BoxMeshes", 0, 600)),
              ("Cyl", c.data(P.Param_Brep, lambda g: g, cyl, "CylinderBreps", 0, 1200))]
    for row, (tag, src) in enumerate(inputs):
        y = row * 600
        o = c.comp("Object (DCT)", tag + "_Object", 250, y)
        c.wire(src, o, "Geometry")
        lo = c.comp("LinearObject (DCT)", tag + "_LinearObject", 250, y + 120)
        c.wire(src, lo, "Geometry")
        bo = c.comp("BoxObject (DCT)", tag + "_BoxObject", 250, y + 240)
        c.wire(src, bo, "Geometry")
        po = c.comp("PlanarObject (DCT)", tag + "_PlanarObject", 250, y + 360)
        c.wire(src, po, "Geometry")
        bset = c.comp("BoxSet (DCT)", tag + "_BoxSet", 500, y + 240)
        c.wire((bo, "BoxObject"), bset, "Objects")
        lset = c.comp("LinearSet (DCT)", tag + "_LinearSet", 500, y + 120)
        c.wire((lo, "LinearObject"), lset, "Objects")
        props = c.comp("ObjectProperties (DCT)", tag + "_Props", 500, y)
        c.wire((o, "Object"), props, "Object")
        bscore = c.comp("BoxScore (DCT)", tag + "_BoxScore", 750, y + 240)
        c.wire((bo, "BoxObject"), bscore, "BoxObject")
        outline = c.comp("PlanarOutline (DCT)", tag + "_Outline", 750, y)
        c.wire((o, "Object"), outline, "Object")
    return c


def fx_heavy_meshes():
    """8 scan-like rocks of about 160k vertices: the PCA/hull bottleneck (PLAN §5)."""
    rng = random.Random(4)
    c = Canvas()
    rocks = []
    for i in range(8):
        dims = (rng.uniform(600, 1200), rng.uniform(400, 800), rng.uniform(250, 500))
        rocks.append(T.GH_Mesh(rock_mesh(rng, grid_point(i, 4, 4000), dims)))
    src = c.data(P.Param_Mesh, lambda g: g, rocks, "Rocks", 0, 0)
    o = c.comp("Object (DCT)", "Rock_Object", 250, 0)
    c.wire(src, o, "Geometry")
    bo = c.comp("BoxObject (DCT)", "Rock_BoxObject", 250, 150)
    c.wire(src, bo, "Geometry")
    so = c.comp("SphericalObject (DCT)", "Rock_SphericalObject", 250, 300)
    c.wire(src, so, "Geometry")
    bset = c.comp("BoxSet (DCT)", "Rock_BoxSet", 500, 150)
    c.wire((bo, "BoxObject"), bset, "Objects")
    sset = c.comp("SphereSet (DCT)", "Rock_SphereSet", 500, 300)
    c.wire((so, "SphereObject"), sset, "Objects")
    sscore = c.comp("SphereScore (DCT)", "Rock_SphereScore", 750, 300)
    c.wire((so, "SphereObject"), sscore, "SphericalObject")
    props = c.comp("ObjectProperties (DCT)", "Rock_Props", 500, 0)
    c.wire((o, "Object"), props, "Object")
    return c


def fx_square_slabs():
    """Squares (1000x1000), near-squares (1000x1010) and rectangles (1000x600), 50 thick, randomly rotated in
    their own plane and in space; Breps and meshes. Shows the PCA-on-the-diagonal problem (ALIGNMENT_SPEC §4)."""
    rng = random.Random(5)
    c = Canvas()
    breps, meshes = [], []
    k = 0
    for (l, w) in [(1000.0, 1000.0), (1000.0, 1010.0), (1000.0, 600.0)]:
        for _ in range(8):
            box = box_from_dims(rand_plane(rng, grid_point(k, 8, 3000)), l, w, 50.0)
            breps.append(T.GH_Brep(box.ToBrep()))
            meshes.append(T.GH_Mesh(noisy_mesh_box(rng, box, counts=(10, 10, 1), noise=0.2)))
            k += 1
    for row, (tag, ptype, goo) in enumerate([("Brep", P.Param_Brep, breps), ("Mesh", P.Param_Mesh, meshes)]):
        y = row * 400
        src = c.data(ptype, lambda g: g, goo, tag + "Slabs", 0, y)
        po = c.comp("PlanarObject (DCT)", tag + "_PlanarObject", 250, y)
        c.wire(src, po, "Geometry")
        pset = c.comp("PlanarSet (DCT)", tag + "_PlanarSet", 500, y)
        c.wire((po, "PlanarObject"), pset, "Objects")
        ps = c.comp("PlaneScore (DCT)", tag + "_PlaneScore", 500, y + 150)
        c.wire((po, "PlanarObject"), ps, "Object")
        props = c.comp("ObjectProperties (DCT)", tag + "_Props", 750, y)
        c.wire((po, "PlanarObject"), props, "Object")
        hull = c.comp("PlanarHull (DCT)", tag + "_PlanarHull", 750, y + 150)
        c.wire((po, "PlanarObject"), hull, "Object")
    return c


def fx_modifiers():
    """Modifier chains. Inspection components read the input object AFTER the modifiers ran, so input mutation
    (B4) shows up as changed 'Before_*' outputs; output types show subtype loss (B5)."""
    rng = random.Random(6)
    c = Canvas()
    goo = []
    for i in range(10):
        box = box_from_dims(rand_plane(rng, grid_point(i, 5, 7000)), rng.uniform(2000, 5000), 150.0, 300.0)
        goo.append(T.GH_Brep(box.ToBrep()) if i % 2 == 0 else T.GH_Mesh(noisy_mesh_box(rng, box)))
    src = c.data(P.Param_Geometry, lambda g: g, goo, "Timber", 0, 0)
    planes = c.data(P.Param_Plane, T.GH_Plane,
                    [rg.Plane(grid_point(i, 5, 7000, 50000), rg.Vector3d(1, 0, 0), rg.Vector3d(0, 1, 0)) for i in range(10)],
                    "TargetPlanes", 0, 300)
    lo = c.comp("LinearObject (DCT)", "LinearObject", 250, 0)
    c.wire(src, lo, "Geometry")
    bo = c.comp("BoxObject (DCT)", "BoxObject", 250, 300)
    c.wire(src, bo, "Geometry")

    ov = c.comp("OverridePCA (DCT)", "OverridePCA", 500, 0)
    c.wire((lo, "LinearObject"), ov, "Object")
    c.wire(planes, ov, "PrincipalPlane")
    rot = c.comp("RotatePCA (DCT)", "RotatePCA", 500, 150)
    c.wire((lo, "LinearObject"), rot, "Object")
    c.value(rot, "RotationAxis", 0)
    c.value(rot, "RotationDegrees", 90.0)
    atp = c.comp("AlignToPlane (DCT)", "AlignToPlane_Box", 500, 300)
    c.wire((bo, "BoxObject"), atp, "Object")
    c.wire(planes, atp, "Plane")
    knoll = c.comp("Knoll (DCT)", "Knoll", 500, 450)
    c.wire((lo, "LinearObject"), knoll, "Objects")
    c.value(knoll, "OriginPlane", rg.Plane.WorldXY)
    c.value(knoll, "NumRows", 3)
    c.value(knoll, "RowSpacing", 500.0)
    c.value(knoll, "ColSpacing", 500.0)
    ato = c.comp("AlignToObject (DCT)", "AlignToObject", 500, 600)
    c.wire((bo, "BoxObject"), ato, "SourceObject")
    c.wire((ov, "Object"), ato, "TargetObject")

    # Downstream of each modifier: does the output keep its subtype? (LinearSet needs LinearObjects)
    for i, (comp, out) in enumerate([(ov, "Object"), (rot, "RotatedObject"), (knoll, "Objects")]):
        s = c.comp("LinearSet (DCT)", "After_%s_LinearSet" % comp.NickName, 750, i * 150)
        c.wire((comp, out), s, "Objects")
    bs = c.comp("BoxSet (DCT)", "After_AlignToPlane_BoxSet", 750, 450)
    c.wire((atp, "Object"), bs, "Objects")
    # Upstream objects inspected after the modifiers (B4: mutation by OverridePCA)
    before = c.comp("ObjectProperties (DCT)", "Before_Props", 1000, 0)
    c.wire((lo, "LinearObject"), before, "Object")
    after = c.comp("ObjectProperties (DCT)", "After_OverridePCA_Props", 1000, 150)
    c.wire((ov, "Object"), after, "Object")
    return c


def fx_characterization():
    """Every remaining component: signatures, harmonics, curve distances, normalization, spheres, Hungarian."""
    rng = random.Random(8)
    c = Canvas()
    outlines, curves_a, curves_b = [], [], []
    for i in range(12):
        o = grid_point(i, 6, 3000)
        n = rng.randint(5, 9)
        pts = [o + rg.Vector3d(math.cos(2 * math.pi * k / n) * rng.uniform(600, 1000),
                               math.sin(2 * math.pi * k / n) * rng.uniform(600, 1000), 0) for k in range(n)]
        pts.append(pts[0])
        outlines.append(T.GH_Curve(rg.PolylineCurve(pts)))
        a = [o + rg.Vector3d(k * 300.0, rng.uniform(-200, 200), 0) for k in range(8)]
        b = [o + rg.Vector3d(k * 300.0, rng.uniform(-200, 200), rng.uniform(0, 100)) for k in range(8)]
        curves_a.append(T.GH_Curve(rg.Curve.CreateInterpolatedCurve(a, 3)))
        curves_b.append(T.GH_Curve(rg.Curve.CreateInterpolatedCurve(b, 3)))
    out_p = c.data(P.Param_Curve, lambda g: g, outlines, "Outlines", 0, 0)
    ca = c.data(P.Param_Curve, lambda g: g, curves_a, "CurvesA", 0, 300)
    cb = c.data(P.Param_Curve, lambda g: g, curves_b, "CurvesB", 0, 400)

    rs = c.comp("RadialSignature (DCT)", "RadialSignature", 250, 0)
    c.wire(out_p, rs, "Curve")
    c.value(rs, "NumSamples", 64)
    hr = c.comp("HarmonicAnalysisReal (DCT)", "HarmonicsReal", 500, 0)
    c.wire((rs, "DistanceSignature"), hr, "Signature")
    c.value(hr, "FVLength", 8)
    hc = c.comp("HarmonicAnalysisComplex (DCT)", "HarmonicsComplex", 500, 150)
    c.wire((rs, "ComplexSignature"), hc, "Signature")
    c.value(hc, "FVLength", 8)
    # Same outlines reversed and mirrored: the harmonics must equal the ones above (direction/mirror invariance).
    flipped = []
    for g in outlines:
        crv = g.Value.DuplicateCurve()
        crv.Reverse()
        o = crv.PointAtStart
        crv.Transform(rg.Transform.Mirror(rg.Plane(o, rg.Vector3d.XAxis)))
        flipped.append(T.GH_Curve(crv))
    out_f = c.data(P.Param_Curve, lambda g: g, flipped, "Outlines_Flipped", 0, 150)
    rs_f = c.comp("RadialSignature (DCT)", "RadialSignature_Flipped", 250, 150)
    c.wire(out_f, rs_f, "Curve")
    c.value(rs_f, "NumSamples", 64)
    hr_f = c.comp("HarmonicAnalysisReal (DCT)", "HarmonicsReal_Flipped", 750, 0)
    c.wire((rs_f, "DistanceSignature"), hr_f, "Signature")
    c.value(hr_f, "FVLength", 8)
    hc_f = c.comp("HarmonicAnalysisComplex (DCT)", "HarmonicsComplex_Flipped", 750, 150)
    c.wire((rs_f, "ComplexSignature"), hc_f, "Signature")
    c.value(hc_f, "FVLength", 8)
    # Normalize: the flipped outlines scaled by 2.5 must give the same normalized harmonics as the originals.
    scaled = []
    for g in flipped:
        crv = g.Value.DuplicateCurve()
        crv.Transform(rg.Transform.Scale(crv.PointAtStart, 2.5))
        scaled.append(T.GH_Curve(crv))
    out_s = c.data(P.Param_Curve, lambda g: g, scaled, "Outlines_Scaled", 0, 1000)
    rs_s = c.comp("RadialSignature (DCT)", "RadialSignature_Scaled", 250, 1000)
    c.wire(out_s, rs_s, "Curve")
    c.value(rs_s, "NumSamples", 64)
    for sig_src, tag in ((rs, ""), (rs_s, "_Scaled")):
        hr_n = c.comp("HarmonicAnalysisReal (DCT)", "HarmonicsReal_Norm" + tag, 500, 1000 + (150 if tag else 0))
        c.wire((sig_src, "DistanceSignature"), hr_n, "Signature")
        c.value(hr_n, "FVLength", 8)
        c.value(hr_n, "Normalize", True)
        hc_n = c.comp("HarmonicAnalysisComplex (DCT)", "HarmonicsComplex_Norm" + tag, 750, 1000 + (150 if tag else 0))
        c.wire((sig_src, "ComplexSignature"), hc_n, "Signature")
        c.value(hc_n, "FVLength", 8)
        c.value(hc_n, "Normalize", True)

    hd = c.comp("HausdorffDistanceCurve (DCT)", "HausdorffCurve", 250, 300)
    c.wire(ca, hd, "First curve")
    c.wire(cb, hd, "Second curve")
    c.value(hd, "Sample count", 50)
    hd_rev = c.comp("HausdorffDistanceCurve (DCT)", "HausdorffCurve_Reversed", 250, 450)
    c.wire(cb, hd_rev, "First curve")
    c.wire(ca, hd_rev, "Second curve")
    c.value(hd_rev, "Sample count", 50)
    fr = c.comp("FrechetDistance (DCT)", "Frechet", 250, 600)
    c.wire(ca, fr, "First curve")
    c.wire(cb, fr, "Second curve")
    c.value(fr, "Sample count", 50)
    pa = c.data(P.Param_Point, T.GH_Point, [grid_point(i, 7, 100.0) for i in range(30)], "PointsA", 0, 700)
    pb = c.data(P.Param_Point, T.GH_Point,
                [rg.Point3d(rng.uniform(0, 700), rng.uniform(0, 500), rng.uniform(0, 50)) for _ in range(20)],
                "PointsB", 0, 800)
    hp = c.comp("HausdorffDistancePoints (DCT)", "HausdorffPoints", 250, 750)
    c.wire(pa, hp, "First set of points")
    c.wire(pb, hp, "Second set of points")

    vals_a = c.data(P.Param_Number, T.GH_Number, [rng.uniform(100, 5000) for _ in range(25)], "ValuesA", 0, 900)
    vals_b = c.data(P.Param_Number, T.GH_Number, [rng.uniform(100, 5000) for _ in range(40)], "ValuesB", 0, 1000)
    nz = c.comp("Normalize (DCT)", "Normalize", 250, 900)
    c.wire(vals_a, nz, "Values")
    c.value(nz, "Factor", 100.0)
    pn = c.comp("PairNormalize (DCT)", "PairNormalize", 250, 1050)
    c.wire(vals_a, pn, "A")
    c.wire(vals_b, pn, "B")
    c.value(pn, "Factor", 100.0)
    fa = c.feature_vector("FV_A", 500, 900, [(pn, "NormalizedA")])
    fb = c.feature_vector("FV_B", 500, 1050, [(pn, "NormalizedB")])
    euc = c.comp("EuclideanDistance (DCT)", "EuclidDist", 750, 900)
    c.wire((fa, "FeatureVector"), euc, "Demand")
    c.wire((fb, "FeatureVector"), euc, "Supply")
    c.value(euc, "Power", 1)
    hung = c.comp("Hungarian (DCT)", "Hungarian", 1000, 900)
    c.wire((euc, "DistanceMatrix"), hung, "DistanceMatrix")
    match = c.comp("Match (DCT)", "Match", 1000, 1050)
    c.wire((euc, "DistanceMatrix"), match, "DistanceMatrix")

    boulders = [T.GH_Mesh(rock_mesh(rng, grid_point(i, 5, 3000, 50000), (rng.uniform(300, 500),) * 3, res=40))
                for i in range(10)]
    bsrc = c.data(P.Param_Mesh, lambda g: g, boulders, "Boulders", 0, 1300)
    so = c.comp("SphericalObject (DCT)", "SphericalObject", 250, 1300)
    c.wire(bsrc, so, "Geometry")
    ss = c.comp("SphereSet (DCT)", "SphereSet", 500, 1300)
    c.wire((so, "SphereObject"), ss, "Objects")
    sc = c.comp("SphereScore (DCT)", "SphereScore", 500, 1450)
    c.wire((so, "SphereObject"), sc, "SphericalObject")
    return c


def fx_inputs():
    """Constructor input handling: SubD / Extrusion / Surface conversion, an unsupported type (Point) (B8),
    a user-supplied PlanarObject thickness (B7), and an invalid buffer."""
    rng = random.Random(9)
    c = Canvas()
    box = box_from_dims(rand_plane(rng, rg.Point3d(0, 0, 0)), 2000.0, 400.0, 60.0)
    subd = rg.SubD.CreateFromMesh(rg.Mesh.CreateFromBox(box, 2, 2, 1))
    rect = rg.Rectangle3d(rg.Plane(rg.Point3d(5000, 0, 0), rg.Vector3d(1, 0.2, 0), rg.Vector3d(-0.2, 1, 0)), 1500.0, 500.0)
    extrusion = rg.Extrusion.Create(rect.ToNurbsCurve(), 40.0, True)
    surface = rg.PlaneSurface(rg.Plane(rg.Point3d(10000, 0, 0), rg.Vector3d(0, 0, 1)), rg.Interval(0, 1200), rg.Interval(0, 800))
    items = [subd, extrusion, surface, rg.Point(rg.Point3d(1, 2, 3)), box.ToBrep()]
    src = c.data(P.Param_Geometry, GH_Convert.ToGeometricGoo, items, "MixedGeometry", 0, 0)
    o = c.comp("Object (DCT)", "Object", 250, 0)
    c.wire(src, o, "Geometry")
    po = c.comp("PlanarObject (DCT)", "PlanarObject_T30", 250, 150)
    c.wire(src, po, "Geometry")
    c.value(po, "Thickness", 30.0)
    pset = c.comp("PlanarSet (DCT)", "PlanarSet_T30", 500, 150)
    c.wire((po, "PlanarObject"), pset, "Objects")
    lo = c.comp("LinearObject (DCT)", "LinearObject_Buffer0", 250, 300)
    c.wire(src, lo, "Geometry")
    c.value(lo, "LengthBuffer", 0.0)
    return c


def fx_cost_matrix():
    """The CostMatrix pipeline (v0.6): every cost component into Assign, 1-D (monotone fast path) and 2-D (general
    solver), with demand > supply and forbidden shortfalls (-1 rows), plus CostMatrixToTree and MatchLines."""
    rng = random.Random(11)
    c = Canvas()
    dem = c.data(P.Param_Curve, lambda g: g, demand_lines(rng, 45), "DemandCurves", 0, 0)
    sup_goo, sup_ids = timber_supply(rng, 20, 60000)
    sup = c.data(P.Param_Geometry, lambda g: g, sup_goo, "SupplyGeometry", 0, 300)
    qty = c.data(P.Param_Integer, T.GH_Integer, [rng.choice([1, 2, 3]) for _ in range(20)], "SupplyQuantity", 0, 400)

    d_obj = c.comp("LinearObject (DCT)", "D_LinearObject", 250, 0)
    c.wire(dem, d_obj, "Geometry")
    s_obj = c.comp("LinearObject (DCT)", "S_LinearObject", 250, 300)
    c.wire(sup, s_obj, "Geometry")
    c.wire(qty, s_obj, "Quantity")
    d_set = c.comp("LinearSet (DCT)", "D_LinearSet", 500, 0)
    c.wire((d_obj, "LinearObject"), d_set, "Objects")
    s_set = c.comp("LinearSet (DCT)", "S_LinearSet", 500, 300)
    c.wire((s_obj, "LinearObject"), s_set, "Objects")

    d_fv = c.feature_vector("D_FV", 750, 0, [(d_set, "EffectiveLength")])
    s_fv = c.feature_vector("S_FV", 750, 300, [(s_set, "EffectiveLength")])
    d_fv2 = c.feature_vector("D_FV2", 750, 100, [(d_set, "EffectiveLength"), (d_set, "Area")])
    s_fv2 = c.feature_vector("S_FV2", 750, 400, [(s_set, "EffectiveLength"), (s_set, "Area")])

    def cost(kind, nick, y, d, s, **values):
        comp = c.comp(kind + " (DCT)", nick, 1000, y)
        c.wire((d, "FeatureVector"), comp, "Demand")
        c.wire((s, "FeatureVector"), comp, "Supply")
        for k, v in values.items():
            c.value(comp, k, v)
        a = c.comp("Assign (DCT)", "Assign_" + nick, 1250, y)
        c.wire((comp, "CostMatrix"), a, "CostMatrix")
        return comp, a

    asym1, a_asym1 = cost("AsymmEuclideanCost", "Asym1D", 0, d_fv, s_fv, Power1=1.0, Power2=2.0)
    cost("AsymmEuclideanCost", "AsymForbid1D", 150, d_fv, s_fv, Power1=1.0, ForbidShortfall=True)
    cost("AsymmEuclideanCost", "Asym2D", 300, d_fv2, s_fv2, Power1=1.0, Power2=2.0)
    cost("AsymmEuclideanCost", "AsymForbid2D", 450, d_fv2, s_fv2, Power1=1.5, ForbidShortfall=True)
    cost("EuclideanCost", "Euc2D", 600, d_fv2, s_fv2, Power=2.0)
    cost("EuclideanCost", "Euc1D", 750, d_fv, s_fv, Power=0.5)
    cost("ManhattanCost", "Man2D", 900, d_fv2, s_fv2)

    tree = c.comp("CostMatrixToTree (DCT)", "Asym1D_Tree", 1250, 1050)
    c.wire((asym1, "CostMatrix"), tree, "CostMatrix")
    lines = c.comp("MatchLines (DCT)", "MatchLines", 1500, 0)
    c.wire((d_set, "Centroids"), lines, "DemandPoints")
    c.wire((s_set, "Centroids"), lines, "SupplyPoints")
    c.wire((a_asym1, "Assignment"), lines, "AssignmentIndices")
    return c


def fx_orientation():
    """Orientation modes (v0.7): newly placed constructors default to Auto. Rotated square/near-square/rectangular
    slabs as box Breps and meshes, rocks in every mode, round and square members, closed planar outlines, and a
    Plane override."""
    rng = random.Random(23)
    c = Canvas()
    breps, meshes = [], []
    for k, (l, w) in enumerate([(1000, 1000), (1000, 1010), (1000, 600)] * 4):
        pl = rand_plane(rng, grid_point(k, 4, 3000))
        box = box_from_dims(pl, l, w, 50)
        breps.append(box.ToBrep())
        meshes.append(rg.Mesh.CreateFromBox(box, 8, 8, 2))
    b = c.data(P.Param_Brep, T.GH_Brep, breps, "SlabBreps", 0, 0)
    m = c.data(P.Param_Mesh, T.GH_Mesh, meshes, "SlabMeshes", 0, 150)
    ob = c.comp("Object (DCT)", "Slab_Brep_Object", 250, 0)
    c.wire(b, ob, "Geometry")
    om = c.comp("Object (DCT)", "Slab_Mesh_Object", 250, 150)
    c.wire(m, om, "Geometry")
    pb = c.comp("PlanarObject (DCT)", "Slab_PlanarObject", 500, 0)
    c.wire(b, pb, "Geometry")
    ps = c.comp("PlanarSet (DCT)", "Slab_PlanarSet", 750, 0)
    c.wire((pb, "PlanarObject"), ps, "Objects")
    bx = c.comp("BoxObject (DCT)", "Slab_Mesh_BoxObject", 500, 150)
    c.wire(m, bx, "Geometry")

    rocks = c.data(P.Param_Mesh, T.GH_Mesh,
                   [rock_mesh(rng, grid_point(k, 4, 4000, y0=20000), (1500, 900, 500), res=40) for k in range(6)], "Rocks", 0, 300)
    for mode, name in ((0, "Auto"), (1, "PCA"), (2, "MinBox"), (3, "Legacy")):
        o = c.comp("Object (DCT)", "Rock_Object_" + name, 250 + 250 * mode, 300)
        c.wire(rocks, o, "Geometry")
        c.value(o, "Orientation", mode)

    members = []
    for k in range(4):
        pl = rand_plane(rng, grid_point(k, 4, 3000, y0=40000))
        members.append(rg.Cylinder(rg.Circle(pl, 80), 3000).ToBrep(True, True))
        members.append(box_from_dims(pl, 120, 120, 3000).ToBrep())
    mem = c.data(P.Param_Brep, T.GH_Brep, members, "Members", 0, 450)
    lo = c.comp("LinearObject (DCT)", "Member_LinearObject", 250, 450)
    c.wire(mem, lo, "Geometry")
    ls = c.comp("LinearSet (DCT)", "Member_LinearSet", 500, 450)
    c.wire((lo, "LinearObject"), ls, "Objects")

    outlines = []
    for k in range(4):
        pl = rand_plane(rng, grid_point(k, 4, 4000, y0=60000))
        pts = [(0, 0), (3000, 0), (3000, 500), (500, 500), (500, 1200), (0, 1200), (0, 0)] if k % 2 == 0 else \
              [(0, 0), (1000, 0), (1000, 1000), (0, 1000), (0, 0)]
        outlines.append(rg.Polyline([pl.PointAt(x, y) for x, y in pts]).ToPolylineCurve())
    out = c.data(P.Param_Curve, T.GH_Curve, outlines, "Outlines", 0, 600)
    po = c.comp("PlanarObject (DCT)", "Outline_PlanarObject", 250, 600)
    c.wire(out, po, "Geometry")

    ov = c.comp("PlanarObject (DCT)", "Override_PlanarObject", 500, 600)
    c.wire(out, ov, "Geometry")
    c.value(ov, "Plane", rg.Plane(rg.Point3d(0, 0, 0), rg.Vector3d(1, 1, 0), rg.Vector3d(-1, 1, 0)))
    props = c.comp("ObjectProperties (DCT)", "Slab_Props", 750, 150)
    c.wire((om, "Object"), props, "Object")
    return c


FIXTURES = [
    ("linear_small", fx_linear_small),
    ("linear_shortage", fx_linear_shortage),
    ("match_large", fx_match_large),
    ("brep_vs_mesh_boxes", fx_brep_vs_mesh_boxes),
    ("heavy_meshes", fx_heavy_meshes),
    ("square_slabs", fx_square_slabs),
    ("modifiers", fx_modifiers),
    ("characterization", fx_characterization),
    ("inputs", fx_inputs),
    ("cost_matrix", fx_cost_matrix),
    ("orientation", fx_orientation),
]


def main():
    out_dir = os.environ["DCT_FIXTURE_DIR"]
    os.makedirs(out_dir, exist_ok=True)
    only = [s for s in os.environ.get("DCT_FIXTURES", "").split(";") if s]
    force = os.environ.get("DCT_FIXTURES_FORCE") == "1"
    for name, build in FIXTURES:
        if only and name not in only:
            continue
        path = os.path.join(out_dir, name + ".gh")
        if os.path.exists(path) and not force:
            continue
        build().save(path)


if __name__ == "__main__":
    main()
