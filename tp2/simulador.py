import numpy as np

# Parámetros de simulación
N = 50                # Tamaño de la grilla
p = 0.01               # Probabilidad de oponerse a la mayoría
max_steps = 1000      # Paso máximo (por si nunca se estabiliza)
output_file = "output.txt"
consenso_file = "consenso.txt"
semilla_file = "semilla.txt"

# Parámetros para detectar estado estacionario
epsilon = 0.001       # Tolerancia para considerar M estable
window = 10           # Cantidad de pasos consecutivos estables requeridos

# Leer grilla inicial desde archivo semilla.txt
def load_seed(file_path, N):
    with open(file_path, "r") as f:
        lines = [line.strip() for line in f.readlines()]
    grid = np.array([list(map(int, line.split())) for line in lines])
    assert grid.shape == (N, N), f"La semilla no tiene tamaño {N}x{N}"
    return grid

grid = load_seed(semilla_file, N)

# Obtener mayoría entre 4 vecinos
def get_majority(grid, i, j):
    up    = grid[(i - 1) % N, j]
    down  = grid[(i + 1) % N, j]
    left  = grid[i, (j - 1) % N]
    right = grid[i, (j + 1) % N]
    neighbors = [up, down, left, right]
    ones = neighbors.count(1)
    zeros = neighbors.count(-1)
    if ones > zeros:
        return 1
    elif zeros > ones:
        return -1
    else:
        return None

# Un paso de Monte Carlo
def monte_carlo_step(grid, p):
    for _ in range(N * N):
        i = np.random.randint(0, N)
        j = np.random.randint(0, N)
        majority = get_majority(grid, i, j)
        if majority is not None:
            if np.random.rand() < p:
                grid[i, j] = -majority  # Invierte el estado en {-1,1}
            else:
                grid[i, j] = majority
    return grid

# Calcular consenso M(t)
def compute_M(grid):
    return np.abs(np.sum(grid)) / (N * N)

# Simulación con corte por estado estacionario
consensus_history = []
output_lines = []

for step in range(max_steps):
    grid = monte_carlo_step(grid, p)

    # Guardar estado aplanado (para animación)
    flat = grid.flatten()
    line = ' '.join(map(str, flat))  # Ahora con espacios entre valores
    output_lines.append(line)

    # Calcular y guardar consenso
    M = compute_M(grid)
    consensus_history.append((step, M))

    # Verificar estado estacionario
    if len(consensus_history) >= window:
        recent = [m for _, m in consensus_history[-window:]]
        if max(recent) - min(recent) < epsilon:
            print(f"Estado estacionario alcanzado en el paso {step}")
            break

# Guardar archivo de grillas
with open(output_file, "w") as f:
    for line in output_lines:
        f.write(line + "\n")

# Guardar archivo de consenso
with open(consenso_file, "w") as f:
    for step, M in consensus_history:
        f.write(f"{step},{M}\n")

print(f"Simulación finalizada. Se guardaron {len(consensus_history)} pasos en '{output_file}' y '{consenso_file}'")
