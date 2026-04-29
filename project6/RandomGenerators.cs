using System;

public static class RandomGenerators

// static class for generating random numbers
// this contains methods for normal and gaussian random variables 

{
    // random number generator object 
    private static readonly Random _rng = new Random();

    // generate uniform random variable on [0, 1)
    private static double U() => _rng.NextDouble();

    // generate uniform random variable on (-1, 1)
    private static double U11() => _rng.NextDouble() * 2.0 - 1.0;

    // sum twelve method generates a single gaussian random variable using sum of 12 uniforms
    public static double Sum12()
    {
        double sum = 0;

        // add 12 uniform random numbers 
        for (int i = 0; i < 12; i++) sum += U();

        // subtract 6 to center around 0
        return sum - 6.0;
    }

    // box muller generates a pair of gaussian random variables from two uniforms 
    public static (double z1, double z2) BoxMuller()
    {
        double x1 = U();
        double x2 = U();

        // radius of polar coordinates 
        double r = Math.Sqrt(-2.0 * Math.Log(x1));

        // return two normally distributed numbers 
        return (r * Math.Cos(2 * Math.PI * x2),
                r * Math.Sin(2 * Math.PI * x2));
    }

    // polar rejection method generates a pair of gaussian random variables using rejection sampling 
    public static (double z1, double z2) PolarRejection()
    {
        double x1, x2, w;

        // repeat until x1^2 + x2^2 <= 1 
        do
        {
            // x1 and x2 are uniform from (-1,1)
            x1 = U11();
            x2 = U11();

            w = x1 * x1 + x2 * x2;

        // reject points outside the unit circle 
        } while (w > 1.0);

        double c = Math.Sqrt(-2.0 * Math.Log(w) / w);

        // return transformed values 
        return (c * x1, c * x2);
    }

    // generate a correlated pair of gaussian random variables with correlation rho
    public static (double z1, double z2) CorrelatedPair(double rho)
    {
        // generate two independent normal values
        var (e1, e2) = BoxMuller();

        // apply correlation formula 
        return (e1, rho * e1 + Math.Sqrt(1 - rho * rho) * e2);
    }


    // van der corput sequence for low discrepancy sequences
    public static double VanDerCorput(int n, int baseVal)
    // generate nth number in a given base 
    {
        double vdc = 0.0;
        double denom = 1.0;

        while (n > 0)
        {
            denom *= baseVal;
            vdc += (n % baseVal) / denom;
            n /= baseVal;
        }

        return vdc;
    }

    // generate a pair of normal variables using van der corput and box muller
    public static (double z1, double z2) BoxMullerVanDerCorput(int n, int base1, int base2)
    {
        double u1 = VanDerCorput(n, base1);
        double u2 = VanDerCorput(n, base2);

        // avoid log(0)
        if (u1 <= 0.0) u1 = 1e-10;

        double r = Math.Sqrt(-2.0 * Math.Log(u1));

        return (r * Math.Cos(2 * Math.PI * u2),
                r * Math.Sin(2 * Math.PI * u2));
    }
}
