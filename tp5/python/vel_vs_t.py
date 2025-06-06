import json
import pandas as pd
import matplotlib.pyplot as plt

# Ruta al archivo de output
file_path = "../bin/Debug/net8.0/output-simple-Q8-beeman.txt"

# Leer el archivo línea por línea
with open(file_path, 'r') as f:
    lines = f.readlines()

# Procesar los datos en frames
frames = []
current_frame = None

for line in lines:
    line = line.strip()
    if line.startswith("{\"step\""):
        if current_frame is not None and "particles" in current_frame:
            frames.append(current_frame)
        parts = line.split(" ; ")
        step_info = json.loads(parts[0])
        current_frame = {"step": step_info["step"], "time": step_info["time"], "particles": []}
        if len(parts) > 1:
            try:
                current_frame["particles"].append(json.loads(parts[1]))
            except json.JSONDecodeError:
                pass
    elif current_frame is not None:
        try:
            current_frame["particles"].append(json.loads(line))
        except json.JSONDecodeError:
            continue

# Agregar el último frame
if current_frame is not None and "particles" in current_frame:
    frames.append(current_frame)

# Calcular <|vx|> para cada instante de tiempo
time_list = []
avg_vx_abs_list = []

for frame in frames:
    time = frame["time"]
    particles = frame["particles"]
    if not particles:
        continue
    vx_vals = [abs(p["vx"]) for p in particles]
    avg_vx_abs = sum(vx_vals) / len(vx_vals)
    time_list.append(time)
    avg_vx_abs_list.append(avg_vx_abs)

# Crear DataFrame
df = pd.DataFrame({
    "time": time_list,
    "avg_abs_vx": avg_vx_abs_list
})

# Mostrar gráfica
plt.figure(figsize=(10, 5))
plt.plot(df["time"], df["avg_abs_vx"], label="<|vx|>(t)")
plt.xlabel("Tiempo (s)")
plt.ylabel("Velocidad promedio en x (m/s)")
plt.title("Evolución temporal de <|vx|>")
plt.grid(True)
plt.legend()
plt.tight_layout()
plt.show()
