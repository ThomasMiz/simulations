import struct

class SimulationStepData:
    def __init__(self, step_number, time, particles_data):
        self.step_number = step_number
        self.time = time
        self.particles_data = particles_data

    def position_of(self, i):
        return (self.particles_data[i][0], self.particles_data[i][1])

    def velocity_of(self, i):
        return (self.particles_data[i][2], self.particles_data[i][3])

class SimulationData:
    def __init__(self, path):
        self.path = path
        self.radio_contenedor = None
        self.N = None
        self.masas = None
        self.radios = None
        self.steps_data = None
        self._load_data()
        print(f"Cargado archivo {path} con {self.N} particulas, {len(self.steps_data)} pasos y duración {self.steps_data[-1].time}")

    def _load_data(self):
        with open(self.path, 'rb') as f:
            self.radio_contenedor = struct.unpack('f', f.read(4))[0]
            self.N = struct.unpack('i', f.read(4))[0]

            self.masas = []
            self.radios = []
            for _ in range(self.N):
                tmp = struct.unpack('ff', f.read(8))
                self.masas.append(tmp[0])
                self.radios.append(tmp[1])

            self.steps_data = []
            while True:
                step_bytes = f.read(4)
                if not step_bytes:
                    break
                step_number = struct.unpack('i', step_bytes)[0]
                time = struct.unpack('f', f.read(4))[0]
                particles_data = [struct.unpack('ffff', f.read(16)) for _ in range(self.N)]
                self.steps_data.append(SimulationStepData(step_number, time, particles_data))

    def __str__(self):
        return f"Simulación: {self.path}"
