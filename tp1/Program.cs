using System;
using tp1;
using tp1.Simulation;

Console.WriteLine("Starting up...");

var config = SimulationConfig.FromFile("Examples/Static100.txt");
Simulation simulation = new Simulation(config);
simulation.Initialize();

// new Window().Run();

Console.WriteLine("Goodbye!");
