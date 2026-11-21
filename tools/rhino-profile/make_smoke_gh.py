#! python3
"""Write a minimal smoke-test definition (one DCT component of each listed name) using the loaded plugin.
Output path: env DCT_SMOKE_GH. Run after `_Grasshopper` has loaded the plugin."""
import os
import clr
clr.AddReference("Grasshopper")
import System
import Grasshopper as gh
from Grasshopper.Kernel import GH_Document, GH_DocumentIO

names = os.environ.get("DCT_SMOKE_COMPONENTS", "Object (DCT);OverridePCA (DCT)").split(";")
doc = GH_Document()
x = 0
for name in names:
    proxy = next(p for p in gh.Instances.ComponentServer.ObjectProxies if p.Desc.Name == name)
    obj = proxy.CreateInstance()
    obj.CreateAttributes()
    obj.Attributes.Pivot = System.Drawing.PointF(x, 0)
    doc.AddObject(obj, False)
    x += 150
io = GH_DocumentIO(doc)
io.SaveQuiet(os.environ["DCT_SMOKE_GH"])
