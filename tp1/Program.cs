﻿using System;
using System.Globalization;
using tp1;
using tp1.Simulate;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Console.WriteLine("Starting up...");

// var config = SimulationConfig.FromFile("Examples/Static100.txt");
// Simulation simulation = new Simulation(config);
// simulation.Initialize();

new Window().Run();

Console.WriteLine("Goodbye!");