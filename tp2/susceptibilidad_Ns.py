import os
import numpy as np
import matplotlib.pyplot as plt

# Parámetros del experimento
valores_N = [25, 50, 75, 100]
valores_p = [0.01, 0.012, 0.015, 0.04, 0.06, 0.08, 0.085, 0.09, 0.1, 0.11, 0.115, 0.12, 0.13, 0.2, 0.3, 0.5, 0.9]
max_steps = 40000
ultimos = 10000

# Diccionario para guardar χ para cada N
susceptibilidad = {N: [] for N in valores_N}

# Procesamiento de archivos
for N in valores_N:
    for p in valores_p:
        filename = f"./bin/Debug/net8.0/consenso-{N}-{p}.txt"
        try:
            data = np.genfromtxt(filename, delimiter=',', comments='#')
            if data.ndim < 2 or data.shape[1] < 2:
                raise ValueError("El archivo no tiene dos columnas completas")

            data = data[~np.isnan(data).any(axis=1)]  # eliminar filas con NaN
            M_t = data[:, 1]

            if len(M_t) < ultimos:
                raise ValueError("No hay suficientes datos para calcular el promedio estacionario")

            estacionario = M_t[-ultimos:]
            M_prom = np.mean(estacionario)
            M2_prom = np.mean(estacionario ** 2)
            chi = N**2 * (M2_prom - M_prom**2)

            susceptibilidad[N].append((p, chi))

        except Exception as e:
            print(f"[AVISO] No se pudo procesar {filename}: {e}")

# Graficar resultados
plt.figure(figsize=(8, 5))

for N in valores_N:
    datos = sorted(susceptibilidad[N], key=lambda x: x[0])
    ps = [x[0] for x in datos]
    chis = [x[1] for x in datos]
    plt.plot(ps, chis, marker='o', label=f'N = {N}')

plt.xlabel('p')
plt.ylabel('Susceptibilidad')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.savefig("susceptibilidad_vs_p.png", dpi=300)
plt.show()
