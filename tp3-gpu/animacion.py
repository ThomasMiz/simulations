import struct
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation
import numpy as np
from matplotlib.patches import Circle
import os

path = "./bin/Debug/net8.0/output.sim"
output_file = "animacion.mp4"

# ========= LECTURA DE ARCHIVO BINARIO =========
def leer_output_binario(path):
    with open(path, 'rb') as f:
        radio_contenedor = struct.unpack('f', f.read(4))[0]
        N = struct.unpack('i', f.read(4))[0]
        masas_radios = [struct.unpack('ff', f.read(8)) for _ in range(N)]

        data = []
        while True:
            step_bytes = f.read(4)
            if not step_bytes:
                break
            step = struct.unpack('i', step_bytes)[0]
            t = struct.unpack('f', f.read(4))[0]
            particulas = [struct.unpack('ffff', f.read(16)) for _ in range(N)]
            data.append((t, particulas))
    return radio_contenedor, N, masas_radios, data

# ========= INTERPOLACIÓN DE ESTADOS =========
def generar_frames_interpolados(data, fps=30):
    interval = 1.0 / fps
    frames = []

    for i in range(len(data) - 1):
        t0, estado0 = data[i]
        t1, estado1 = data[i + 1]
        dur = t1 - t0
        num_frames = max(1, int(np.round(dur / interval)))

        for j in range(num_frames):
            t_interp = t0 + j * interval
            alpha = (t_interp - t0) / (t1 - t0)

            interpoladas = []
            for p0, p1 in zip(estado0, estado1):
                x0, y0, vx0, vy0 = p0
                # Velocidades se mantienen constantes entre eventos
                x_interp = x0 + vx0 * (t_interp - t0)
                y_interp = y0 + vy0 * (t_interp - t0)
                interpoladas.append((x_interp, y_interp))

            frames.append((t_interp, interpoladas))

    # Agregar último frame exactamente en t final
    t_last, estado_last = data[-1]
    posiciones_finales = [(x, y) for (x, y, vx, vy) in estado_last]
    frames.append((t_last, posiciones_finales))
    return frames

# ========= ANIMACION =========
def animar_interpolado(N, data, radio_contenedor):
    interpolados = generar_frames_interpolados(data)

    fig, ax = plt.subplots()
    ax.set_xlim(-radio_contenedor, radio_contenedor)
    ax.set_ylim(-radio_contenedor, radio_contenedor)
    ax.set_aspect('equal')
    ax.set_facecolor('black')

    # Borde del recinto circular (con espesor visual)
    # Dibujar el borde del recinto circular
    borde = plt.Circle((0, 0), radio_contenedor, color='white', fill=False, linewidth=0.5)
    ax.add_patch(borde)

    # Obstáculo central
    #obstaculo = Circle((L/2, L/2), obstaculo_radio, color='red', fill=True)
    #ax.add_patch(obstaculo)

    # Crear los círculos representando cada partícula con su radio real
    circulos = [Circle((0, 0), radius=masas_radios[i][1], color='white', linewidth=0, fill=True) for i in range(N)]
    for c in circulos:
        ax.add_patch(c)

    def init():
        for c in circulos:
            c.center = (0, 0)
        return circulos

    def update(frame_index):
        t, pos = interpolados[frame_index]
        for i, (x, y) in enumerate(pos):
            circulos[i].center = (x, y)
        ax.set_title(f"t = {t:.3f} s", color='white')
        return circulos
    
    ani = FuncAnimation(fig, update, frames=len(interpolados), init_func=init, blit=True, interval=1000 / 30)
    ani.save(output_file, fps=30, dpi=200)
    print(f"Animación guardada como {output_file}")
    plt.close()

# ========= EJECUCION =========
if not os.path.exists(path):
    print(f"Archivo '{path}' no encontrado.")
else:
    radio_contenedor, N, masas_radios, data = leer_output_binario(path)
    animar_interpolado(N, data, radio_contenedor)
