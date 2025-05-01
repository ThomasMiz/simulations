import matplotlib.pyplot as plt
from file_loader import parse_simulation_file

output_file = "../bin/Debug/net8.0/output.txt"

sim_data = parse_simulation_file(output_file)

times = []
x_positions = []

for step in sim_data.steps:
    times.append(step.time)
    x_positions.append(step.particles[0].position[0])  # Assuming one particle

plt.figure(figsize=(8, 5))
plt.plot(times, x_positions, linestyle='--')
plt.title('Particle X Position Over Time')
plt.xlabel('Time')
plt.ylabel('X Position')
plt.grid(True)
plt.tight_layout()
plt.show()
