import numpy as np
import matplotlib.pyplot as plt
import matplotlib.animation as animation

# Los índices de los fotogramas a mostrar (inclusive ambos)
from_frame = 4800
to_frame = 4920

# No dibujar todos los fotogramas, mostrar por ej uno de cada 10 (desactivar con skip_frames=1)
skip_frames = 1

# Tiempo entre fotogramas
interval = 1

# Parámetros de la grilla
N = 50                    # Tamaño de la grilla (debe coincidir con el usado en la simulación)
input_file = "./bin/Debug/net8.0/output.txt"  # Archivo generado por simulador.py

# Leer las grillas desde el archivo
grids = []
with open(input_file, "r") as f:
    frame_index = 0
    for line in f:
        if frame_index >= from_frame and (to_frame is None or frame_index <= to_frame) and (frame_index - from_frame) % skip_frames == 0:
            flat = np.array(list(map(int, line.strip().split())))  # Usa split() para manejar espacios
            grid = flat.reshape((N, N))
            grids.append((frame_index, grid))
        frame_index += 1

# Crear la animación
fig, ax = plt.subplots()
im = ax.imshow(grids[0][1], cmap='gray', vmin=-1, vmax=1)  # Ajuste de colores para {-1,1}

def update(frame):
    im.set_data(grids[frame][1])
    ax.set_title(f"Paso {grids[frame][0]}")
    return [im]

ani = animation.FuncAnimation(fig, update, frames=len(grids), interval=interval, blit=True)

plt.tight_layout()
plt.show()
