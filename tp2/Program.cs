﻿using tp2;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

Console.WriteLine("Starting up simulation...");

var config = new SimulationConfig()
{
    GridFile = "data/semilla.txt",
    Probability = 0.02f,
    RandomSeed = 1234,
    MaxSteps = 50000,
    StationaryWindowSize = 100,
    ContinueAfterStationary = 1000,
    OutputFile = "output.txt",
    ConsensoFile = "consenso.txt",
    ClusterStatsFile = "clusterstats.txt",
};

using var simulation = config.Build();

simulation.Run();

if (simulation.StationaryReached)
{
    Console.WriteLine("Susceptibility is {0}", simulation.CalculateSusceptibility());
}

Console.WriteLine("goodbye :-)");