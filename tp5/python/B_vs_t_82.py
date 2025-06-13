import os
import json
import numpy as np
import matplotlib.pyplot as plt

def is_blocked(filename):
    """
    Determina si una simulación está bloqueada analizando el archivo de salida.
    Una simulación está bloqueada si en el último tiempo hay partículas presentes.
    """
    if not os.path.exists(filename):
        return None  # Archivo no existe
    
    try:
        last_particles = []
        with open(filename, "r") as f:
            for line in f:
                # Solo líneas con partículas (las que tienen ';')
                if ";" in line:
                    # Ejemplo de línea:
                    # {"step": 101, "time": 0.101} ; {"id": 1, ...} ; {"id": 2, ...}
                    step_data, *particle_data = line.strip().split(" ; ")
                    
                    # Contar partículas en esta línea
                    current_particles = []
                    for pdata in particle_data:
                        pdata = pdata.strip()
                        if pdata.endswith(","):
                            pdata = pdata[:-1]
                        if pdata:  # Si hay datos de partícula
                            try:
                                p = json.loads(pdata)
                                current_particles.append(p)
                            except json.JSONDecodeError:
                                continue
                    
                    # Actualizar la última lista de partículas
                    last_particles = current_particles
        
        # Si en el último tiempo hay partículas, está bloqueado
        return len(last_particles) > 2
        
    except Exception as e:
        print(f"Error leyendo {filename}: {e}")
        return None

# Parámetros
B_values = [0.02, 0.04, 0.06, 0.08, 0.10]
num_runs = 11
output_dir = "../bin/Debug/net8.0"
file_template = "probability/output-simple-Q8-B{}-beeman-run-{}.txt"
penalized_tf = 82.7  # valor a usar si la corrida está bloqueada

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
        
        # Verificar si está bloqueado usando la función is_blocked
        blocked = is_blocked(file_path)
        if blocked is True:
            print(f"[B={B_val} | run={run}] Bloqueado → tf = {penalized_tf}")
            tf_values.append(penalized_tf)
            continue
        elif blocked is None:
            print(f"[B={B_val} | run={run}] Archivo no encontrado.")
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

    # Rellenar con penalizaciones si hay menos de num_runs
    while len(tf_values) < num_runs:
        print(f"[B={B_val}] Rellenando con tf = {penalized_tf}")
        tf_values.append(penalized_tf)

    # Guardar promedio y error estándar
    B_list.append(B_val)
    tf_means.append(np.mean(tf_values))
    tf_errors.append(np.std(tf_values) / np.sqrt(len(tf_values)))

# Graficar
plt.figure(figsize=(10, 5))
plt.errorbar(B_list, tf_means, yerr=tf_errors, fmt='-o', capsize=5)
plt.xlabel(r"$B$ [m]", fontsize=20)
plt.ylabel(r"$\langle t_f \rangle$ [s]", fontsize=20)
plt.gca().xaxis.major.formatter._useMathText = True

# Aumentar tamaño de fuente de la notación científica
plt.tick_params(axis='both', which='major', labelsize=20)
plt.gca().xaxis.offsetText.set_fontsize(20)
plt.xticks(fontsize=20)
plt.yticks(fontsize=20)
plt.grid(True)
plt.tight_layout()
plt.show()
