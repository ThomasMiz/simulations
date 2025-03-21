import numpy as np
import matplotlib.pyplot as plt

# 📂 Leer datos desde "output.txt"
archivo = "output.txt"

# Cargar datos ignorando la primera fila (encabezado)
data = np.loadtxt(archivo, skiprows=1)

# Extraer columnas
M = data[:, 1].astype(int)  # Número de celdas M
Time_ms = data[:, 2]  # Tiempo en milisegundos

# Verificar si hay desvío estándar en el archivo
if data.shape[1] == 4:
    Std_dev = data[:, 3]  # Desvío estándar en milisegundos
else:
    Std_dev = np.zeros_like(Time_ms)  # Si no hay desvío estándar, usar ceros

# 📊 Graficar Tiempo vs M con escala logarítmica y barras de error verticales
plt.figure(figsize=(8, 6))
plt.errorbar(M, Time_ms, yerr=Std_dev, fmt='o-', color='blue', label="Tiempo de ejecución", 
             capsize=5, capthick=1, elinewidth=1, ecolor='black')

plt.xlabel("Número de Celdas (M)")
plt.ylabel("Tiempo de ejecución (ms)")
plt.title("Tiempo de ejecución vs Número de Celdas (M) con Desvío Estándar")

plt.yscale("log")  # 🔹 Escala logarítmica en el eje Y para mejor visualización
plt.xticks(M)  # Asegurar que todas las etiquetas de M sean visibles
plt.legend()
plt.grid(True, which="both", linestyle="--", linewidth=0.5)
plt.show()
