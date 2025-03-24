using System;
using tp2;

Console.WriteLine("Starting up simulation...");

using var simulation = Simulation.Builder()
    .WithSeedFile("data/semilla.txt")
    .WithProbability(0.02f)
    .WithRandomSeed(1234)
    .WithMaxSteps(20000)
    .WithStationaryWindowSize(100)
    .WithOutputFile("output.txt")
    .WithConsensoFile("consenso.txt")
    .Build();

simulation.Run();

Console.WriteLine("goodbye :-)");
