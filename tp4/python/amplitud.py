import math

import matplotlib.pyplot as plt
import numpy as np
import re
import os

from matplotlib import ticker


class ParticleState:
    def __init__(self, pos, vel):
        self.position = pos
        self.velocity = vel

class SimulationStep:
    def __init__(self, step, time, particles):
        self.step = step
        self.time = time
        self.particles = particles

def parse_simulation_file(file_path):
    with open(file_path, 'r') as file:
        lines = [line.strip() for line in file if line.strip()]
    steps = []
    for line in lines[3:]:
        match = re.match(r'\[(\d+)\s+([\d.eE+-]+)\]\s+(.*)', line)
        if not match:
            continue
        step = int(match.group(1))
        time = float(match.group(2))
        particles_raw = match.group(3).split(';')
        particles = []
        for pdata in particles_raw:
            values = list(map(float, pdata.strip().split()))
            if len(values) != 4:
                continue
            pos = (values[0], values[1])
            vel = (values[2], values[3])
            particles.append(ParticleState(pos, vel))
        steps.append(SimulationStep(step, time, particles))
    return steps

# === CAMBIÁ ESTA RUTA POR TU ARCHIVO ===
file_path = "../bin/Debug/net8.0/k=1e4/complex-k1e4-w19.txt"

# === EXTRAER ω Y k DEL NOMBRE DEL ARCHIVO ===
omega_match = re.search(r"-w([0-9]+(?:\.[0-9]+)?)\.txt", file_path)
k_match = re.search(r"-k([0-9]+(?:\.[0-9]+)?e[+-]?[0-9]+)", file_path)

omega_str = omega_match.group(1) if omega_match else "UNKNOWN"
k_str = k_match.group(1) if k_match else "UNKNOWN"

omega_value = float(omega_str) if omega_str != "UNKNOWN" else None

# === PARSEAR Y PROCESAR ===
steps = parse_simulation_file(file_path)
times = [step.time for step in steps]
amplitudes = [max(abs(p.position[1]) for p in step.particles) for step in steps]

# Recortar primeros pasos (fase transitoria)
cut_low = int(len(steps) * 0)
steps_cut = steps[cut_low:]

# Tomar último 30% para cálculo de amplitud máxima
cut_high = int(len(steps_cut) * 0.7)
final_segment = steps_cut[cut_high:]
max_amplitud_final = max(max(abs(p.position[1]) for p in step.particles) for step in final_segment)

omega_formatted = f"{omega_value:.2f}" if omega_value is not None else "UNKNOWN"

# === GENERAR ARCHIVO CON ω Y k EN EL NOMBRE ===
output_dir = "../bin/Debug/net8.0/amplitudes"
os.makedirs(output_dir, exist_ok=True)
output_filename = os.path.join(output_dir, f"amplitud-w{omega_formatted}-k{k_str}.csv")

with open(output_filename, "w") as f:
    f.write("omega,k,amplitud_max\n")
    f.write(f"{omega_value},{k_str},{max_amplitud_final}\n")

print(f"Amplitud máxima guardada en: {output_filename}")

# === GRAFICAR (opcional, para revisar) ===
times_cut = [step.time for step in steps_cut]
amps_cut = [max(abs(p.position[1]) for p in step.particles) for step in steps_cut]

plt.figure(figsize=(10, 6))
plt.plot(times_cut, amps_cut)
plt.axhline(max_amplitud_final, color="red", linestyle="--", label="Amplitud máxima en estado estacionario")
plt.xlabel("Tiempo [s]", fontsize=20)
plt.ylabel("Amplitud máxima |y| [m]", fontsize=20)
# plt.title(f"Amplitud vs tiempo – ω = {omega_str}, k = {k_str}")

# Configurar ticks del eje x cada 1s
t_min = math.floor(min(times_cut))
t_max = math.ceil(max(times_cut))
plt.xticks(np.arange(t_min, t_max + 1, 1), fontsize=20)

# Notación científica en el eje y
formatter = ticker.ScalarFormatter(useMathText=True)
formatter.set_powerlimits((-2, 2))
plt.gca().yaxis.set_major_formatter(formatter)
plt.gca().yaxis.offsetText.set_fontsize(20)
plt.yticks(fontsize=20)

plt.legend(fontsize=20)
plt.grid(True)
plt.tight_layout()
plt.show()