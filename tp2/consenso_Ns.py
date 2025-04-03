import os
import numpy as np
import matplotlib.pyplot as plt

valores_N = [25, 50, 75, 100]
valores_p = [0.01, 0.012, 0.015, 0.04, 0.06, 0.08, 0.085, 0.09, 0.1, 0.11, 0.115, 0.12, 0.13, 0.2, 0.3, 0.5, 0.9]
max_steps = 40000
ultimos = 10000

resultados = {N: [] for N in valores_N}

for N in valores_N:
    for p in valores_p:
        filename = f"./bin/Debug/net8.0/consenso-{N}-{p}.txt"
        try:
            data = np.genfromtxt(filename, delimiter=',', comments='#')
            if data.ndim < 2 or data.shape[1] < 2:
                raise ValueError("El archivo no tiene dos columnas completas")

            data = data[~np.isnan(data).any(axis=1)]  # eliminar filas inválidas
            M_t = data[:, 1]
            if len(M_t) < ultimos:
                raise ValueError("No hay suficientes datos para promedio estacionario")

            estacionario = M_t[-ultimos:]
            M_prom = np.mean(estacionario)
            M_std = np.std(estacionario)  # ⚠️ Acá calculamos la desviación estándar
            resultados[N].append((p, M_prom, M_std))

        except Exception as e:
            print(f"[AVISO] No se pudo procesar {filename}: {e}")

# Graficar con barras de error
plt.figure(figsize=(10, 6))

for N in valores_N:
    datos = sorted(resultados[N], key=lambda x: x[0])
    ps = [x[0] for x in datos]
    Ms = [x[1] for x in datos]
    errores = [x[2] for x in datos]
    plt.errorbar(ps, Ms, yerr=errores, fmt='-o', capsize=4, label=f'N = {N}')

plt.xlabel('p')
plt.ylabel('Promedio de consenso en estacionario')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.savefig("consenso_vs_p.png", dpi=300)

plt.show()
