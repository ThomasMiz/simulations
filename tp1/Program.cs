using System;
using tp1;
using tp1.Simulate;

Console.WriteLine("Starting up...");

var config = SimulationConfig.FromFile("Examples/Static100.txt");
Simulation simulation = new Simulation(config);
simulation.Initialize();

new Window(simulation).Run();

Console.WriteLine("Goodbye!");
