import matplotlib.pyplot as plt
import matplotlib.animation as animation
from matplotlib.patches import Circle
from file_loader import parse_simulation_file

# === Configuration ===
FILE_PATHS = {
    "DaisyChain5": "../bin/Debug/net8.0/gravitydaisychain-N5-verlet-25000steps.txt",
    "ThreeBodyS1": "../bin/Debug/net8.0/gravity1-N3-beeman-25000steps.txt",
    "ThreeBodyS2": "../bin/Debug/net8.0/gravity7-N3-beeman-25000steps.txt",
    "ThreeBodyS3": "../bin/Debug/net8.0/gravity8-N3-beeman-25000steps.txt"
}
PARTICLE_RADIUS = 0.1

# === Load all simulations ===
simulations = {
    name: parse_simulation_file(path)
    for name, path in FILE_PATHS.items()
}

# Determine number of frames from first simulation (assumed to be synced)
num_frames = len(next(iter(simulations.values())).steps)

# === Set up the figure with one Axes per simulation ===
fig, axes = plt.subplots(
    nrows=2,
    ncols=2,
    figsize=(12, 10),
    squeeze=False
)

# === Set up each subplot ===
plots = []
ax_list = [ax for row in axes for ax in row]  # Flatten 2D axes array
for (name, sim_data), ax in zip(simulations.items(), ax_list):
    num_particles = len(sim_data.steps[0].particles)

    # Get all positions to determine bounds
    all_positions = [
        (p.position[0], p.position[1])
        for step in sim_data.steps
        for p in step.particles
    ]
    xs, ys = zip(*all_positions)
    padding = PARTICLE_RADIUS * 2

    ax.set_xlim(min(xs) - padding, max(xs) + padding)
    ax.set_ylim(min(ys) - padding, max(ys) + padding)
    ax.set_title(name)
    ax.set_aspect(aspect=1)
    ax.set_xlabel("X")
    ax.set_ylabel("Y")

    # Circles per simulation
    circles = [
        Circle((0, 0), PARTICLE_RADIUS, fc='blue', alpha=0.6)
        for _ in range(num_particles)
    ]
    for c in circles:
        ax.add_patch(c)

    plots.append((sim_data, circles, ax))

# === Update function ===
def update(frame):
    artists = []
    for sim_data, circles, ax in plots:
        step = sim_data.steps[frame]
        for i, particle in enumerate(step.particles):
            x, y = particle.position
            circles[i].center = (x, y)
        ax.set_title(f"{ax.get_title().split()[0]} – Time: {step.time:.3f}s")
        artists.extend(circles)
    return artists

# === Animate ===
ani = animation.FuncAnimation(
    fig, update, frames=num_frames, interval=30, blit=True
)

# Save as MP4
print("Saving as mp4...")
from matplotlib.animation import FFMpegWriter
ani.save("particles.mp4", writer=FFMpegWriter(fps=30, metadata=dict(artist='el tuki'), bitrate=1800))

# Show or save
print("Plotting...")
plt.tight_layout()
plt.show()
