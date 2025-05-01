import matplotlib.pyplot as plt
from file_loader import parse_simulation_file
import numpy as np

# Map of simulation method names to their corresponding output files
output_files = {
    "Verlet": "../bin/Debug/net8.0/output-1-verlet-501steps.txt",
    "Beeman": "../bin/Debug/net8.0/output-1-beeman-501steps.txt"
}

# Parameters for analytical solution
A = 1.0
m = 70.0
k = 10000
gamma = 100.0

# Time range will be determined from the first simulation file
time_range = []

plt.figure(figsize=(10, 6))

# Plot simulation data
for method, file_path in output_files.items():
    sim_data = parse_simulation_file(file_path)
    times = [step.time for step in sim_data.steps]
    x_positions = [step.particles[0].position[0] for step in sim_data.steps]
    plt.plot(times, x_positions, linestyle='--', linewidth=2, label=method)
    if not time_range:
        time_range = times

# Plot analytical solution
omega_squared = k / m - (gamma ** 2) / (4 * m ** 2)
omega = np.sqrt(omega_squared)
t = np.array(time_range)
r = A * np.exp(-gamma / (2 * m) * t) * np.cos(omega * t)
plt.plot(t, r, label='Analytical', color='black', linewidth=1)

# Final plot settings
plt.title('Particle X Position Over Time')
plt.xlabel('Time')
plt.ylabel('X Position')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.show()
