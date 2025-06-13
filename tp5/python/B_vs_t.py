import os
import json
import numpy as np
import matplotlib.pyplot as plt

# Parámetros
B_values = [0.02, 0.04, 0.06, 0.08, 0.10]
num_runs = 5
output_dir = "../bin/Debug/net8.0/"
file_template = "output-simple-Q8-B{}-beeman-run-{}.txt"

B_list = []
tf_means = []
tf_errors = []

for B_val in B_values:
    tf_values = []

    for run in range(1, num_runs + 1):
        # Formatear B_val correctamente para los nombres de archivo
        if B_val == 0.10:
            B_str = "0.1"
        else:
            B_str = str(B_val)
        
        file_path = os.path.join(output_dir, file_template.format(B_str, run))
        if not os.path.exists(file_path):
            print(f"Archivo no encontrado: {file_path}")
            continue

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
        B_list.append(B_val)
        tf_means.append(np.mean(tf_values))
        tf_errors.append(np.std(tf_values) / np.sqrt(len(tf_values)))
    else:
        print(f"B={B_val}: no se pudieron leer tiempos finales.")

# Verificar si tenemos datos para graficar
if len(B_list) == 0:
    print("No se encontraron datos para graficar. Verificando archivos disponibles...")
    # Listar archivos disponibles
    for file in os.listdir(output_dir):
        if file.startswith("output-simple-Q8-B") and file.endswith(".txt"):
            print(f"Archivo encontrado: {file}")
else:
    # Graficar
    plt.figure(figsize=(10, 5))
    plt.errorbar(B_list, tf_means, yerr=tf_errors, fmt='-o', capsize=5)
    plt.xlabel(r"$\langle B [m]", fontsize=20)
    plt.ylabel(r"$\langle t_f [s]", fontsize=20)
    #plt.title(r"Tiempo promedio de arribo $\langle t_f \rangle$ vs. $Q_{\mathrm{in}}$", fontsize=18)
    # Agregar notación científica - corregida para mostrar 10^-2 solo en eje X
    plt.ticklabel_format(style='scientific', axis='x', scilimits=(-2,-2))
    plt.ticklabel_format(style='plain', axis='y')  # Mantener formato normal en eje Y
    plt.gca().xaxis.major.formatter._useMathText = True

    # Aumentar tamaño de fuente de la notación científica
    plt.tick_params(axis='both', which='major', labelsize=20)
    plt.gca().xaxis.offsetText.set_fontsize(20)
    plt.xticks(fontsize=20)
    plt.yticks(fontsize=20)
    plt.grid(True)
    plt.tight_layout()
    plt.show()
