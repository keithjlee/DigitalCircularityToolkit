#! python3
"""Used by scripts/rhino-dev.sh: load Grasshopper, show the editor and open the definition in env DCT_OPEN_GH.
(The `_-Grasshopper _Document _Open` command-line route is unreliable at startup on macOS.)"""
import os
import Rhino

gh = Rhino.RhinoApp.GetPlugInObject("Grasshopper")  # GH_RhinoScriptInterface
gh.LoadEditor()
gh.ShowEditor()
path = os.environ.get("DCT_OPEN_GH")
if path:
    gh.OpenDocument(path)
