import matplotlib.pyplot as plt

# Parámetros de detección de estacionariedad (usamos los mismos del simulador)
epsilon = 0.001
window = 10

# Leer archivo consenso.txt
steps = []
consensos = []

with open("./bin/Debug/net8.0/consenso.txt", "r") as f:
    for line in f:
        step, M = line.strip().split(",")
        steps.append(int(step))
        consensos.append(float(M))

# Buscar el paso en que se alcanza el estado estacionario
estacionario_step = None
for i in range(len(consensos) - window):
    ventana = consensos[i:i + window]
    if max(ventana) - min(ventana) < epsilon:
        estacionario_step = steps[i + window - 1]
        break

# Graficar
plt.figure(figsize=(9, 5))
plt.plot(steps, consensos, label="M(t) - Consenso", linewidth=1.8)

# Línea vertical si se detectó estacionariedad
if estacionario_step is not None:
    plt.axvline(x=estacionario_step, color='red', linestyle='--', label=f"Estacionario en paso {estacionario_step}")

plt.xlabel("Paso de simulación")
plt.ylabel("Consenso M(t)")
plt.title("Evolución del Consenso en el tiempo")
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.show()
