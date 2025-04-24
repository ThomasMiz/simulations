import os, struct, re
import numpy as np
import matplotlib.pyplot as plt
from collections import defaultdict

# === CONFIG ===
R_obst = 0.005
tol_factor = 0.00001
sim_dir = "../outputs"
pattern = r"output\d+-fixedobstacle-250particles-vel(\d+)-50ksteps.sim"

# === FUNCIONES ===
def read_tfirst(simfile):
    data = open(simfile, "rb").read()
    offset = 4
    N = struct.unpack_from("I", data, offset)[0]; offset += 4
    radii = np.empty(N, dtype=np.float32)
    for i in range(N):
        _, r = struct.unpack_from("ff", data, offset)
        radii[i] = r
        offset += 8

    hit_first = np.zeros(N, bool)
    times_first = np.full(N, np.nan, dtype=np.float32)

    while offset + 8 + 16*N <= len(data):
        _, t = struct.unpack_from("If", data, offset)
        offset += 8
        pos = np.empty((N,2), dtype=np.float32)
        for i in range(N):
            x,y,_,_ = struct.unpack_from("ffff", data, offset)
            pos[i] = (x,y)
            offset += 16

        dist = np.hypot(pos[:,0], pos[:,1])
        sum_r = R_obst + radii
        tol = tol_factor * sum_r
        collided = np.abs(dist - sum_r) < tol
        idx = np.nonzero(collided)[0]
        for i in idx:
            if not hit_first[i]:
                hit_first[i] = True
                times_first[i] = t

    return times_first[~np.isnan(times_first)]

# === AGRUPAR ARCHIVOS ===
files_by_vel = defaultdict(list)
for fname in os.listdir(sim_dir):
    match = re.match(pattern, fname)
    if match:
        vel = int(match.group(1))
        files_by_vel[vel].append(os.path.join(sim_dir, fname))

# === PROCESAR ===
results = []
for vel, filelist in sorted(files_by_vel.items()):
    all_t_first = []
    for f in filelist:
        t_first_i = read_tfirst(f)
        all_t_first.append(np.mean(t_first_i))
    avg_tfirst = np.mean(all_t_first)
    std_tfirst = np.std(all_t_first)
    T = vel ** 2
    results.append((T, avg_tfirst, std_tfirst))

# === GRAFICAR ===
results = np.array(results)
plt.figure(figsize=(8,5))
plt.errorbar(results[:,0], results[:,1], yerr=results[:,2], fmt='o--', capsize=4, color="mediumblue")
plt.xlabel("Temperatura T (proporcional a v₀²)")
plt.ylabel("⟨t₁er choque⟩ [s]")
plt.title("Tiempo promedio del primer choque vs Temperatura")
plt.grid(True, alpha=0.3)
plt.tight_layout()
plt.savefig("tfirst_vs_T.png", dpi=150)
plt.show()
