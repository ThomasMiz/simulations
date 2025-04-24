#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
plot_collisions_limited.py

Similar a plot_collisions_two_plots.py, pero recorta todos los gráficos
hasta el tiempo mínimo común (t_max) de las simulaciones.

Uso:
  python plot_collisions_limited.py \
    outputs-fixedobstacle/output1-fixedobstacle-250particles-vel1-50ksteps.sim \
    outputs-fixedobstacle/output1-fixedobstacle-250particles-vel3-50ksteps.sim \
    outputs-fixedobstacle/output1-fixedobstacle-250particles-vel6-50ksteps.sim \
    outputs-fixedobstacle/output1-fixedobstacle-250particles-vel10-50ksteps.sim \
    -r 0.005 --tol-factor 0.00001
"""

import os, re, struct, argparse
import numpy as np
import matplotlib.pyplot as plt

def read_collisions(simfile, R_obst, tol_factor):
    data = open(simfile, "rb").read()
    offset = 0
    offset += 4  # container_radius
    N = struct.unpack_from("I", data, offset)[0]; offset += 4
    radii = np.empty(N, dtype=np.float32)
    for i in range(N):
        _, r = struct.unpack_from("ff", data, offset)
        radii[i] = r
        offset += 8

    hit_first   = np.zeros(N, bool)
    times_first = []
    times_all   = []
    t_final     = 0.0

    while offset + 8 + 16*N <= len(data):
        _, t = struct.unpack_from("If", data, offset)
        offset += 8
        pos = np.empty((N,2), dtype=np.float32)
        for i in range(N):
            x,y,_,_ = struct.unpack_from("ffff", data, offset)
            pos[i] = (x,y)
            offset += 16
        t_final = t

        dist     = np.hypot(pos[:,0], pos[:,1])
        sum_r    = R_obst + radii
        tol      = tol_factor * sum_r
        collided = np.abs(dist - sum_r) < tol

        for i in np.nonzero(collided)[0]:
            times_all.append(t)
            if not hit_first[i]:
                hit_first[i] = True
                times_first.append(t)

    return np.array(times_first), np.array(times_all), t_final

def main():
    p = argparse.ArgumentParser()
    p.add_argument("simfiles", nargs="+", help="archivos .sim de distintas velocidades")
    p.add_argument("-r","--obstacle-radius", type=float, default=0.005, help="radio del obstáculo (m)")
    p.add_argument("--tol-factor", type=float, default=0.5, help="tolerancia (fracción de R_obst+r_i)")
    args = p.parse_args()

    # Leer todas las corridas y extraer tiempos
    results = []
    for simfile in args.simfiles:
        t_first, t_all, t_final = read_collisions(simfile, args.obstacle_radius, args.tol_factor)
        m = re.search(r"vel(\d+)", os.path.basename(simfile))
        vel = (m.group(1) + " m/s") if m else os.path.basename(simfile)
        results.append((vel, t_first, t_all, t_final))

    # Tiempo máximo común a todas las simulaciones
    t_max = min(t_final for _,_,_,t_final in results)

    # === Gráfico 1: TODOS los choques acumulados, limitado a t_max ===
    plt.figure(figsize=(8,5))
    for vel, t_first, t_all, t_final in results:
        # ordenar y truncar
        t_all_sorted = np.sort(t_all)
        mask_all = t_all_sorted <= t_max
        t_plot_all = t_all_sorted[mask_all]
        N_all_cum = np.arange(1, len(t_plot_all)+1)
        plt.step(t_plot_all, N_all_cum, where='post', label=f"v={vel}", linewidth=2)

    plt.xlim(0, t_max)
    plt.xlabel("Tiempo (s)")
    plt.ylabel("Choques acumulados (todos)")
    plt.title(f"Choques todos (hasta t_max={t_max:.3f}s)")
    plt.legend(fontsize="small")
    plt.grid(alpha=0.3)
    plt.tight_layout()
    plt.savefig("collisions_all_limited.png", dpi=150)

    # === Gráfico 2: PRIMER choque acumulado, limitado a t_max ===
    plt.figure(figsize=(8,5))
    for vel, t_first, t_all, t_final in results:
        t_first_sorted = np.sort(t_first)
        mask_first = t_first_sorted <= t_max
        t_plot_first = t_first_sorted[mask_first]
        N_first_cum = np.arange(1, len(t_plot_first)+1)
        plt.step(t_plot_first, N_first_cum, where='post', linestyle='--', label=f"v={vel}", linewidth=2)

    plt.xlim(0, t_max)
    plt.xlabel("Tiempo (s)")
    plt.ylabel("Choques acumulados (1er choque)")
    plt.title(f"Primer choque (hasta t_max={t_max:.3f}s)")
    plt.legend(fontsize="small")
    plt.grid(alpha=0.3)
    plt.tight_layout()
    plt.savefig("collisions_first_limited.png", dpi=150)

    plt.show()

    # === Imprimir frecuencias de choque ===
    print("\nFrecuencias de choque (1/s):")
    for vel, t_first, t_all, t_final in results:
        nu_all   = len(t_all[t_all <= t_max])   / t_max
        nu_first = len(t_first[t_first <= t_max]) / t_max
        print(f"  v={vel}: ν_all={nu_all:.3f}, ν_first={nu_first:.3f}")

if __name__=="__main__":
    main()
