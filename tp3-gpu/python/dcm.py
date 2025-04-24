#!/usr/bin/env python3
"""
compute_msd_diffusion.py

Lee todos los archivos .sim en outputs-movingobstacle/, extrae la
trayectoria de la partícula grande (primera en cada archivo),
calcula el MSD ⟨|r(t)-r₀|²⟩ promedio y desviación estándar,
ajusta una regresión lineal ⟨r²⟩ = 4 D t para extraer D,
y dibuja el MSD con barras de error y línea de datos.
"""

import os
import struct
import glob

import numpy as np
import matplotlib.pyplot as plt

# ---- Parámetros ----
DATA_DIR       = "./outputs-movingobstacle"
FILE_PATTERN   = "output*-movingobstacle-*.sim"
PARTICLE_INDEX = 0     # índice de la partícula grande
N_SAMPLES      = 100   # número de tiempos uniformes
FIT_START_FRAC = 0.1   # descartar el primer 10% de t_max para el ajuste

def read_sim_file(path):
    """
    Lee un archivo .sim y devuelve:
      - times: array de tiempos (float)
      - positions: array de shape (len(times), 2) con (x, y) de la partícula grande
    """
    times, positions = [], []
    with open(path, "rb") as f:
        # Leer header
        f.read(4)                  # container_radius (float)
        n = struct.unpack("I", f.read(4))[0]  # número de partículas
        f.read(n * 8)              # saltar masas y radios (2 floats c/u)
        # Leer pasos
        while True:
            hdr = f.read(8)        # step (uint32) + t (float)
            if len(hdr) < 8:
                break
            _, t = struct.unpack("If", hdr)
            data = f.read(n * 16)  # x,y,vx,vy (4 floats) por partícula
            if len(data) < n * 16:
                break
            offset = PARTICLE_INDEX * 16
            x, y, _, _ = struct.unpack_from("ffff", data, offset)
            times.append(t)
            positions.append((x, y))
    return np.array(times), np.array(positions)

def main():
    # 1) Listar archivos
    pattern = os.path.join(DATA_DIR, FILE_PATTERN)
    files = sorted(glob.glob(pattern))
    if not files:
        raise FileNotFoundError(f"No se encontraron archivos con patrón {pattern!r}")
    print(f"→ {len(files)} archivos encontrados")

    # 2) Leer corridas
    runs = []
    for path in files:
        t, pos = read_sim_file(path)
        if len(t) > 0:
            runs.append((t, pos))
    if not runs:
        raise RuntimeError("Ninguna corrida válida fue leída.")

    # 3) Tiempo máximo común y tiempos uniformes
    t_max    = min(t_run[-1] for t_run, _ in runs)
    t_common = np.linspace(0, t_max, N_SAMPLES)

    # 4) Calcular MSD por corrida
    msd_runs = []
    for t_run, pos_run in runs:
        disp2 = np.sum((pos_run - pos_run[0])**2, axis=1)
        msd_runs.append(np.interp(t_common, t_run, disp2))
    msd_runs = np.array(msd_runs)

    # 5) Promedio y desviación estándar
    msd_mean = msd_runs.mean(axis=0)
    msd_std  = msd_runs.std(axis=0)

    # 6) Ajuste lineal en la región difusiva
    mask = t_common >= (FIT_START_FRAC * t_max)
    slope, intercept = np.polyfit(t_common[mask], msd_mean[mask], 1)
    D = slope / 4.0
    print(f"Rango de ajuste: t ∈ [{t_common[mask][0]:.3f}, {t_common[-1]:.3f}] s")
    print(f"Pendiente = {slope:.5e}  ->  D = {D:.5e} m²/s")

    # 7) Gráfica con línea que une puntos y barras de error
    plt.figure(figsize=(8,5))
    plt.errorbar(
        t_common, msd_mean, yerr=msd_std,
        marker='o', linestyle='-', capsize=4,
        label='MSD ± σ'
    )
    # Línea de ajuste
    plt.plot(
        t_common,
        intercept + slope * t_common,
        'r--', linewidth=2,
        label=f'Ajuste lineal\nD = {D:.2e} m²/s'
    )
    plt.xlabel("Tiempo t [s]")
    plt.ylabel("MSD ⟨|r(t)-r₀|²⟩ [m²]")
    plt.title("MSD vs Tiempo con Línea de Datos y Ajuste")
    plt.legend()
    plt.tight_layout()
    plt.savefig("msd_vs_time_line.png", dpi=150)
    plt.show()

if __name__ == "__main__":
    main()
