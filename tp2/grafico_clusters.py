import matplotlib.pyplot as plt

# Parámetros de detección de estacionariedad (usamos los mismos del simulador)
epsilon = 0.001
window = 10

# Leer archivo consenso.txt
steps = []
cluster_counts = []
cluster_mins = []
cluster_maxs = []
cluster_avgs = []

with open("./bin/Debug/net8.0/clusterstats.txt", "r") as f:
    for line in f:
        step, count, cmin, cmax, cavg = line.strip().split(",")
        steps.append(int(step))
        cluster_counts.append(float(count))
        cluster_mins.append(float(cmin))
        cluster_maxs.append(float(cmax))
        cluster_avgs.append(float(cavg))

# Graficar
plt.figure(figsize=(9, 5))
plt.plot(steps, cluster_counts, label="Cantidad de clusters", linewidth=1.8)
plt.plot(steps, cluster_mins, label="Tamaño mínimo", linewidth=1.8)
plt.plot(steps, cluster_maxs, label="Tamaño máximo", linewidth=1.8)
plt.plot(steps, cluster_avgs, label="Tamaño promedio", linewidth=1.8)

plt.xlabel("Paso de simulación")
# plt.ylabel("tuki")
plt.title("Evolución de los clústeres en el tiempo")
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.show()
