// when running this program, make sure your terminal is in the folder titled project4
// in the terminal, use dotnet run, then follow the commands 

using System;

class Program
{
    static void Main()
    {
        // ask user how many samples to generate
        Console.WriteLine("how many samples u want?");
        int n = int.Parse(Console.ReadLine()!);

        // ask user for correlation value rho 
        Console.WriteLine("enter correlation value rho [-1, 1]:");
        double rho = double.Parse(Console.ReadLine()!);

        // generate n samples using sum twelve
        Console.WriteLine($"sum twelve ({n} samples)");
        for (int i = 0; i < n; i++)
        {
            // call Sum12 to get one normally distributed value
            double value = RandomGenerators.Sum12();
            Console.WriteLine($"  {value:F6}");
        }

        // generate n pairs using box muller
        Console.WriteLine($"box muller ({n} pairs)");
        for (int i = 0; i < n; i++)
        {
            // generate two independent normal values
            double z1, z2;
            (z1, z2) = RandomGenerators.BoxMuller();

            // print the two numbers in the pair
            Console.WriteLine($"  z1={z1:F6}  z2={z2:F6}");
        }

        // generate n pairs using polar rejection
        Console.WriteLine($" polar rejection ({n} pairs)");
        for (int i = 0; i < n; i++)
        {
            // generate a pair using polar rejection
            double z1, z2;
            (z1, z2) = RandomGenerators.PolarRejection();
            Console.WriteLine($"  z1={z1:F6}  z2={z2:F6}");
        }

        // generate n correlated pairs using rho 
        Console.WriteLine($"correlated pairs (rho={rho}, {n} pairs)");
        for (int i = 0; i < n; i++)
        {
            // generate a pair with the specified correlation
            double z1, z2;
            (z1, z2) = RandomGenerators.CorrelatedPair(rho);
            Console.WriteLine($"  z1={z1:F6}  z2={z2:F6}");
        }
    }
}
