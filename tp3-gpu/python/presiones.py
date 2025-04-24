import struct
import numpy as np
import matplotlib.pyplot as plt

# Parámetros
ventana = 0.01  # segundos

# Archivos a analizar
archivos = {
    1: "../outputs/output1-fixedobstacle-250particles-vel1-50ksteps.sim",
    3: "../outputs/output1-fixedobstacle-250particles-vel3-50ksteps.sim",
    6: "../outputs/output1-fixedobstacle-250particles-vel6-50ksteps.sim",
    10: "../outputs/output1-fixedobstacle-250particles-vel10-50ksteps.sim"
}

def calcular_presion_pared(path):
    with open(path, 'rb') as f:
        data = f.read()

    offset = 0
    container_radius = struct.unpack_from('f', data, offset)[0]
    offset += 4
    N = struct.unpack_from('I', data, offset)[0]
    offset += 4

    masas = []
    radios = []
    for _ in range(N):
        masa, radio = struct.unpack_from('ff', data, offset)
        masas.append(masa)
        radios.append(radio)
        offset += 8

    perimetro_ext = 2 * np.pi * container_radius

    vel_prev = np.zeros((N, 2))
    prev_time = None
    acum_impulso_ext = 0.0
    t_actual = 0.0

    tiempos = []
    presion_ext = []

    while offset + 8 + 16 * N <= len(data):
        step, tiempo = struct.unpack_from('If', data, offset)
        offset += 8

        posiciones = []
        velocidades = []
        for i in range(N):
            px, py, vx, vy = struct.unpack_from('ffff', data, offset)
            posiciones.append((px, py))
            velocidades.append((vx, vy))
            offset += 16

        posiciones = np.array(posiciones)
        velocidades = np.array(velocidades)

        if prev_time is not None:
            while tiempo >= t_actual + ventana:
                presion_ext.append(acum_impulso_ext / (ventana * perimetro_ext))
                tiempos.append(t_actual + ventana / 2)
                t_actual += ventana
                acum_impulso_ext = 0.0

            for i in range(N):
                if np.sign(vel_prev[i][0]) != np.sign(velocidades[i][0]) or np.sign(vel_prev[i][1]) != np.sign(velocidades[i][1]):
                    dist = np.linalg.norm(posiciones[i])
                    if abs(dist - container_radius) < 2 * radios[i]:
                        v_normal = np.dot(vel_prev[i], posiciones[i] / dist)
                        acum_impulso_ext += 2 * masas[i] * abs(v_normal)

        vel_prev = velocidades.copy()
        prev_time = tiempo

    return tiempos, presion_ext

# Plot
plt.figure(figsize=(12, 6))

for v0, path in archivos.items():
    try:
        tiempos, presiones = calcular_presion_pared(path)
        if len(presiones) == 0:
            print(f"[!] No se pudieron calcular presiones para v0={v0}")
            continue

        plt.plot(tiempos, presiones, label=f'v₀ = {v0}')
    except FileNotFoundError:
        print(f"[X] Archivo no encontrado para v0={v0}: {path}")

plt.xlabel('Tiempo [s]')
plt.ylabel('Presión sobre la pared [N/m]')
plt.title('Comparación de presión contra la pared para distintas velocidades iniciales')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.show()
