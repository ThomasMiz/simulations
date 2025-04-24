import os, struct, re
import numpy as np
import matplotlib.pyplot as plt
from collections import defaultdict

# === CONFIGURACIÓN ===
R_obst = 0.005
tol_factor = 0.00001
sim_dir = "../outputs"
pattern = r"output\d+-fixedobstacle-250particles-vel(\d+)-50ksteps.sim"

# === PARSER .sim ===
def read_collisions(simfile):
    data = open(simfile, "rb").read()
    offset = 4
    N = struct.unpack_from("I", data, offset)[0]; offset += 4
    radii = np.empty(N, dtype=np.float32)
    for i in range(N):
        _, r = struct.unpack_from("ff", data, offset)
        radii[i] = r
        offset += 8

    times_all = []
    t_final = 0.0

    while offset + 8 + 16*N <= len(data):
        _, t = struct.unpack_from("If", data, offset)
        offset += 8
        pos = np.empty((N,2), dtype=np.float32)
        for i in range(N):
            x,y,_,_ = struct.unpack_from("ffff", data, offset)
            pos[i] = (x,y)
            offset += 16
        t_final = t

        dist = np.hypot(pos[:,0], pos[:,1])
        sum_r = R_obst + radii
        tol = tol_factor * sum_r
        collided = np.abs(dist - sum_r) < tol
        idx = np.nonzero(collided)[0]
        for i in idx:
            times_all.append(t)

    ν_all = len(times_all) / t_final
    return ν_all

# === AGRUPAR ARCHIVOS POR VELOCIDAD ===
files_by_vel = defaultdict(list)
for fname in os.listdir(sim_dir):
    match = re.match(pattern, fname)
    if match:
        vel = int(match.group(1))
        files_by_vel[vel].append(os.path.join(sim_dir, fname))

# === PROCESAR SOLO ν_all ===
results = []
for vel, filelist in sorted(files_by_vel.items()):
    ν_all_list = []
    for f in filelist:
        ν_all = read_collisions(f)
        ν_all_list.append(ν_all)

    T = vel ** 2
    results.append({
        "vel": vel,
        "T": T,
        "ν_all_mean": np.mean(ν_all_list),
        "ν_all_std": np.std(ν_all_list),
    })

# === GRAFICO ÚNICO ===
T_vals = [r["T"] for r in results]
ν_all = [r["ν_all_mean"] for r in results]
ν_all_err = [r["ν_all_std"] for r in results]

plt.figure(figsize=(8,5))
plt.errorbar(T_vals, ν_all, yerr=ν_all_err, fmt='o-', color='darkorange', capsize=3)
plt.xlabel("Temperatura T (proporcional a v₀²)")
plt.ylabel("Frecuencia de choques total [1/s]")
plt.title("ν (todos los choques) vs Temperatura")
plt.grid(True, alpha=0.3)
plt.tight_layout()
plt.savefig("observable_all_vs_T.png", dpi=150)
plt.show()
