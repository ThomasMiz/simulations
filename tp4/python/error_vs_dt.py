import matplotlib.pyplot as plt
import numpy as np
import os
from file_loader import parse_simulation_file
from math import exp, sqrt

# Ruta donde están tus archivos .txt
base_path = "../bin/Debug/net8.0"

# Parámetros del sistema físico
m = 70.0
k = 1e4
gamma = 100.0
A = 1.0

# Solución analítica del oscilador amortiguado
def analytical_position(t):
    omega0 = sqrt(k / m - (gamma / (2 * m))**2)
    return A * exp(-gamma * t / (2 * m)) * np.cos(omega0 * t)

# Archivos por método
methods_files = {
    "Verlet": {
        "output-simple-verlet-500steps-dt1e-002.txt": 1e-2,
        "output-simple-verlet-5000steps-dt1e-003.txt": 1e-3,
        "output-simple-verlet-50000steps-dt1e-004.txt": 1e-4,
        "output-simple-verlet-500000steps-dt1e-005.txt": 1e-5,
        "output-simple-verlet-5000000steps-dt1e-006.txt": 1e-6,
    },
    "Beeman": {
        "output-simple-beeman-500steps-dt1e-002.txt": 1e-2,
        "output-simple-beeman-5000steps-dt1e-003.txt": 1e-3,
        "output-simple-beeman-50000steps-dt1e-004.txt": 1e-4,
        "output-simple-beeman-500000steps-dt1e-005.txt": 1e-5,
        "output-simple-beeman-5000000steps-dt1e-006.txt": 1e-6,
    },
    "Gear Corrector Predictor": {
        "output-simple-gear5-500steps-dt1e-002.txt": 1e-2,
        "output-simple-gear5-5000steps-dt1e-003.txt": 1e-3,
        "output-simple-gear5-50000steps-dt1e-004.txt": 1e-4,
        "output-simple-gear5-500000steps-dt1e-005.txt": 1e-5,
        "output-simple-gear5-5000000steps-dt1e-006.txt": 1e-6,
    }
}

# Calcular el MSE por método
method_errors = {}

for method, files in methods_files.items():
    errors = []
    for fname, dt in files.items():
        full_path = os.path.join(base_path, fname)
        if not os.path.exists(full_path):
            print(f"Archivo no encontrado: {full_path}")
            continue
        sim = parse_simulation_file(full_path)
        positions = [step.particles[0].position[0] for step in sim.steps]
        times = [step.time for step in sim.steps]
        analytic = [analytical_position(t) for t in times]
        mse = np.mean([(x - y) ** 2 for x, y in zip(positions, analytic)])
        errors.append((dt, mse))
    method_errors[method] = sorted(errors)

# Graficar
plt.figure(figsize=(8, 6))
markers = {'Verlet': 'o', 'Beeman': 's', 'Gear Corrector Predictor': 'o'}
colors = {'Verlet': 'tab:blue', 'Beeman': 'tab:orange', 'Gear Corrector Predictor': 'tab:green'}

for method, values in method_errors.items():
    dts, mses = zip(*values)
    plt.loglog(dts, mses, marker=markers[method], label=method, color=colors[method])

plt.xlabel("Tiempo (s)")
plt.ylabel("MSE")
plt.title("Error cuadrático medio vs paso de integración")
plt.grid(True, which='both', linestyle='--', linewidth=0.5)
plt.legend()
plt.tight_layout()
plt.savefig("mse_vs_dt.png")
plt.show()
