import os
import matplotlib.pyplot as plt
import json

def is_blocked(filename):
    """
    Determina si una simulación está bloqueada analizando el archivo de salida.
    Una simulación está bloqueada si en el último tiempo hay partículas presentes.
    """
    if not os.path.exists(filename):
        return None  # Archivo no existe
    
    try:
        last_particles = []
        with open(filename, "r") as f:
            for line in f:
                # Solo líneas con partículas (las que tienen ';')
                if ";" in line:
                    # Ejemplo de línea:
                    # {"step": 101, "time": 0.101} ; {"id": 1, ...} ; {"id": 2, ...}
                    step_data, *particle_data = line.strip().split(" ; ")
                    
                    # Contar partículas en esta línea
                    current_particles = []
                    for pdata in particle_data:
                        pdata = pdata.strip()
                        if pdata.endswith(","):
                            pdata = pdata[:-1]
                        if pdata:  # Si hay datos de partícula
                            try:
                                p = json.loads(pdata)
                                current_particles.append(p)
                            except json.JSONDecodeError:
                                continue
                    
                    # Actualizar la última lista de partículas
                    last_particles = current_particles
        
        # Si en el último tiempo hay partículas, está bloqueado
        return len(last_particles) > 2
        
    except Exception as e:
        print(f"Error leyendo {filename}: {e}")
        return None

B_values = [0.02, 0.04, 0.06, 0.08, 0.1]
output_dir = "../bin/Debug/net8.0/"

B_probs = []
unblocked_runs = {}  # Diccionario para guardar los runs no bloqueados por cada B
blocked_runs = {}    # Diccionario para guardar los runs bloqueados por cada B
missing_runs = {}    # Diccionario para guardar los runs con archivos faltantes

for B in B_values:
    blocked = 0
    unblocked = 0
    missing = 0
    unblocked_runs[B] = []
    blocked_runs[B] = []
    missing_runs[B] = []

    for run in range(1, 11): 
        # Probar diferentes ubicaciones del archivo
        possible_files = [
            os.path.join(output_dir, f"probability/output-simple-Q8-B{B}-beeman-run-{run}.txt"),
        ]
        
        file_found = False
        for file_path in possible_files:
            if os.path.exists(file_path):
                result = is_blocked(file_path)
                if result is True:
                    blocked += 1
                    blocked_runs[B].append(run)
                elif result is False:
                    unblocked += 1
                    unblocked_runs[B].append(run)
                file_found = True
                break
        
        if not file_found:
            missing += 1
            missing_runs[B].append(run)

    total = blocked + unblocked
    prob = blocked / total if total > 0 else 0
    B_probs.append(prob)
    
    print(f"B = {B:.2f}: {blocked} bloqueados, {unblocked} no bloqueados, {missing} archivos faltantes")

# 📊 Graficar
plt.figure(figsize=(8, 6))
bars = plt.bar(B_values, B_probs, color='#FF4500', edgecolor='black', linewidth=1, width=0.01)

# Etiquetas de valor arriba de cada barra
for bar in bars:
    height = bar.get_height()
    plt.text(bar.get_x() + bar.get_width() / 2, height + 0.02, f"{height:.2f}",
             ha='center', va='bottom', fontsize=14, fontweight='bold')

plt.ylim(0, 1.05)
plt.xlabel(r"B [m]", fontsize=16, fontweight='bold')
plt.ylabel(r"$P_{\mathrm{bloqueo}}$", fontsize=16, fontweight='bold')
plt.title("Probabilidad de bloqueo vs B", fontsize=18, fontweight='bold', pad=20)

# Configurar ejes con formato limpio
plt.xticks(B_values, [f"{B:.2f}" for B in B_values], fontsize=14)
plt.yticks([0.00, 0.25, 0.50, 0.75, 1.00], fontsize=14)

# Sin grilla para aspecto más limpio
plt.grid(False)

# Mejorar apariencia de los ejes
plt.gca().spines['top'].set_visible(False)
plt.gca().spines['right'].set_visible(False)
plt.gca().spines['left'].set_linewidth(1)
plt.gca().spines['bottom'].set_linewidth(1)

# Agregar nota al pie
plt.figtext(0.12, 0.02, "Se realizaron hasta 10 corridas por simulación", fontsize=12, style='italic')

plt.tight_layout()
plt.show()

# Imprimir información detallada
print("\n" + "="*60)
print("ANÁLISIS DETALLADO POR CADA VALOR DE B:")
print("="*60)
for B in B_values:
    print(f"\nB = {B:.2f}:")
    if blocked_runs[B]:
        runs_str = ", ".join(map(str, blocked_runs[B]))
        print(f"  Runs BLOQUEADOS: {runs_str}")
    else:
        print(f"  Runs BLOQUEADOS: ninguno")
    
    if unblocked_runs[B]:
        runs_str = ", ".join(map(str, unblocked_runs[B]))
        print(f"  Runs NO BLOQUEADOS: {runs_str}")
    else:
        print(f"  Runs NO BLOQUEADOS: ninguno")
    
    if missing_runs[B]:
        runs_str = ", ".join(map(str, missing_runs[B]))
        print(f"  Archivos FALTANTES: {runs_str}")
print("="*60)
