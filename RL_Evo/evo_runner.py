import subprocess, time
from pathlib import Path

BUILD_EXE    = Path(r"M:\RL_Model_1\Build\RL_Model_1.exe")
CONFIG_YAML  = Path("config/pred_prey_sac.yaml")
RUN_ID       = "sac_3"
RESULTS_ROOT = Path("results")
NUM_ENVS     = 3
BASE_PORT    = 5005
TIME_SCALE   = 20

RESULTS_ROOT.mkdir(exist_ok=True)

unity_procs = []
for i in range(NUM_ENVS):
    port = BASE_PORT + i
    p = subprocess.Popen([
        str(BUILD_EXE),
        "-batchmode", "-nographics",
        "--port", str(port)
    ])
    unity_procs.append(p)

time.sleep(5)

trainer = subprocess.Popen([
    "mlagents-learn",
    str(CONFIG_YAML),
    "--run-id", RUN_ID,
    "--env", str(BUILD_EXE),
    "--no-graphics",
    "--time-scale", str(TIME_SCALE),
    "--num-envs", str(NUM_ENVS),
    "--base-port", str(BASE_PORT),
    "--force",
    "--results-dir", str(RESULTS_ROOT)
])

trainer.wait()

for p in unity_procs:
    p.terminate()
    p.wait()