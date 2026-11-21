#! python3
"""One unattended Rhino session: load Grasshopper (and the dev DCT build), generate missing fixtures, record outputs.
Run through tests/regress.sh, which sets the environment (see make_fixtures.py and record.py)."""
import os
import runpy
import Rhino

Rhino.RhinoApp.GetPlugInObject("Grasshopper").LoadEditor()
here = os.path.dirname(os.path.abspath(__file__))
runpy.run_path(os.path.join(here, "make_fixtures.py"), run_name="__main__")
if os.environ.get("DCT_RECORD_DIR"):
    runpy.run_path(os.path.join(here, "record.py"), run_name="__main__")
