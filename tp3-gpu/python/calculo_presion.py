import struct
import numpy as np
import matplotlib.pyplot as plt

file_path = '../output.sim'

with open(file_path, 'rb') as f:
    data = f.read()

offset = 0

# Leer containerRadius (float) y N (uint)
container_radius = struct.unpack_from('f', data, offset)[0]
offset += 4
N = struct.unpack_from('I', data, offset)[0]
offset += 4

# Leer masa y radio de cada partícula
masas = []
radios = []
for _ in range(N):
    masa, radio = struct.unpack_from('ff', data, offset)
    masas.append(masa)
    radios.append(radio)
    offset += 8

# Constantes geométricas
perimetro_ext = 2 * np.pi * container_radius
perimetro_obs = 2 * np.pi * 0.005

# Configuración
ventana = 0.005  # 0.5 ms
t_actual = 0.0

tiempos = []
presion_ext = []
presion_obs = []

vel_prev = np.zeros((N, 2))
prev_time = None
acum_impulso_ext = 0.0
acum_impulso_obs = 0.0

# Leer los pasos de simulación
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
            presion_obs.append(acum_impulso_obs / (ventana * perimetro_obs))
            tiempos.append(t_actual + ventana / 2)
            t_actual += ventana
            acum_impulso_ext = 0.0
            acum_impulso_obs = 0.0

        for i in range(N):
            if np.sign(vel_prev[i][0]) != np.sign(velocidades[i][0]) or np.sign(vel_prev[i][1]) != np.sign(velocidades[i][1]):
                dist = np.linalg.norm(posiciones[i])
                if abs(dist - container_radius) < 2 * radios[i]:
                    v_normal = np.dot(vel_prev[i], posiciones[i] / dist)
                    acum_impulso_ext += 2 * masas[i] * abs(v_normal)
                elif abs(dist - 0.005) < 2 * radios[i]:
                    v_normal = np.dot(vel_prev[i], posiciones[i] / dist)
                    acum_impulso_obs += 2 * masas[i] * abs(v_normal)

    vel_prev = velocidades.copy()
    prev_time = tiempo

# Rellenar ventanas pendientes al final
while len(tiempos) > 0 and t_actual < prev_time:
    presion_ext.append(acum_impulso_ext / (ventana * perimetro_ext))
    presion_obs.append(acum_impulso_obs / (ventana * perimetro_obs))
    tiempos.append(t_actual + ventana / 2)
    t_actual += ventana
    acum_impulso_ext = 0.0
    acum_impulso_obs = 0.0

# Suavizado con promedio móvil (rolling mean)
def suavizar(lista, ventana=5):
    return np.convolve(lista, np.ones(ventana)/ventana, mode='same')

presion_ext_suave = suavizar(presion_ext, ventana=5)
presion_obs_suave = suavizar(presion_obs, ventana=5)

# Graficar
plt.figure(figsize=(12, 5))
plt.plot(tiempos, presion_ext_suave, label='Presión Pared')
plt.plot(tiempos, presion_obs_suave, label='Presión Obstáculo')
plt.xlabel('Tiempo [s]')
plt.ylabel('Presión [N/m]')
plt.title('Presión en función del tiempo (suavizada)')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.show()
