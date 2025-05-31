# file_loader.py

import re
from dataclasses import dataclass
from typing import List

@dataclass
class ParticleState:
    position: tuple[float, float]
    velocity: tuple[float, float]

@dataclass
class SimulationStep:
    step: int
    time: float
    particles: List[ParticleState]

@dataclass
class SimulationData:
    integration_type: str
    delta_time: float
    masses: List[float]
    steps: List[SimulationStep]

def parse_simulation_file(file_path: str) -> SimulationData:
    with open(file_path, 'r') as file:
        lines = [line.strip() for line in file if line.strip()]

    integration_type = lines[0].split(':', 1)[1].strip()
    delta_time = float(lines[1].split(':', 1)[1].strip())
    masses = eval(lines[2].split(':', 1)[1].strip())

    step_lines = lines[3:]
    steps = []

    for line in step_lines:
        match = re.match(r'\[(\d+)\s+([\d.]+)\]\s+(.*)', line)
        if not match:
            continue
        step_number = int(match.group(1))
        time = float(match.group(2))
        particle_data = match.group(3).split(';')

        particles = []
        for pdata in particle_data:
            values = list(map(float, pdata.strip().split()))
            if len(values) != 4:
                continue
            pos = (values[0], values[1])
            vel = (values[2], values[3])
            particles.append(ParticleState(position=pos, velocity=vel))

        steps.append(SimulationStep(step=step_number, time=time, particles=particles))

    return SimulationData(
        integration_type=integration_type,
        delta_time=delta_time,
        masses=masses,
        steps=steps
    )
