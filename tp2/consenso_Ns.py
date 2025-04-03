import os
import numpy as np
import matplotlib.pyplot as plt

valores_N = [25, 50, 75, 100]
valores_p = [0.015, 0.04, 0.07, 0.08, 0.09, 0.1, 0.011, 0.12, 0.13, 0.2, 0.3, 0.5, 0.9]
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
            resultados[N].append((p, M_prom))

        except Exception as e:
            print(f"[AVISO] No se pudo procesar {filename}: {e}")

# Graficar (igual que antes)
plt.figure(figsize=(8, 5))

for N in valores_N:
    datos = sorted(resultados[N], key=lambda x: x[0])
    ps = [x[0] for x in datos]
    Ms = [x[1] for x in datos]
    plt.plot(ps, Ms, marker='o', label=f'N = {N}')

plt.xlabel('Probabilidad p')
plt.ylabel('⟨M⟩ en estado estacionario')
plt.title('Promedio del consenso ⟨M⟩ vs. p para distintos N')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.show()
