import os
import re
import numpy as np

# Reutilizamos la lógica de parseo
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

# === Parámetros de entrada ===
k_target = "1.02e2"  # <- cambiar por el k que quieras
input_dir = "../bin/Debug/net8.0"
output_dir = os.path.join(input_dir, "amplitudes")
os.makedirs(output_dir, exist_ok=True)

# === Buscar archivos con ese k específico ===
all_files = os.listdir(input_dir)
sim_files = [f for f in all_files if f.endswith(".txt") and f"-k{k_target}" in f]

print(f"Encontrados {len(sim_files)} archivos con k = {k_target}")

# === Procesar cada archivo ===
for fname in sim_files:
    file_path = os.path.join(input_dir, fname)

    omega_match = re.search(r"-w([0-9]+)", fname)
    if not omega_match:
        print(f"No se pudo extraer omega de {fname}")
        continue
    omega_str = omega_match.group(1)
    omega_value = int(omega_str)

    # Parsear y procesar
    steps = parse_simulation_file(file_path)

    cut_low = int(len(steps) * 0.1)
    steps_cut = steps[cut_low:]
    cut_high = int(len(steps_cut) * 0.7)
    final_segment = steps_cut[cut_high:]

    max_amplitud_final = max(
        max(abs(p.position[1]) for p in step.particles) for step in final_segment
    )

    output_filename = os.path.join(output_dir, f"amplitud-w{omega_str}-k{k_target}.csv")
    with open(output_filename, "w") as f:
        f.write("omega,k,amplitud_max\n")
        f.write(f"{omega_value},{k_target},{max_amplitud_final}\n")

    print(f"[OK] Guardado: {output_filename}")
