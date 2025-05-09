import matplotlib.pyplot as plt
import numpy as np
import re

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

# CAMBIÁ ESTA RUTA POR TU ARCHIVO
file_path = "../bin/Debug/net8.0/e-4/complex-N1000-gear5-dt1e-004-k1.02e2-w6pi.txt"

# Parsear datos
steps = parse_simulation_file(file_path)

# Obtener tiempo y amplitud máxima (máximo |y|) por paso
times = []
max_amplitudes = []

for step in steps:
    max_amp = max(abs(p.position[1]) for p in step.particles)
    times.append(step.time)
    max_amplitudes.append(max_amp)

# Recortar fase transitoria (opcional)
cut = int(len(times) * 0.1)
times = times[cut:]
max_amplitudes = max_amplitudes[cut:]

# Graficar
plt.figure(figsize=(10, 6))
plt.plot(times, max_amplitudes)
plt.xlabel("Tiempo (s)", fontsize=16)
plt.ylabel("Amplitud máxima |y| (m)", fontsize=16)
plt.title("Amplitud máxima del sistema vs tiempo", fontsize=18)
plt.grid(True)
plt.tight_layout()
plt.show()
