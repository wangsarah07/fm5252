// to run the program, use 'dotnet run' in the terminal and input the parameters

// this is the interface between the user and engine
// it handles all user input from the terminal
// creates an option object and starts the engine
// it runs the simulation and prints the result

using System;

class Program
{
    static void Main()
    {
        // collect user input
        Console.Write("option type (call/put): ");
        string type = Console.ReadLine()!.Trim().ToLower();

        Console.Write("stock price: ");
        double S = double.Parse(Console.ReadLine()!);

        Console.Write("strike price: ");
        double K = double.Parse(Console.ReadLine()!);

        Console.Write("time to expiration (years): ");
        double T = double.Parse(Console.ReadLine()!);

        Console.Write("risk free rate (decimal): ");
        double r = double.Parse(Console.ReadLine()!);

        Console.Write("volatility (decimal): ");
        double sigma = double.Parse(Console.ReadLine()!);

        Console.Write("number of paths: ");
        int paths = int.Parse(Console.ReadLine()!);

        Console.Write("number of steps (per path): ");
        int steps = int.Parse(Console.ReadLine()!);

        // create objects
        Option option = new Option(type, S, K, T, r, sigma);
        Engine engine = new Engine(option, paths, steps);

        // run simulation 
        var result = engine.RunSimulation();

        // output results to user
        Console.WriteLine();
        Console.WriteLine($"price: {result.price:F6}");
        Console.WriteLine($"standard error: {result.stdErr:F6}");
        Console.WriteLine($"delta: {result.delta:F6}");
        Console.WriteLine($"gamma: {result.gamma:F6}");
        Console.WriteLine($"vega (per 1.0 (100%) change in volatility): {result.vega:F6}");
        Console.WriteLine($"theta (annualized): {result.theta:F6}");
        Console.WriteLine($"rho (per 1.0 (100%) change in interest): {result.rho:F6}");
    }
}