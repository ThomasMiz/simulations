import os
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from scipy.optimize import curve_fit

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

# === Ajuste teórico omega = C * sqrt(k) ===
def model(k, C):
    return C * np.sqrt(k)

params, _ = curve_fit(model, ks, omegas)
C_fit = params[0]
omega_fit = model(ks, C_fit)

# === Gráfico ===
plt.figure(figsize=(8, 5))
plt.plot(ks, omegas, 'o', label="ω₀ simulada")
plt.plot(ks, omega_fit, '--', color='red', label=f"Fit: ω₀ = {C_fit:.2f}·√k")

plt.xlabel("k (kg/s²)")
plt.ylabel("ω₀ (rad/s)")
plt.title("Relación entre frecuencia de resonancia y constante elástica")
plt.grid(True)
plt.legend()
plt.tight_layout()
plt.savefig("omega_resonancia_vs_k_fit.png")
plt.show()

print(f"Constante de ajuste C ≈ {C_fit:.4f} (rad/s) / √(kg/s²)")
