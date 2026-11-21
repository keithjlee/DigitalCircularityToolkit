#! python3
"""Dump every DCT component's identity and parameters (name, nickname, type, access, optional) to JSON.
Output: env DCT_CATALOG_OUT. Used to write fixtures and to detect parameter changes (COMPONENT_SPEC §3)."""
import json
import os
import Rhino
import clr
clr.AddReference("Grasshopper")
import Grasshopper as gh

gh_if = Rhino.RhinoApp.GetPlugInObject("Grasshopper")
gh_if.LoadEditor()

def params(ps):
    return [{"name": p.Name, "nick": p.NickName, "type": p.GetType().Name, "access": str(p.Access),
             "optional": bool(p.Optional)} for p in ps]

out = []
for proxy in gh.Instances.ComponentServer.ObjectProxies:
    if proxy.Desc.Category != "DigitalCircularityToolkit":
        continue
    obj = proxy.CreateInstance()
    rec = {"name": proxy.Desc.Name, "nick": proxy.Desc.NickName, "guid": str(proxy.Guid),
           "subcategory": proxy.Desc.SubCategory, "class": obj.GetType().FullName, "exposure": str(proxy.Exposure)}
    if hasattr(obj, "Params"):
        rec["inputs"] = params(obj.Params.Input)
        rec["outputs"] = params(obj.Params.Output)
    out.append(rec)
out.sort(key=lambda r: r["name"])
with open(os.environ["DCT_CATALOG_OUT"], "w") as f:
    json.dump(out, f, indent=1)
