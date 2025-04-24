#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
plot_msd_error_vs_D.py

Calcula el MSD promedio de la partícula grande usando compute_msd_diffusion.py,
luego, para un rango de valores de D candidate, evalúa el error:
    E(D) = Σ_i [ MSD(t_i) − 4 D t_i ]²
o su versión media MSE(D)=E(D)/N,
y traza E(D) vs D marcando el mínimo.
"""

import os
import struct
import glob
import numpy as np
import matplotlib.pyplot as plt

# Parámetros MSD
DATA_DIR     = "./outputs-movingobstacle"
FILE_PATTERN = "output*-movingobstacle-*.sim"
PART_INDEX   = 0     # índice de la partícula grande
N_SAMPLES    = 100
FIT_START_FRAC = 0.1

def read_sim_file(path):
    times, pos = [], []
    with open(path,"rb") as f:
        f.read(4)
        n = struct.unpack("I", f.read(4))[0]
        f.read(n*8)
        while True:
            hdr = f.read(8)
            if len(hdr)<8: break
            _,t = struct.unpack("If", hdr)
            data = f.read(n*16)
            if len(data)<n*16: break
            off = PART_INDEX*16
            x,y,_,_ = struct.unpack_from("ffff", data, off)
            times.append(t); pos.append((x,y))
    return np.array(times), np.array(pos)

def compute_msd():
    # lee todas las corridas
    paths = sorted(glob.glob(os.path.join(DATA_DIR, FILE_PATTERN)))
    runs = []
    for p in paths:
        t,pos = read_sim_file(p)
        if len(t)>0: runs.append((t,pos))
    # tiempos comunes
    t_max = min(t[-1] for t,_ in runs)
    t_common = np.linspace(0, t_max, N_SAMPLES)
    # interpolar cada corrida
    msd_runs = []
    for t,pos in runs:
        disp2 = np.sum((pos-pos[0])**2,axis=1)
        msd_runs.append(np.interp(t_common, t, disp2))
    msd = np.array(msd_runs)
    return t_common, msd.mean(axis=0)

def main():
    t_common, msd_mean = compute_msd()

    # Seleccionamos la región de ajuste para D* (usado luego solo como guía)
    t_max = t_common[-1]
    mask = t_common >= FIT_START_FRAC * t_max

    # Generamos un grid de D candidatos
    D_vals = np.linspace(0, 1.5 * (msd_mean[mask] @ t_common[mask])/(4*(t_common[mask]@t_common[mask])), 200)
    # También podemos estrechar alrededor del D típico de la recta
    # pero este método adapta el rango automáticamente.

    # Calcular error E(D) = Σ [msd_mean - 4 D t]^2
    errors = []
    for D in D_vals:
        model = 4*D*t_common
        E = np.sum((msd_mean - model)**2)
        errors.append(E)
    errors = np.array(errors)

    # Encontrar D* que minimiza E
    idx_min = np.argmin(errors)
    D_opt = D_vals[idx_min]
    E_min = errors[idx_min]

    # Graficar E vs D
    plt.figure(figsize=(8,5))
    plt.plot(D_vals, errors, '-o', markersize=4, label='E(D) = Σ[MSD − 4Dt]²')
    plt.axvline(D_opt, color='r', linestyle='--', label=f'D* = {D_opt:.2e}')
    plt.scatter([D_opt],[E_min], color='r')
    plt.xlabel("D candidato [m²/s]")
    plt.ylabel("Error E(D)")
    plt.title("Error cuadrático de ajuste vs coeficiente D")
    plt.legend()
    plt.tight_layout()
    plt.savefig("msd_error_vs_D.png", dpi=150)
    plt.show()

    print(f"Coeficiente óptimo D* = {D_opt:.5e} m²/s   Error mínimo E = {E_min:.5e}")

if __name__=="__main__":
    main()
