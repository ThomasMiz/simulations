import os
import json
import numpy as np
import matplotlib.pyplot as plt

# Parámetros
q_values = [2, 4, 6, 8]
num_runs = 10
output_dir = "../bin/Debug/net8.0/q_vs_t/"
file_template = "output-simple-Q{}-beeman-run-{}"
penalized_tf = 90.0  # valor a usar si la corrida está bloqueada

qins = []
tf_means = []
tf_errors = []

for Q in q_values:
    tf_values = []

    for run in range(1, num_runs + 1):
        file_base = file_template.format(Q, run)
        file_path_clean = os.path.join(output_dir, file_base + ".txt")
        file_path_blocked = os.path.join(output_dir, file_base + "-b.txt")

        # Si está bloqueado, agregar penalización
        if os.path.exists(file_path_blocked):
            print(f"[Q={Q} | run={run}] Bloqueado → tf = {penalized_tf}")
            tf_values.append(penalized_tf)
            continue

        # Si no existe ningún archivo, ignorar
        elif not os.path.exists(file_path_clean):
            print(f"[Q={Q} | run={run}] Archivo no encontrado.")
            continue

        # Leer tiempo final
        last_time = None
        with open(file_path_clean, 'r') as f:
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

    # Rellenar con penalizaciones si hay menos de 10
    while len(tf_values) < num_runs:
        print(f"[Q={Q}] Rellenando con tf = {penalized_tf}")
        tf_values.append(penalized_tf)

    # Guardar promedio y error estándar
    qins.append(Q)
    tf_means.append(np.mean(tf_values))
    tf_errors.append(np.std(tf_values) / np.sqrt(len(tf_values)))

# Graficar
plt.figure(figsize=(10, 5))
plt.errorbar(qins, tf_means, yerr=tf_errors, fmt='-o', capsize=5)
plt.xlabel(r"$Q_{\mathrm{in}}$ [1/s]", fontsize=20)
plt.ylabel(r"$\langle t_f \rangle$ [s]", fontsize=20)
plt.xticks(fontsize=20)
plt.yticks(fontsize=20)
plt.grid(True)
plt.tight_layout()
plt.show()
