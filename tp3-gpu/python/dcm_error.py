#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
plot_msd_error_vs_D_border_cut.py

Calcula el MSD promedio de la partícula grande (ignorando los datos tras 
el choque con el borde), luego, para un rango de valores de D candidato, 
evalúa el error:
    E(D) = Σ_i [ MSD(t_i) − 4 D t_i ]²
y traza E(D) vs D marcando el mínimo.
"""

import os
import struct
import glob
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.ticker import ScalarFormatter

# Parámetros MSD
DATA_DIR        = "./outputs-movingobstacle"
FILE_PATTERN    = "output*-movingobstacle-*.sim"
PART_INDEX      = 0       # índice de la partícula grande
PARTICLE_RADIUS = 0.005   # radio de la partícula grande [m]
N_SAMPLES       = 100
FIT_START_FRAC  = 0.12

def read_sim_file(path):
    """
    Lee un .sim y devuelve:
      - container_radius (float)
      - times: array de tiempos (float)
      - pos:   array de shape (len(times),2) con (x,y) de la partícula grande
    """
    times = []
    pos   = []
    with open(path, "rb") as f:
        # 1) Leer el radius del contenedor
        container_radius = struct.unpack("f", f.read(4))[0]
        # 2) Leer número de partículas
        n = struct.unpack("I", f.read(4))[0]
        # 3) Saltar masas y radios
        f.read(n * 8)
        # 4) Leer cada step
        while True:
            hdr = f.read(8)
            if len(hdr) < 8:
                break
            _, t = struct.unpack("If", hdr)
            data = f.read(n * 16)
            if len(data) < n * 16:
                break
            off = PART_INDEX * 16
            x, y, _, _ = struct.unpack_from("ffff", data, off)
            times.append(t)
            pos.append((x, y))
    return container_radius, np.array(times), np.array(pos)

def compute_msd():
    # lee todas las corridas y las recorta al choque con el borde
    paths = sorted(glob.glob(os.path.join(DATA_DIR, FILE_PATTERN)))
    runs = []
    for p in paths:
        container_radius, t, pos = read_sim_file(p)
        # umbral de choque
        thresh = container_radius - PARTICLE_RADIUS
        # distancia al centro
        d = np.linalg.norm(pos, axis=1)
        # primer índice donde choca (d >= thresh)
        hits = np.where(d >= thresh)[0]
        if hits.size > 0:
            idx = hits[0]
            # recortar justo antes del choque
            t = t[:idx]
            pos = pos[:idx]
        # aceptar solo si quedan datos suficientes
        if t.size > 1:
            runs.append((t, pos))

    if not runs:
        raise RuntimeError("Tras recortar bordes no quedan datos válidos.")

    # tiempos comunes
    t_max    = min(t_run[-1] for t_run, _ in runs)
    t_common = np.linspace(0, t_max, N_SAMPLES)

    # interpolar cada corrida
    msd_runs = []
    for t_run, pos_run in runs:
        disp2 = np.sum((pos_run - pos_run[0])**2, axis=1)
        msd_runs.append(np.interp(t_common, t_run, disp2))
    msd = np.array(msd_runs)

    return t_common, msd.mean(axis=0)

def main():
    t_common, msd_mean = compute_msd()
    t_max = t_common[-1]

    # máscara para la fase difusiva
    mask = t_common >= FIT_START_FRAC * t_max

    # rango de D candidatos
    D_typical = (msd_mean[mask] @ t_common[mask])/(4*(t_common[mask]@t_common[mask]))
    D_vals    = np.linspace(0, 1.5 * D_typical, 200)

    # calcular E(D)
    errors = np.array([np.sum((msd_mean - 4*D*t_common)**2) for D in D_vals])

    # D* mínimo de E
    idx_min = np.argmin(errors)
    D_opt   = D_vals[idx_min]
    E_min   = errors[idx_min]

        # graficar
    exp_D  = int(np.floor(np.log10(abs(D_opt))))
    mant_D = D_opt / 10**exp_D
    
    fig, ax = plt.subplots(figsize=(15,6))
    
    ax.plot(D_vals, errors, '-o', markersize=4)
    
    ax.axvline(
        D_opt, color='r', linestyle='--',
        label=rf'$D^* = {mant_D:.2f}\times10^{{{exp_D}}}\,\mathrm{{m}}^2/\mathrm{{s}}$'
    )
    ax.scatter([D_opt], [E_min], color='r', zorder=5)
    
    ax.set_xlabel("D [m²/s]", fontsize=20)
    ax.set_ylabel("Error [m²]",   fontsize=20)
    
    # —————————————
    # NOTACIÓN CIENTÍFICA EN ejes X e Y
    # —————————————
    # Esto fuerza tanto las etiquetas de tick como el offset
    ax.ticklabel_format(
        style='sci',    # científica
        axis='x',       # en X
        scilimits=(0,0),# siempre ×10^k
        useMathText=True
    )
    ax.ticklabel_format(
        style='sci',
        axis='y',
        scilimits=(0,0),
        useMathText=True
    )
    
    # Aumentar tamaño del offset text (×10ⁿ)
    ax.xaxis.get_offset_text().set_fontsize(20)
    ax.yaxis.get_offset_text().set_fontsize(20)
    
    # aumentar tamaño de los ticks numéricos
    ax.tick_params(axis='both', labelsize=20)
    
    ax.legend(fontsize=20, loc='upper right')
    ax.grid(alpha=0.3)
    
    plt.tight_layout()
    plt.savefig("msd_error_vs_D_border_cut.png", dpi=150)
    plt.show()
    
    print(f"Coeficiente óptimo D* = {mant_D:.2f}×10^{exp_D} m²/s   Error mínimo E = {E_min:.5e}")



if __name__=="__main__":
    main()
