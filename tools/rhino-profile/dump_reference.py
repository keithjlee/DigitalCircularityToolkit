#! python3
"""Dump the documentation reference for every DCT component: identity, description, parameters (with types,
access, defaults) and the 24x24 icon. Run after changing any component, then commit the result:

    tools/rhino-profile/run.sh tools/rhino-profile/dump_reference.py

Writes docs/reference/components.json and docs/src/assets/icons/<ClassName>.png. docs/make.jl turns the JSON into
the Component reference pages (CI cannot run Rhino, so the JSON is committed)."""
import json
import os

import Rhino
import clr
clr.AddReference("Grasshopper")
clr.AddReference("System.Drawing")
import Grasshopper as gh
from System.Drawing.Imaging import ImageFormat

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.environ.get("DCT_PROFILE_SCRIPT", __file__)), "..", ".."))
OUT_JSON = os.path.join(ROOT, "docs", "reference", "components.json")
ICON_DIR = os.path.join(ROOT, "docs", "src", "assets", "icons")

gh_if = Rhino.RhinoApp.GetPlugInObject("Grasshopper")
gh_if.LoadEditor()


def defaults(p):
    """Persistent (default) values of a parameter, as strings; empty when none."""
    data = getattr(p, "PersistentData", None)
    if data is None or data.DataCount == 0:
        return []
    return [str(g) for g in data.AllData(True)]


def params(ps):
    return [{"name": p.Name, "nick": p.NickName, "description": p.Description, "type": p.TypeName,
             "access": str(p.Access).lower(), "optional": bool(p.Optional), "default": defaults(p)} for p in ps]


os.makedirs(os.path.dirname(OUT_JSON), exist_ok=True)
os.makedirs(ICON_DIR, exist_ok=True)

out = []
for proxy in gh.Instances.ComponentServer.ObjectProxies:
    # Obsolete (replaced) components stay documented: saved files still use them.
    if proxy.Desc.Category != "DigitalCircularityToolkit":
        continue
    obj = proxy.CreateInstance()
    cls = obj.GetType().Name
    rec = {"name": proxy.Desc.Name, "nick": proxy.Desc.NickName, "description": proxy.Desc.Description,
           "guid": str(proxy.Guid), "subcategory": proxy.Desc.SubCategory, "class": cls,
           "exposure": str(proxy.Exposure), "obsolete": bool(proxy.Obsolete), "variable_inputs": isinstance(obj, gh.Kernel.IGH_VariableParameterComponent)}
    if hasattr(obj, "Params"):
        rec["inputs"] = params(obj.Params.Input)
        rec["outputs"] = params(obj.Params.Output)
    icon = obj.Icon_24x24
    if icon is not None:
        icon.Save(os.path.join(ICON_DIR, cls + ".png"), ImageFormat.Png)
        rec["icon"] = "assets/icons/%s.png" % cls
    out.append(rec)

out.sort(key=lambda r: (r["subcategory"], r["name"]))
with open(OUT_JSON, "w") as f:
    json.dump(out, f, indent=1, ensure_ascii=False)
    f.write("\n")
