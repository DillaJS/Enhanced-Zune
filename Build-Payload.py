from pathlib import Path
import runpy
runpy.run_path(str(Path(__file__).with_name("Build-RC7-Payload.py")),run_name="__main__")
