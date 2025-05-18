import os
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from scipy.optimize import curve_fit
import warnings

# Suprimir la advertencia de optimización
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

# === Ajuste forzado al origen: omega = C * sqrt(k) ===
def model(k, C):
    return C * np.sqrt(k)

try:
    params, pcov = curve_fit(model, ks, omegas, p0=[1], bounds=(0, np.inf))
    C_fit = params[0]
    k_fit = np.linspace(0, max(ks), 300)  # desde 0 hasta el máximo k
    omega_fit = model(k_fit, C_fit)

    # === Gráfico ===
    plt.figure(figsize=(8, 5))
    plt.plot(ks, omegas, 'o', label="ω₀")
    plt.plot(k_fit, omega_fit, '--', color='red', label=f"Fit: ω₀ = {C_fit:.2f}·√k")
    plt.xlabel("k [kg/s²]", fontsize=20)
    plt.ylabel("ω₀ [rad/s]", fontsize=20)
    #plt.title("Relación ω₀ vs k (con ajuste desde el origen)")
    plt.grid(True)
    plt.xticks(fontsize=20)
    plt.yticks(fontsize=20)
    plt.legend(fontesize=20)
    plt.tight_layout()
    plt.savefig("omega_resonancia_vs_k_fit_forzado_origen.png", format='png', dpi=300)
    plt.show()

    print(f"Constante de ajuste C ≈ {C_fit:.4f} (rad/s) / √(kg/s²)")

    # === Gráfico de E(c) vs c (error en función del coeficiente) ===
    c_values = np.linspace(0.6, 1.0, 300)
    errors = []

    for c in c_values:
        omega_model = model(ks, c)
        error = np.sum((omegas - omega_model) ** 2)
        errors.append(error)

    # Encontrar mínimo visualmente
    min_index = np.argmin(errors)
    c_optimo = c_values[min_index]
    E_min = errors[min_index]

    # Graficar
    plt.figure(figsize=(8, 5))
    plt.plot(c_values, errors, color='blue')
    plt.axvline(c_optimo, color='red', linestyle='--')

    plt.scatter([c_optimo], [E_min], color='blue', label=f"mínimo: C* = {c_optimo:.3f}")
    plt.xlabel("c")
    plt.ylabel("E(c)")
   # plt.title("Error del ajuste E(c) en función de c")
    plt.grid(True)
    plt.legend()
    plt.tight_layout()
    plt.savefig("error_ec_vs_c.png", format='png', dpi=300)
    plt.show()

except Exception as e:
    print(f"Error durante el ajuste o la generación de gráficos: {str(e)}")
