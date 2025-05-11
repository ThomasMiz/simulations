import os
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.ticker as ticker
import numpy as np

# === Ruta a la carpeta de amplitudes ===
amplitudes_dir = "../bin/Debug/net8.0/amplitudes"

# === Cargar todos los archivos .csv ===
data = []

for fname in os.listdir(amplitudes_dir):
    if not fname.endswith(".csv"):
        continue
    path = os.path.join(amplitudes_dir, fname)
    df = pd.read_csv(path)
    if "omega" in df.columns and "amplitud_max" in df.columns:
        omega = df["omega"].iloc[0]
        amplitud = df["amplitud_max"].iloc[0]
        data.append((omega, amplitud))

# === Ordenar por omega ===
data.sort(key=lambda x: x[0])
omegas, amplitudes = zip(*data)

# === Encontrar omega de resonancia (máxima amplitud) ===
max_idx = amplitudes.index(max(amplitudes))
omega_resonancia = omegas[max_idx]
amplitud_maxima = amplitudes[max_idx]

# === Graficar ===
plt.figure(figsize=(10, 6))
plt.plot(omegas, amplitudes, marker="o", linestyle="-", label="Amplitud máxima")
plt.axvline(omega_resonancia, color="red", linestyle="--", label=f"ω₀ ≈ {omega_resonancia:.2f} rad/s")
plt.scatter([omega_resonancia], [amplitud_maxima], color="red", zorder=5)

# Configurar el formateador de ticks para mostrar múltiplos de π
def format_func(value, tick_number):
    if value == 0:
        return "0"
    k = value / np.pi
    if k == 1:
        return "π"
    elif k == -1:
        return "-π"
    elif k.is_integer():
        return f"{int(k)}π"
    return f"{k:.1f}π"

# Establecer los ticks exactos de omega
plt.gca().set_xticks(omegas)
plt.gca().xaxis.set_major_formatter(ticker.FuncFormatter(format_func))

plt.title("Amplitud máxima vs. Frecuencia ω")
plt.xlabel("ω (rad/s)")
plt.ylabel("Amplitud máxima |y| (m)")
plt.grid(True)
plt.legend()
plt.tight_layout()
plt.savefig("amplitud_vs_omega.png")
plt.show()
