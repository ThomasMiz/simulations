import glob
import re
import matplotlib.pyplot as plt

# Parámetros de detección de estacionariedad (usamos los mismos del simulador)
epsilon = 0.001
window = 10

# Leer archivo consenso-*.txt
steps_map = {}
consensos_map = {}

for file in glob.glob("./bin/Debug/net8.0/consenso-*.txt"):
    print(f"Picked up file {file}")
    p = re.search(r"consenso-(\d+\.\d+)\.txt", file).group(1)
    with open(file, "r") as f:
        steps = []
        consensos = []
        for line in f:
            step, M = line.strip().split(",")
            steps.append(int(step))
            consensos.append(float(M))
        steps_map[p] = steps
        consensos_map[p] = consensos

# Graficar
plt.figure(figsize=(9, 5))

for p, steps in steps_map.items():
    print(f"plotting {p}")
    plt.plot(steps, consensos_map[p], label=f"p={p}", linewidth=0.7)

plt.xlabel("Paso de simulación")
plt.ylabel("Consenso M(t)")
plt.title("Evolución del Consenso en el tiempo")
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.show()
