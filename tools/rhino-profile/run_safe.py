#! python3
"""Run the script named by env DCT_PROFILE_SCRIPT inside Rhino; write any traceback to <script>.error.txt
(Rhino's script errors otherwise only reach the Rhino command line, which is lost when running unattended).

If DCT_EXIT_WHEN_DONE=1, Rhino quits on the first idle after the script returns. Do NOT put `_-Exit` in the
startup macro instead: Grasshopper's LoadEditor() pumps the UI loop on macOS, and a queued `_-Exit` then fires
in the middle of the script."""
import os
import runpy
import traceback

import Rhino

target = os.environ["DCT_PROFILE_SCRIPT"]
try:
    runpy.run_path(target, run_name="__main__")
except BaseException:  # noqa: BLE001
    with open(os.path.splitext(target)[0] + ".error.txt", "w") as f:
        f.write(traceback.format_exc())

if os.environ.get("DCT_EXIT_WHEN_DONE") == "1":
    def _exit_on_idle(sender, e):
        Rhino.RhinoApp.Idle -= _exit_on_idle
        Rhino.RhinoApp.RunScript("_-Exit", False)
    Rhino.RhinoApp.Idle += _exit_on_idle
