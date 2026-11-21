#! python3
"""Smoke test of a dev build inside Rhino (no clicks): load Grasshopper (and every GHA), write a small definition
with the listed DCT components, open it, then run the component conformance check.

    DCT_SMOKE_GH=/path/smoke.gh  DCT_SMOKE_COMPONENTS="Object (DCT);OverridePCA (DCT)"  DCT_CHECK_OUT=/path/out.json
    DCT_PROFILE_SCRIPT=<this file>  DCT_MACRO='_-RunPythonScript "<repo>/tools/rhino-profile/run_safe.py" _-Exit' \\
        scripts/rhino-dev.sh
"""
import os
import runpy
import Rhino

gh_if = Rhino.RhinoApp.GetPlugInObject("Grasshopper")   # GH_RhinoScriptInterface
gh_if.LoadEditor()                                     # loads all GHAs, incl. RHINO_PACKAGE_DIRS
here = os.path.dirname(os.path.abspath(__file__))
path = os.environ["DCT_SMOKE_GH"]
if not os.path.exists(path):
    runpy.run_path(os.path.join(here, "make_smoke_gh.py"))
gh_if.OpenDocument(path)
runpy.run_path(os.path.join(here, "check_components.py"))
gh_if.CloseDocument()
