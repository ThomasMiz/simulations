import os
import json
import numpy as np
import matplotlib.pyplot as plt

# Parámetros
q_values = [2, 4, 6, 8]
num_runs = 10
output_dir = "../bin/Debug/net8.0/q_vs_t/"
file_template = "output-simple-Q{}-beeman-run-{}"

qins = []
tf_means = []
tf_errors = []

for Q in q_values:
    tf_values = []

    for run in range(1, num_runs + 1):
        file_base = file_template.format(Q, run)
        file_path_blocked = os.path.join(output_dir, file_base + "-b.txt")
        file_path_clean = os.path.join(output_dir, file_base + ".txt")

        # Usar el archivo bloqueado si existe, si no, el normal
        file_path = None
        if os.path.exists(file_path_blocked):
            file_path = file_path_blocked
        elif os.path.exists(file_path_clean):
            file_path = file_path_clean
        else:
            print(f"[Q={Q} | run={run}] Archivo no encontrado.")
            continue

        # Leer último time del archivo
        last_time = None
        with open(file_path, 'r') as f:
            for line in f:
                if "step" not in line:
                    continue
                try:
                    time_info = json.loads(line.strip().split(" ; ")[0])
                    last_time = time_info["time"]
                except (json.JSONDecodeError, IndexError, KeyError):
                    continue

        if last_time is not None:
            tf_values.append(last_time)

    if tf_values:
        qins.append(Q)
        tf_means.append(np.mean(tf_values))
        tf_errors.append(np.std(tf_values) / np.sqrt(len(tf_values)))
        print(f"Q = {Q}: {len(tf_values)} muestras usadas")
    else:
        print(f"Q={Q}: no se pudieron leer tiempos finales válidos.")

# Graficar
plt.figure(figsize=(10, 5))
plt.errorbar(qins, tf_means, yerr=tf_errors, fmt='-o', capsize=5)
plt.xlabel(r"$Q_{\mathrm{in}}$ [1/s]", fontsize=16)
plt.ylabel(r"$\langle t_f \rangle$ [s]", fontsize=16)
plt.xticks(fontsize=14)
plt.yticks(fontsize=14)
plt.grid(True)
plt.tight_layout()
plt.show()
