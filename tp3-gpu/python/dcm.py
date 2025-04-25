#!/usr/bin/env python3
"""
compute_msd_diffusion.py

Lee todos los archivos .sim en outputs-movingobstacle/, extrae la
trayectoria de la partícula grande (primera en cada archivo),
calcula el MSD ⟨|r(t)-r₀|²⟩ promedio y desviación estándar,
ignora datos a partir del primer choque con el borde,
ajusta una regresión lineal ⟨r²⟩ = 4 D t para extraer D,
y dibuja el MSD con barras de error y línea de ajuste.
"""

import os
import struct
import glob

import numpy as np
import matplotlib.pyplot as plt
from matplotlib.ticker import ScalarFormatter

# ---- Parámetros ----
DATA_DIR         = "./outputs-movingobstacle"
FILE_PATTERN     = "output*-movingobstacle-*.sim"
PARTICLE_INDEX   = 0       # índice de la partícula grande
PARTICLE_RADIUS  = 0.005   # radio de la partícula grande [m]
N_SAMPLES        = 100     # número de tiempos uniformes
FIT_START_FRAC   = 0.1     # descartar el primer 10% de t_max para el ajuste


def read_sim_file(path):
    """
    Lee un archivo .sim y devuelve:
      - container_radius: float
      - times: array de tiempos (float)
      - positions: array de shape (len(times), 2) con (x, y) de la partícula grande
    """
    times, positions = [], []
    with open(path, "rb") as f:
        # Leer container_radius (float)
        container_radius = struct.unpack('f', f.read(4))[0]
        # Leer número de partículas (uint32)
        n = struct.unpack('I', f.read(4))[0]
        # Leer masas y radios (skip)
        f.read(n * 8)
        # Leer steps
        while True:
            hdr = f.read(8)
            if len(hdr) < 8:
                break
            _, t = struct.unpack("If", hdr)
            data = f.read(n * 16)
            if len(data) < n * 16:
                break
            off = PARTICLE_INDEX * 16
            x, y, _, _ = struct.unpack_from("ffff", data, off)
            times.append(t)
            positions.append((x, y))
    return container_radius, np.array(times), np.array(positions)


def main():
    # 1) Listar archivos
    pattern = os.path.join(DATA_DIR, FILE_PATTERN)
    files = sorted(glob.glob(pattern))
    if not files:
        raise FileNotFoundError(f"No se encontraron archivos con patrón {pattern!r}")

    runs = []  # lista de (t_run, pos_run) tras cortar al primer choque
    for path in files:
        container_radius, times, pos = read_sim_file(path)
        # Umbral de choque contra borde:
        threshold = container_radius - PARTICLE_RADIUS
        # Calcular distancias al centro
        d = np.linalg.norm(pos, axis=1)
        # Encontrar primer índice donde choca contra el borde
        idx = np.argmax(d >= threshold)
        if d[idx] < threshold:
            # nunca choca, usar todo
            t_cut, pos_cut = times, pos
        else:
            # cortar hasta justo antes del choque
            t_cut   = times[:idx]
            pos_cut = pos[:idx]
        # Guardar si hay datos suficientes
        if t_cut.size > 1:
            runs.append((t_cut, pos_cut))

    if not runs:
        raise RuntimeError("No hay datos válidos tras recortar bordes.")

    # 2) Tiempo máximo común y retículo uniforme
    t_max    = min(t[-1] for t, _ in runs)
    t_common = np.linspace(0, t_max, N_SAMPLES)

    # 3) Calcular MSD por corrida
    msd_runs = []
    for t_run, pos_run in runs:
        disp2 = np.sum((pos_run - pos_run[0])**2, axis=1)
        msd_runs.append(np.interp(t_common, t_run, disp2))
    msd_runs = np.array(msd_runs)

    # 4) Promedio y desviación estándar
    msd_mean = msd_runs.mean(axis=0)
    msd_std  = msd_runs.std(axis=0)

    # 5) Ajuste lineal en la región difusiva
    mask = t_common >= (FIT_START_FRAC * t_max)
    slope, intercept = np.polyfit(t_common[mask], msd_mean[mask], 1)
    D = slope / 4.0
    print(f"Fase de ajuste: t ∈ [{t_common[mask][0]:.3f}, {t_common[-1]:.3f}] s")
    print(f"Pendiente = {slope:.5e}  →  D = {D:.5e} m²/s")

        # 6) Graficar
    plt.figure(figsize=(15,6))
    plt.errorbar(
        t_common, msd_mean, yerr=msd_std,
        marker='o', linestyle='-', capsize=4
    )
    
    exp  = int(np.floor(np.log10(abs(D))))
    mant = D / 10**exp
    
    # creamos el label con MathText
    label_ajuste = rf'Ajuste lineal, $D = {mant:.2f}\times10^{{{exp}}}\ \mathrm{{m}}^2/\mathrm{{s}}$'
    plt.plot(
        t_common,
        intercept + slope * t_common,
        'r--', linewidth=2,
        label=label_ajuste
    )
    plt.xlabel("Tiempo [s]", fontsize=20)
    plt.ylabel("DCM [m²]", fontsize=20)
    plt.xticks(fontsize=20)
    plt.yticks(fontsize=20)
    
    # activar notación científica en el eje Y
    from matplotlib.ticker import ScalarFormatter
    ax = plt.gca()
    formatter = ScalarFormatter(useMathText=True)
    formatter.set_scientific(True)
    formatter.set_powerlimits((0, 0))
    formatter.set_useOffset(False)
    ax.yaxis.set_major_formatter(formatter)
    ax.yaxis.get_offset_text().set_fontsize(20)

    plt.legend(fontsize=20)
    plt.grid(True, alpha=0.3)
    plt.tight_layout()
    plt.savefig("msd_vs_time_border_cut.png", dpi=150)
    plt.show()


if __name__ == "__main__":
    main()
