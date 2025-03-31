import numpy as np

N = 250  # Tamaño de la grilla
semilla_file = f"./data/semilla-{N}.txt"

# Generar semilla con valores {-1,1}
grid = np.random.choice([-1, 1], size=(N, N))

# Guardar la semilla en archivo
with open(semilla_file, "w") as f:
    for row in grid:
        f.write(' '.join(map(str, row)) + '\n')

print(f"Semilla generada en '{semilla_file}' con valores {-1,1} separados por espacios.")
