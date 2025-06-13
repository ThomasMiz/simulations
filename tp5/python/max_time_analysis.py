import os
import json
import glob

# Directorio de salida
output_dir = "../bin/Debug/net8.0"
file_pattern = "probability/no_bloqueados/output-simple-Q8-B*-beeman-run-*.txt"

# Encontrar todos los archivos que coincidan con el patrón
file_paths = glob.glob(os.path.join(output_dir, file_pattern))

max_time = 0
max_time_file = None

for file_path in file_paths:
    last_time = None
    
    try:
        with open(file_path, 'r') as f:
            for line in f:
                if "step" not in line:
                    continue
                try:
                    time_info = json.loads(line.strip().split(" ; ")[0])
                    last_time = time_info["time"]
                except (json.JSONDecodeError, IndexError, KeyError):
                    continue
        
        if last_time is not None and last_time > max_time:
            max_time = last_time
            max_time_file = file_path
            
    except Exception as e:
        print(f"Error al procesar {file_path}: {str(e)}")

if max_time_file:
    print(f"\nEl archivo con el tiempo final más largo es:")
    print(f"Archivo: {os.path.basename(max_time_file)}")
    print(f"Tiempo final: {max_time:.2f} segundos")
else:
    print("No se encontraron archivos válidos para analizar.") 