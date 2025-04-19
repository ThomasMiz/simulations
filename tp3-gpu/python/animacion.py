import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation
import numpy as np
from matplotlib.patches import Circle

from file_loader import SimulationData

path = "../bin/Debug/net8.0/output.sim"
output_file = "animacion.mp4"

animation_speed = 0.025 # Velocidad de animación respecto a la simulación
animation_fps = 30 # Fotogramas por segundo
external_clock_period = (1.0 / animation_fps) * animation_speed

simulation = SimulationData(path)

fig, ax = plt.subplots()
ax.set_xlim(-simulation.radio_contenedor, simulation.radio_contenedor)
ax.set_ylim(-simulation.radio_contenedor, simulation.radio_contenedor)
ax.set_aspect('equal')
ax.set_facecolor('black')

# Borde del recinto circular (con espesor visual)
# Dibujar el borde del recinto circular
borde = plt.Circle((0, 0), simulation.radio_contenedor, color='white', fill=False, linewidth=0.5)
ax.add_patch(borde)

# Crear los círculos representando cada partícula con su radio real
circulos = [Circle((0, 0), radius=simulation.radios[i], color='white', linewidth=0, fill=True) for i in range(simulation.N)]
for c in circulos:
    ax.add_patch(c)

def init():
    print(f"Generando animación...")
    for c in circulos:
        c.center = (0, 0)
    return circulos

frame_count = int(simulation.steps_data[-1].time / external_clock_period)

last_frame_index = [0]
def update(frame_index):
    frame_time = frame_index * external_clock_period
    curr_frame_index = last_frame_index[0]
    while curr_frame_index + 1 < len(simulation.steps_data) and abs(simulation.steps_data[curr_frame_index].time - frame_time) >= abs(simulation.steps_data[curr_frame_index + 1].time - frame_time):
        curr_frame_index += 1

    step_data = simulation.steps_data[curr_frame_index]
    for i, (x, y, _, _) in enumerate(step_data.particles_data):
        circulos[i].center = (x, y)

    ax.set_title(f"t = {frame_time:.3f} s (paso {step_data.step_number})", color='white')

    last_frame_index[0] = curr_frame_index
    if frame_index != 0 and frame_index % 30 == 0:
        print(f"Procesado el fotograma {frame_index} de {frame_count}")
    return circulos

ani = FuncAnimation(fig, update, frames=frame_count, init_func=init, blit=True, interval=1000 / animation_fps)
ani.save(output_file, fps=animation_fps, dpi=200)
print(f"Animación guardada como {output_file}")
plt.close()
