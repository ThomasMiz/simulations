import glob
import re
import matplotlib.pyplot as plt

from_step = 0
to_step = None

# Leer archivo consenso-*.txt
steps_map = {}
consensos_map = {}

# Filtro de qué archivos levantar
#p_filter = None
#n = "*"
n=50
p_filter = ["0.01", "0.1", "0.9"]

allfiles = glob.glob(f"./bin/Debug/net8.0-2/consenso-{n}-*.txt")
allfiles.sort()
for file in allfiles:
    searchy = re.search(r"consenso-(\d+)-(\d+(\.\d+)?)\.txt", file)
    n = searchy.group(1)
    p = searchy.group(2)
    if p_filter is not None and not p in p_filter:
        continue
    print(f"Picked up file {file} with N={n} and p={p}")
    with open(file, "r") as f:
        steps = []
        consensos = []
        step_index = 0
        for line in f:
            if step_index >= from_step and (to_step is None or step_index <= to_step):
                step, M = line.strip().split(",")
                steps.append(int(step))
                consensos.append(float(M))
            step_index += 1
        steps_map[p] = steps
        consensos_map[p] = consensos

# Graficar
plt.figure(figsize=(9, 5))

for p, steps in steps_map.items():
    print(f"plotting {p}")
    plt.plot(steps, consensos_map[p], label=f"p={p}", linewidth=0.7)

plt.xlabel("Paso de simulación")
plt.ylabel("Consenso")
#plt.title("Evolución del Consenso en el tiempo")
plt.legend(fontsize=12, loc="upper right", bbox_to_anchor=(1.0, 0.9))
plt.grid(True)
plt.tight_layout()
plt.show()
