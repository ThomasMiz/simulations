import json
import pandas as pd
import matplotlib.pyplot as plt
from collections import defaultdict
import os

# Lista de valores de B
B_values = [ 0.02, 0.08, 0.10]
file_template = "../bin/Debug/net8.0/output-simple-Q8-B{}-beeman-run-1.txt"

# Diccionario para almacenar curvas por B
results = {}

for B in B_values:
    file_path = file_template.format(B)

    if not os.path.exists(file_path):
        print(f"Archivo no encontrado para B = {B}: {file_path}")
        continue

    # Leer líneas del archivo
    with open(file_path, 'r') as f:
        lines = f.readlines()

    # Parsear datos por frame
    frames = []
    current_frame = None
    for line in lines:
        line = line.strip()
        if line.startswith("{\"step\""):
            if current_frame and "particles" in current_frame:
                frames.append(current_frame)
            parts = line.split(" ; ")
            step_info = json.loads(parts[0])
            current_frame = {"time": step_info["time"], "particles": []}
            if len(parts) > 1:
                try:
                    current_frame["particles"].append(json.loads(parts[1]))
                except json.JSONDecodeError:
                    pass
        elif current_frame:
            try:
                current_frame["particles"].append(json.loads(line))
            except json.JSONDecodeError:
                continue
    if current_frame and "particles" in current_frame:
        frames.append(current_frame)

    # Historial de posiciones por partícula
    particle_history = defaultdict(list)
    for frame in frames:
        t = frame["time"]
        for p in frame["particles"]:
            particle_history[p["id"]].append((t, p["x"]))

    # Calcular <|vx|> en bloques de 1 segundo
    start_time = min(frame["time"] for frame in frames)
    end_time = max(frame["time"] for frame in frames)
    step_size = 1.0  # segundos

    times = []
    avg_abs_vx_per_second = []

    t = start_time
    while t + step_size <= end_time:
        vx_list = []
        for particle_id, history in particle_history.items():
            x0 = x1 = None
            for i in range(len(history) - 1):
                if history[i][0] <= t < history[i+1][0]:
                    x0 = history[i][1]
                if history[i][0] <= t + step_size < history[i+1][0]:
                    x1 = history[i][1]
                    break
            if x0 is not None and x1 is not None:
                vx = (x1 - x0) / step_size
                vx_list.append(abs(vx))

        if vx_list:
            avg_vx = sum(vx_list) / len(vx_list)
            times.append(t + step_size / 2)
            avg_abs_vx_per_second.append(avg_vx)

        t += step_size

    df = pd.DataFrame({
        "time": times,
        "avg_abs_vx_1s": avg_abs_vx_per_second
    })

    results[B] = df

# ------------------------------
# Graficar todos los B en un mismo gráfico
# ------------------------------
plt.figure(figsize=(12, 6))

for B, df in results.items():
    plt.plot(df["time"], df["avg_abs_vx_1s"], marker='o', label=fr"B = {B} m")

#plt.axhline(y=1.5, color='red', linestyle='--', label=r"$v_d = 1.5$ m/s")
plt.xlabel("Tiempo [s]", fontsize=20)
plt.ylabel(r"$\langle |v_x| \rangle$ [m/s]", fontsize=20)
plt.xticks(fontsize=20)
plt.yticks(fontsize=20)
plt.grid(True)
plt.legend(fontsize=20, loc='lower right')
plt.tight_layout()
plt.show()
