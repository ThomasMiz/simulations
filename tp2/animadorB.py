import numpy as np
import matplotlib.pyplot as plt
import matplotlib.animation as animation
import glob
import re
import os

# Parámetros
N = 50  # Tamaño de la grilla
max_duration_secs = 15
skip_frames = 1

# Buscar archivos
file_pattern = "./bin/Debug/net8.0/output-b-*.txt"
files = glob.glob(file_pattern)
files.sort(key=lambda x: float(re.search(r"output-b-(\d+\.\d+)\.txt", x).group(1)))

# Mostrar una animación por archivo
for i, file in enumerate(files):
    print(f"▶️ Mostrando animación {i+1} de {len(files)}: {os.path.basename(file)}")

    grids = []
    with open(file, "r") as f:
        for line in f:
            flat = np.array(list(map(int, line.strip().split())))
            grid = flat.reshape((N, N))
            grids.append(grid)

    num_frames = len(grids)
    if num_frames == 0:
        print(f"⚠️  {file} está vacío, se salta.")
        continue

    # Calcular duración por frame
    interval = int((max_duration_secs * 1000) / num_frames)
    interval = max(interval, 1)

    # Crear y mostrar animación
    fig, ax = plt.subplots()
    im = ax.imshow(grids[0], cmap='gray', vmin=-1, vmax=1)
    ax.set_title(f"{os.path.basename(file)} - Paso 0")

    def update(frame):
        im.set_data(grids[frame])
        ax.set_title(f"{os.path.basename(file)} - Paso {frame}")
        return [im]

    ani = animation.FuncAnimation(fig, update, frames=num_frames, interval=interval, blit=True)
    plt.tight_layout()
    plt.show()
