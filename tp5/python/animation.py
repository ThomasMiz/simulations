import matplotlib.pyplot as plt
import matplotlib.patches as patches
import matplotlib.animation as animation
import json
import re

# Dimensiones del pasillo según tu config
HALLWAY_LENGTH = 16
HALLWAY_WIDTH = 3.6

# Colores para cada dirección
COLOR_LEFT = "#228be6"  # azul
COLOR_RIGHT = "#fa5252"  # rojo

def parse_output(filename):
    frames = []
    times = []
    with open(filename, "r") as f:
        for line in f:
            # Solo líneas con partículas (las que tienen ';')
            if ";" in line:
                state = []
                # Ejemplo de línea:
                # {"step": 101, "time": 0.101} ; {"id": 1, ...} ; {"id": 2, ...}
                step_data, *particle_data = line.strip().split(" ; ")
                
                # Extraer información del tiempo
                step_info = json.loads(step_data.strip())
                times.append(step_info.get("time", 0))
                
                # Cada partícula es un JSON
                for pdata in particle_data:
                    pdata = pdata.strip()
                    if pdata.endswith(","):
                        pdata = pdata[:-1]
                    p = json.loads(pdata)
                    state.append(p)
                frames.append(state)
    return frames, times

frames, times = parse_output("../bin/Debug/net8.0/output-simple-beeman.txt")

fig, ax = plt.subplots(figsize=(12, 4))
ax.set_xlim(0, HALLWAY_LENGTH)
ax.set_ylim(-0.5, HALLWAY_WIDTH + 1)  # Más espacio para el texto
ax.set_aspect('equal')

# Dibuja las paredes del pasillo
ax.add_patch(patches.Rectangle((0, 0), HALLWAY_LENGTH, HALLWAY_WIDTH, fill=False, linewidth=2))

# Prepara los objetos para las partículas (círculos)
max_particles = max(len(f) for f in frames)
circles = []
for _ in range(max_particles):
    circ = plt.Circle((0, 0), 0.25, color='gray', alpha=1)
    circles.append(circ)
    ax.add_patch(circ)

# Agregar texto para mostrar el tiempo en la esquina superior izquierda
time_text = ax.text(0.2, HALLWAY_WIDTH + 0.5, '', fontsize=16, ha='left', weight='bold', 
                   bbox=dict(boxstyle="round,pad=0.3", facecolor="white", alpha=0.8))

def update(frame_idx):
    frame = frames[frame_idx]
    current_time = times[frame_idx]
    
    # Actualizar el texto del tiempo
    time_text.set_text(f'Tiempo: {current_time:.3f} s')
    
    # Oculta todos
    for c in circles:
        c.set_visible(False)
    for i, p in enumerate(frame):
        circles[i].center = (p['x'], p['y'])
        # El color según el nombre
        if "Left" in p['name']:
            circles[i].set_color(COLOR_LEFT)
        else:
            circles[i].set_color(COLOR_RIGHT)
        circles[i].set_visible(True)
    return circles + [time_text]

ani = animation.FuncAnimation(fig, update, frames=len(frames), interval=40, blit=True)

plt.show()
