import struct
import numpy as np
import matplotlib.pyplot as plt
import glob

from matplotlib.ticker import StrMethodFormatter

# Archivos generados para distintas velocidades
archivos = {
    1: "./outputs-fixedobstacle/output1-fixedobstacle-250particles-vel1-50ksteps.sim",
    3: "./outputs-fixedobstacle/output1-fixedobstacle-250particles-vel3-50ksteps.sim",
    6: "./outputs-fixedobstacle/output1-fixedobstacle-250particles-vel6-50ksteps.sim",
    10: "./outputs-fixedobstacle/output1-fixedobstacle-250particles-vel10-50ksteps.sim"
}

# Parámetros
ventana = 0.01  # intervalo de tiempo en segundos

temperaturas = []
presiones = []

for v0, path in archivos.items():
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
    presion_ext = []
    tiempos = []

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

    # Calcular presión promedio en equilibrio (últimos valores)
    # Tomar el último 90% de los datos para calcular el promedio
    cantidad = int(len(presion_ext) * 0.9)
    if cantidad > 0:
        promedio = np.mean(presion_ext[-cantidad:])
        presiones.append(promedio)
        temperaturas.append(v0 ** 2)  # T ~ v0²
    else:
        print(f"[!] No hay suficientes datos en {path}")


plt.figure(figsize=(15, 6))
plt.plot(temperaturas, presiones, marker='o')

plt.xlabel('Temperatura relativa [v₀²]', fontsize=20)
plt.ylabel('Presión promedio [N/m]',    fontsize=20)

plt.gca().yaxis.set_major_formatter(StrMethodFormatter('{x:,.0f}'))

plt.xticks(fontsize=20)
plt.yticks(fontsize=20)
plt.grid(True)
plt.tight_layout()
plt.show()