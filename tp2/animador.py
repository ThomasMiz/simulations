import numpy as np
import matplotlib.pyplot as plt
import matplotlib.animation as animation

# Parámetros de la grilla
N = 50                    # Tamaño de la grilla (debe coincidir con el usado en la simulación)
input_file = "output.txt"  # Archivo generado por simulador.py

# Leer las grillas desde el archivo
grids = []
with open(input_file, "r") as f:
    for line in f:
        flat = np.array(list(map(int, line.strip().split())))  # Usa split() para manejar espacios
        grid = flat.reshape((N, N))
        grids.append(grid)

# Crear la animación
fig, ax = plt.subplots()
im = ax.imshow(grids[0], cmap='gray', vmin=-1, vmax=1)  # Ajuste de colores para {-1,1}

def update(frame):
    im.set_data(grids[frame])
    ax.set_title(f"Paso {frame}")
    return [im]

ani = animation.FuncAnimation(fig, update, frames=len(grids), interval=100, blit=True)

plt.tight_layout()
plt.show()
