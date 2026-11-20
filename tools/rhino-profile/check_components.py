#! python3
"""Component conformance + load check, run inside Rhino after Grasshopper has loaded (see docs/specs/COMPONENT_SPEC.md §7).

Checks every DCT component proxy: unique GUIDs, "(DCT)" name suffix, known subcategory, icon present, and which
assembly it came from (to confirm the dev snapshot, not a stale install, was loaded). Also reports the open
Grasshopper documents and their DCT components. Writes JSON to env DCT_CHECK_OUT (default: next to this file)."""
import json
import os
import clr
clr.AddReference("Grasshopper")
import Grasshopper as gh

TAB = "DigitalCircularityToolkit"
SUBCATS = {"Objects", "Sets", "Characterization", "Distance", "Matching", "Utilities"}
out = {"errors": [], "warnings": []}

proxies = [p for p in gh.Instances.ComponentServer.ObjectProxies if p.Desc.Category == TAB]
out["component_count"] = len(proxies)
out["assemblies"] = sorted({str(p.Location) for p in proxies})
seen = {}
for p in proxies:
    d = p.Desc
    g = str(p.Guid)
    if g in seen:
        out["errors"].append("duplicate GUID %s: %s / %s" % (g, seen[g], d.Name))
    seen[g] = d.Name
    if not d.Name.endswith("(DCT)"):
        out["errors"].append("name without '(DCT)' suffix: %s" % d.Name)
    if d.SubCategory not in SUBCATS:
        out["errors"].append("unknown subcategory '%s': %s" % (d.SubCategory, d.Name))
    if p.Icon is None:
        out["errors"].append("missing icon: %s" % d.Name)
out["components"] = sorted("%s [%s] %s%s" % (p.Desc.Name, p.Desc.SubCategory, p.Guid, " (obsolete)" if p.Obsolete else "") for p in proxies)

docs = []
for doc in gh.Instances.DocumentServer:
    docs.append({
        "file": str(doc.FilePath),
        "objects": sorted("%s:%s" % (o.Name, o.RuntimeMessageLevel) for o in doc.Objects),
    })
out["documents"] = docs
out["ok"] = not out["errors"]

path = os.environ.get("DCT_CHECK_OUT") or os.path.join(os.path.dirname(os.path.abspath(__file__)), "check_components_results.json")
with open(path, "w") as f:
    json.dump(out, f, indent=1)
