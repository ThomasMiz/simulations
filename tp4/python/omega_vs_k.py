import os
import pandas as pd
import matplotlib.pyplot as plt

# Definir carpeta donde están los CSV generados por el script anterior
resonancias_dir = "../bin/Debug/net8.0/resonancias"

# Leer todos los archivos CSV que empiecen con "omega_resonancia_k" y terminen en .csv
data = []

for fname in os.listdir(resonancias_dir):
    if fname.startswith("omega_resonancia_k") and fname.endswith(".csv"):
        path = os.path.join(resonancias_dir, fname)
        df = pd.read_csv(path)
        if "k (kg/s^2)" in df.columns and "omega_resonancia (rad/s)" in df.columns:
            k = df["k (kg/s^2)"].iloc[0]
            omega = df["omega_resonancia (rad/s)"].iloc[0]
            data.append((k, omega))

# Ordenar por k
data.sort(key=lambda x: x[0])
ks, omegas = zip(*data)

# Crear gráfico
plt.figure(figsize=(8, 5))
plt.plot(ks, omegas, marker="o", linestyle="-")
plt.xlabel("k (kg/s²)")
plt.ylabel("ω₀ (rad/s)")
plt.title("Frecuencia de resonancia ω₀ vs constante elástica k")
plt.grid(True)
plt.tight_layout()
plt.savefig("omega_resonancia_vs_k.png")
plt.show()
