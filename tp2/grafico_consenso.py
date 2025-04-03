import matplotlib.pyplot as plt

# Parámetros de detección de estacionariedad (usamos los mismos del simulador)
epsilon = 0.001
window = 10

# Leer archivo consenso.txt
steps = []
consensos = []

with open("./bin/Debug/net8.0/consenso-50-0.01.txt", "r") as f:
    for line in f:
        step, M = line.strip().split(",")
        steps.append(int(step))
        consensos.append(float(M))

# Graficar
plt.figure(figsize=(9, 5))
plt.plot(steps, consensos, label="M(t) - Consenso", linewidth=1.8)

plt.xlabel("Paso de simulación")
plt.ylabel("Consenso")
#plt.title("Evolución del Consenso en el tiempo")
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.show()
