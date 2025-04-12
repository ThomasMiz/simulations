import struct
import numpy as np
import matplotlib.pyplot as plt
import pandas as pd

path = "./bin/Debug/net8.0/output.sim"

def leer_colisiones_bin(path):
    with open(path, 'rb') as f:
        radio_contenedor = struct.unpack('f', f.read(4))[0]
        N = struct.unpack('i', f.read(4))[0]
        masas_radios = [struct.unpack('ff', f.read(8)) for _ in range(N)]
        datos = []

        while True:
            step_bytes = f.read(4)
            if not step_bytes:
                break
            step = struct.unpack('i', step_bytes)[0]
            t = struct.unpack('f', f.read(4))[0]
            particulas = [struct.unpack('ffff', f.read(16)) for _ in range(N)]
            datos.append((t, particulas))
    return radio_contenedor, N, masas_radios, datos

def calcular_presion(radio_contenedor, datos, masas_radios):
    perimetro = 2 * np.pi * radio_contenedor
    m = masas_radios[0][0]

    tiempos = []
    presiones = []

    for i in range(1, len(datos)):
        t_anterior, estado_anterior = datos[i - 1]
        t_actual, estado_actual = datos[i]
        dt = t_actual - t_anterior
        impulso_total_evento = 0.0

        for j in range(len(estado_actual)):
            vx0, vy0 = estado_anterior[j][2], estado_anterior[j][3]
            vx1, vy1 = estado_actual[j][2], estado_actual[j][3]

            if not np.allclose([vx0, vy0], [vx1, vy1], atol=1e-8):
                delta_vx = vx1 - vx0
                delta_vy = vy1 - vy0
                delta_p = m * np.sqrt(delta_vx**2 + delta_vy**2)
                impulso_total_evento += delta_p

        presion_inst = impulso_total_evento / (dt * perimetro)
        tiempos.append(t_actual)
        presiones.append(presion_inst)

    return tiempos, presiones

radio_contenedor, N, masas_radios, datos = leer_colisiones_bin(path)
tiempos, presiones = calcular_presion(radio_contenedor, datos, masas_radios)

df = pd.DataFrame({
    "Tiempo [s]": tiempos,
    "Presión [N/m]": presiones
})
print(df.head(10))

plt.figure(figsize=(8, 5))
plt.plot(tiempos, presiones, color='blue')
plt.xlabel("Tiempo [s]")
plt.ylabel("Presión instantánea [N/m]")
plt.title("Presión sobre el borde del recinto en función del tiempo")
plt.grid(True)
plt.tight_layout()
plt.savefig("grafico_presion.png")
plt.show()
