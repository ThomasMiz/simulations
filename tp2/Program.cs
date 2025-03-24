using tp2;

Console.WriteLine("Starting up simulation...");

var config = new SimulationConfig()
{
    GridFile = "data/semilla.txt",
    Probability = 0.02f,
    RandomSeed = 1234,
    MaxSteps = 20000,
    StationaryWindowSize = 100,
    ContinueAfterStationary = 1000,
    OutputFile = "output.txt",
    ConsensoFile = "consenso.txt",
};

using var simulation = config.Build();

simulation.Run();

Console.WriteLine("goodbye :-)");