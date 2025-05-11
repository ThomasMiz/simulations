import matplotlib.pyplot as plt
import numpy as np
import re
import os

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
file_path = "../bin/Debug/net8.0/complex-N1000-gear5-dt1e-004-k1.02e2-w2pi.txt"

# === EXTRAER ω Y k DEL NOMBRE DEL ARCHIVO ===
omega_match = re.search(r"-w([0-9]+)pi", file_path)
k_match = re.search(r"-k([\d.eE+-]+)", file_path)

omega_str = omega_match.group(1) if omega_match else "UNKNOWN"
k_str = k_match.group(1) if k_match else "UNKNOWN"

omega_value = int(omega_str) * np.pi if omega_match else None

# === PARSEAR Y PROCESAR ===
steps = parse_simulation_file(file_path)
times = [step.time for step in steps]
amplitudes = [max(abs(p.position[1]) for p in step.particles) for step in steps]

# Recortar primeros pasos (fase transitoria)
cut_low = int(len(steps) * 0.1)
steps_cut = steps[cut_low:]

# Tomar último 30% para cálculo de amplitud máxima
cut_high = int(len(steps_cut) * 0.7)
final_segment = steps_cut[cut_high:]
max_amplitud_final = max(max(abs(p.position[1]) for p in step.particles) for step in final_segment)

# === GENERAR ARCHIVO CON ω Y k EN EL NOMBRE ===
output_dir = "../bin/Debug/net8.0/amplitudes"
os.makedirs(output_dir, exist_ok=True)
output_filename = os.path.join(output_dir, f"amplitud-w{omega_str}pi-k{k_str}.csv")
with open(output_filename, "w") as f:
    f.write("omega,k,amplitud_max\n")
    f.write(f"{omega_value},{k_str},{max_amplitud_final}\n")

print(f"Amplitud máxima guardada en: {output_filename}")

# === GRAFICAR (opcional, para revisar) ===
times_cut = [step.time for step in steps_cut]
amps_cut = [max(abs(p.position[1]) for p in step.particles) for step in steps_cut]

plt.figure(figsize=(10, 6))
plt.plot(times_cut, amps_cut, label="|y| máximo por paso")
plt.axhline(max_amplitud_final, color="red", linestyle="--", label="Amplitud máxima en estado estacionario")
plt.xlabel("Tiempo (s)")
plt.ylabel("Amplitud máxima |y| (m)")
plt.title(f"Amplitud vs tiempo – ω = {omega_str}π, k = {k_str}")
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.show()
