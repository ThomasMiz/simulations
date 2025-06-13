import json
import os
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import glob

# Valores de B a procesar
B_values = [0.02, 0.04, 0.06, 0.08, 0.1]

# Intervalo base
t1 = 10.0
t2_max = 40.0

B_vals = []
mean_vx_vals = []
error_vx_vals = []

def parse_output_file(filepath):
    with open(filepath, 'r') as f:
        lines = f.readlines()

    frames = []
    current_frame = None

    for line in lines:
        line = line.strip()
        if line.startswith("{\"step\""):
            if current_frame is not None and "particles" in current_frame:
                frames.append(current_frame)
            parts = line.split(" ; ")
            step_info = json.loads(parts[0])
            current_frame = {"step": step_info["step"], "time": step_info["time"], "particles": []}
            if len(parts) > 1:
                try:
                    current_frame["particles"].append(json.loads(parts[1]))
                except json.JSONDecodeError:
                    pass
        elif current_frame is not None:
            try:
                current_frame["particles"].append(json.loads(line))
            except json.JSONDecodeError:
                continue

    if current_frame is not None and "particles" in current_frame:
        frames.append(current_frame)

    return frames

for B in B_values:
    # Buscar todos los archivos que coincidan con el patrón para este B
    pattern = f"../bin/Debug/net8.0/probability/output-simple-Q8-B{B}-beeman-run-*.txt"
    matching_files = glob.glob(pattern)
    
    if not matching_files:
        print(f"No se encontraron archivos para B = {B}")
        continue

    all_vx_means = []
    
    for filename in matching_files:
        frames = parse_output_file(filename)
        if not frames:
            continue

        t_max = max(f["time"] for f in frames)
        t2 = min(t2_max, t_max)

        vx_t = []
        for frame in frames:
            t = frame["time"]
            if t1 <= t <= t2 and frame["particles"]:
                vx_vals = [abs(p["vx"]) for p in frame["particles"]]
                avg_vx = sum(vx_vals) / len(vx_vals)
                vx_t.append(avg_vx)

        if vx_t:
            mean_vx = np.mean(vx_t)
            all_vx_means.append(mean_vx)

    if all_vx_means:
        # Calcular el promedio y error entre corridas
        mean_vx = np.mean(all_vx_means)
        std_vx = np.std(all_vx_means)
        error = std_vx / np.sqrt(len(all_vx_means))

        B_vals.append(B)
        mean_vx_vals.append(mean_vx)
        error_vx_vals.append(error)

# Graficar
plt.errorbar(B_vals, mean_vx_vals, yerr=error_vx_vals, fmt='o-', capsize=5, label="⟨|vx|⟩")
plt.xlabel(r"$B$ [m]", fontsize=20)
plt.ylabel(r"$\langle |v_{x}| \rangle$ [m/s]", fontsize=20)
plt.grid(True)
plt.xticks(fontsize=20)
plt.yticks(fontsize=20)

# Aumentar el número de ticks en el eje Y
plt.locator_params(axis='y', nbins=10)

plt.tick_params(axis='both', which='major', labelsize=20)
plt.gca().xaxis.offsetText.set_fontsize(20)
plt.tight_layout()
plt.show()
