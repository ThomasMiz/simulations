import matplotlib.pyplot as plt
import matplotlib.animation as animation
import matplotlib.patches as patches
import numpy as np
import re
import json
import os
import glob
from pathlib import Path

# ==== CONFIGURACIÓN MANUAL (ajustá si tu output cambia) ====
HALLWAY_LENGTH = 16
HALLWAY_WIDTH = 3.6
PARTICLE_RADIUS = 0.25

# ==== FUNCIONES PARA ENCONTRAR ARCHIVOS DE OUTPUT ====

def find_output_files():
    """Busca archivos de output en bin/Debug/net8.0/"""
    # Directorio donde se generan los outputs
    output_dir = Path(__file__).parent.parent / "bin" / "Debug" / "net8.0"
    
    if not output_dir.exists():
        return []
    
    # Buscar archivos que contengan "output" en el nombre y tengan extensión .txt
    output_files = list(output_dir.glob("output*.txt"))
    
    # También buscar otros patrones comunes
    for pattern in ["simulation*.txt", "resultado*.txt", "*.out"]:
        output_files.extend(output_dir.glob(pattern))
    
    # Remover duplicados y ordenar por fecha de modificación (más reciente primero)
    output_files = list(set(output_files))
    return sorted(output_files, key=lambda f: f.stat().st_mtime, reverse=True)

def select_output_file():
    """Selecciona automáticamente el archivo de output"""
    files = find_output_files()
    
    if not files:
        print("❌ No se encontraron archivos de output en bin/Debug/net8.0/")
        print("   Ejecutá primero la simulación para generar el archivo de output")
        return None
    
    if len(files) == 1:
        print(f"📁 Archivo encontrado: {files[0].name}")
        return str(files[0])
    
    print(f"📁 Se encontraron {len(files)} archivos de output:")
    for i, file in enumerate(files, 1):
        print(f"   {i}. {file.name}")
    
    print(f"\n🎯 Usando el más reciente: {files[0].name}")
    return str(files[0])

# ==== PARSING DEL OUTPUT ====
def parse_line(line):
    # Separar en estado global y partículas
    bloques = [b.strip() for b in line.strip().split(' ; ') if b.strip()]
    if not bloques:
        return None, None
    main = bloques[0]
    particles = bloques[1:] if len(bloques) > 1 else []
    state = json.loads(main)
    parts = [json.loads(p) for p in particles] if particles else []
    return state, parts

def load_simulation(filename):
    frames = []
    with open(filename) as f:
        lines = f.readlines()
        # Saltear línea de cabecera
        for line in lines[1:]:
            if not line.strip():
                continue
            state, particles = parse_line(line)
            if state is None or particles is None or not particles:
                continue  # Ignora frames sin partículas
            xs = [p['x'] for p in particles]
            ys = [p['y'] for p in particles]
            types = [p['name'] for p in particles]
            frames.append({'time': state['time'], 'x': xs, 'y': ys, 'type': types})
    return frames

# ==== ANIMACIÓN ====

def animate_simulation(frames):
    # Ajustar las proporciones de la figura para que coincidan con las dimensiones reales
    aspect_ratio = HALLWAY_LENGTH / HALLWAY_WIDTH  # 16/3.6 ≈ 4.44
    fig_height = 4
    fig_width = fig_height * aspect_ratio
    
    fig, ax = plt.subplots(figsize=(fig_width, fig_height))

    ax.set_xlim(0, HALLWAY_LENGTH)
    ax.set_ylim(0, HALLWAY_WIDTH)
    ax.set_aspect('equal')
    ax.set_title('Simulación dinámica peatonal (TP5) - Escala real')
    ax.set_xlabel('x [m]')
    ax.set_ylabel('y [m]')
    
    # Agregar grilla para referencia de escala
    ax.grid(True, alpha=0.3)
    ax.set_xticks(np.arange(0, HALLWAY_LENGTH + 1, 2))
    ax.set_yticks(np.arange(0, HALLWAY_WIDTH + 1, 0.5))

    # Dibujar las paredes del pasillo
    # Pared inferior (y = 0)
    ax.plot([0, HALLWAY_LENGTH], [0, 0], 'k-', linewidth=3, label='Paredes')
    # Pared superior (y = HALLWAY_WIDTH)
    ax.plot([0, HALLWAY_LENGTH], [HALLWAY_WIDTH, HALLWAY_WIDTH], 'k-', linewidth=3)
    
    # Opcional: marcar las entradas/salidas con líneas punteadas
    ax.plot([0, 0], [0, HALLWAY_WIDTH], 'k--', linewidth=2, alpha=0.7, label='Entradas/Salidas')
    ax.plot([HALLWAY_LENGTH, HALLWAY_LENGTH], [0, HALLWAY_WIDTH], 'k--', linewidth=2, alpha=0.7)

    # Listas para almacenar los círculos de las partículas
    circles_left = []
    circles_right = []
    circles_other = []
    
    # Colores para cada tipo
    colors = {
        'left': 'blue',
        'right': 'red', 
        'other': 'green'
    }

    def update(frame_idx):
        # Limpiar círculos anteriores
        for circle in circles_left + circles_right + circles_other:
            circle.remove()
        circles_left.clear()
        circles_right.clear()
        circles_other.clear()
        
        data = frames[frame_idx]
        xs, ys, types = data['x'], data['y'], data['type']
        
        # Crear círculos para cada partícula con su tamaño real
        for x, y, particle_type in zip(xs, ys, types):
            if 'Left' in particle_type:
                circle = patches.Circle((x, y), PARTICLE_RADIUS, 
                                      facecolor=colors['left'], 
                                      edgecolor='black', 
                                      linewidth=0.5,
                                      alpha=0.8)
                circles_left.append(circle)
            elif 'Right' in particle_type:
                circle = patches.Circle((x, y), PARTICLE_RADIUS, 
                                      facecolor=colors['right'], 
                                      edgecolor='black', 
                                      linewidth=0.5,
                                      alpha=0.8)
                circles_right.append(circle)
            else:
                circle = patches.Circle((x, y), PARTICLE_RADIUS, 
                                      facecolor=colors['other'], 
                                      edgecolor='black', 
                                      linewidth=0.5,
                                      alpha=0.8)
                circles_other.append(circle)
            
            ax.add_patch(circle)
        
        ax.set_title(f"t = {data['time']:.2f} s | N = {len(xs)} partículas | Radio: {PARTICLE_RADIUS}m")
        ax.set_ylim(0, HALLWAY_WIDTH)
        return circles_left + circles_right + circles_other

    # Crear leyenda manual
    legend_elements = [
        patches.Circle((0, 0), 0.1, facecolor=colors['left'], label='SFM-Left'),
        patches.Circle((0, 0), 0.1, facecolor=colors['right'], label='SFM-Right'),
        patches.Circle((0, 0), 0.1, facecolor=colors['other'], label='Other')
    ]
    ax.legend(handles=legend_elements, loc='upper right')

    ani = animation.FuncAnimation(fig, update, frames=len(frames), interval=50, blit=False)
    plt.tight_layout()
    plt.show()

# ==== USO ====

if __name__ == "__main__":
    import sys
    
    # Si se pasa un archivo como parámetro, usarlo
    if len(sys.argv) >= 2:
        filename = sys.argv[1]
        print(f"📁 Usando archivo especificado: {filename}")
    else:
        # Buscar automáticamente en bin/Debug/net8.0/
        filename = select_output_file()
        if filename is None:
            exit(1)
    
    try:
        frames = load_simulation(filename)
        print(f"✅ Cargados {len(frames)} frames de simulación")
        animate_simulation(frames)
    except Exception as e:
        print(f"❌ Error al cargar la simulación: {e}")
        print("   Verifica que el archivo tenga el formato correcto")
