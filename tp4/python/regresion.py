import os
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from scipy.optimize import minimize_scalar
import warnings

# Suprimir warnings innecesarios
warnings.filterwarnings('ignore', category=RuntimeWarning)

# === Carpeta con los CSV ===
resonancias_dir = "../bin/Debug/net8.0/resonancias"

# === Leer datos ===
data = []

for fname in os.listdir(resonancias_dir):
    if fname.startswith("omega_resonancia_k") and fname.endswith(".csv"):
        path = os.path.join(resonancias_dir, fname)
        df = pd.read_csv(path)
        if "k (kg/s^2)" in df.columns and "omega_resonancia (rad/s)" in df.columns:
            k = df["k (kg/s^2)"].iloc[0]
            omega = df["omega_resonancia (rad/s)"].iloc[0]
            data.append((k, omega))

# === Ordenar por k ===
data.sort(key=lambda x: x[0])
ks, omegas = zip(*data)
ks = np.array(ks)
omegas = np.array(omegas)

# === Modelo teórico: omega0 = c * sqrt(k)
def modelo(k, c):
    return c * np.sqrt(k)

# === Error cuadrático total
def error_cuadratico(c):
    return np.sum((omegas - modelo(ks, c)) ** 2)

# === Encontrar el c que minimiza el error
res = minimize_scalar(error_cuadratico, bounds=(0.1, 1.0), method='bounded')
c_opt = res.x
E_min = res.fun

# === Graficar ajuste
k_vals = np.linspace(min(ks), max(ks), 300)
fit_vals = modelo(k_vals, c_opt)

plt.figure()
plt.plot(ks, omegas, 'o', label='ω₀ simulada')
plt.plot(k_vals, fit_vals, 'r--', label=f'Fit: ω₀ = {c_opt:.3f}·√k')
plt.title('Relación ω₀ vs k (ajuste según teoría)')
plt.xlabel('k (kg/s²)')
plt.ylabel('ω₀ (rad/s)')
plt.grid(True)
plt.legend()
plt.tight_layout()
plt.show()

# === Graficar error E(c)
c_values = np.linspace(0.1, 1.0, 200)
errors = [error_cuadratico(c) for c in c_values]

plt.figure()
plt.plot(c_values, errors, 'b-')
plt.axvline(c_opt, color='r', linestyle='--')
plt.plot(c_opt, E_min, 'ro', label=f'Mínimo: c* = {c_opt:.3f}')
plt.title('Error del ajuste E(c) en función de c')
plt.xlabel('c')
plt.ylabel('E(c)')
plt.legend()
plt.tight_layout()
plt.show()
