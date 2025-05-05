import matplotlib.pyplot as plt
import matplotlib.animation as animation
from matplotlib.patches import Circle
from file_loader import parse_simulation_file

# === Configuration ===
FILE_PATH = "../bin/Debug/net8.0/complex-N200-verlet-1001steps.txt"
PARTICLE_RADIUS = 0.0005  # Adjust as needed

# === Load simulation data ===
sim_data = parse_simulation_file(FILE_PATH)
num_particles = len(sim_data.steps[0].particles)
num_frames = len(sim_data.steps)

# === Setup figure ===
fig, ax = plt.subplots()
ax.set_aspect('equal')
ax.set_title("Particle Simulation")
ax.set_xlabel("X")
ax.set_ylabel("Y")

# Determine bounds from all positions to set plot limits
all_positions = [
    (p.position[0], p.position[1])
    for step in sim_data.steps
    for p in step.particles
]
xs, ys = zip(*all_positions)
padding = PARTICLE_RADIUS * 2
ax.set_xlim(min(xs) - padding, max(xs) + padding)
ax.set_ylim(min(ys) - padding, max(ys) + padding)

# === Create circle objects for each particle ===
circles = [
    Circle((0, 0), PARTICLE_RADIUS, fc='blue', alpha=0.6)
    for _ in range(num_particles)
]
for circle in circles:
    ax.add_patch(circle)

# === Animation update function ===
def update(frame):
    step = sim_data.steps[frame]
    for i, particle in enumerate(step.particles):
        x, y = particle.position
        circles[i].center = (x, y)
    ax.set_title(f"Time: {step.time:.3f}s")
    return circles

# === Animate ===
ani = animation.FuncAnimation(
    fig, update, frames=num_frames, interval=30, blit=True
)

# Save as GIF
#from matplotlib.animation import PillowWriter
#ani.save("particles.gif", writer=PillowWriter(fps=30))

# Save as MP4
from matplotlib.animation import FFMpegWriter
ani.save("particles.mp4", writer=FFMpegWriter(fps=30, metadata=dict(artist='el tuki'), bitrate=1800))

# Plot
plt.tight_layout()
plt.show()
