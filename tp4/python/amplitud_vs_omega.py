import os
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.ticker as ticker
import numpy as np
import re


# === Ruta a la carpeta de amplitudes ===
amplitudes_dir = "../bin/Debug/net8.0/amplitudes"

# === Cargar todos los archivos .csv ===
data = []

# Definir k_deseado al inicio para usarlo en todo el script
k_deseado = "1.02e2"  # Cambiar este valor por el que quieras analizar  1.02e2

for fname in os.listdir(amplitudes_dir):
    # === FILTRO: solo incluir archivos con el k deseado ===
    if f"-k{k_deseado}" not in fname:
        continue

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
    k = value
    if k.is_integer():
        return f"{int(k)}"
    return f"{k:.1f}"

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



# === AGREGADO: guardar CSV con k y ω₀ ===

# Obtener la lista de archivos válidos
archivos_validos = [fname for fname in os.listdir(amplitudes_dir) if fname.endswith(".csv") and not fname.endswith("-.csv")]
archivo_max = archivos_validos[max_idx] if max_idx < len(archivos_validos) else None

if archivo_max:
    # Usar k_deseado directamente en lugar de extraerlo del nombre del archivo
    k_valor = float(k_deseado)



    # Crear carpeta 'resonancias' si no existe
    resonancias_dir = os.path.join(os.path.dirname(amplitudes_dir), "resonancias")
    os.makedirs(resonancias_dir, exist_ok=True)

    # Guardar CSV usando k_deseado directamente
    csv_filename = f"omega_resonancia_k{k_deseado}.csv"
    csv_path = os.path.join(resonancias_dir, csv_filename)

    with open(csv_path, "w") as f:
        f.write("k (kg/s^2),omega_resonancia (rad/s)\n")
        f.write(f"{k_valor},{omega_resonancia}\n")

    print(f"CSV guardado en: {csv_path}")
else:
    print("No se pudo determinar el archivo de máxima amplitud.")