// this is the simulation engine for pricing options
// it simulates asset price paths using geometric brownian motion
// computes option prices as a discounted expected payoff
// estimates greeks using finite differences
// computes the standard error of the simulation
// the engine takes an option object as input and performs all the work
// it does not handle user input or output

using System;

public class Engine
{
    // store option parameters and simulation settings
    // readonly: values are fixed after the constructor runs 
    private readonly Option _option;
    private readonly int _numPaths;
    private readonly int _numSteps;

    public Engine(Option option, int numPaths, int numSteps)
    {
        _option = option;
        _numPaths = numPaths;
        _numSteps = numSteps;
    }

    // pricing method that runs the monte carlo simulation 
    public (double price, double stdErr,
            double delta, double gamma,
            double vega, double theta, double rho) RunSimulation()
    {
        // break time to maturity into smaller steps 
        double dt = _option.T / _numSteps;
        double sqrtDt = Math.Sqrt(dt);

        // risk neutral drift term from geometric brownian motion
        double drift = (_option.R - 0.5 * _option.Sigma * _option.Sigma) * dt;

        // finite difference bumps used to estimate greeks
        // 1% bump in stock price
        double dS = _option.S * 0.01;
        // 1% bump in volatility
        double dSig = _option.Sigma * 0.01;
        // bump used for calculating rho
        double dr = 0.0001;
        // bump used for calculating theta
        double dT = 1.0 / 365.0;

        // store payoffs from each monte carlo path
        // each index is one simulated path 
        double[] payoffs = new double[_numPaths];

        // simulations for finite difference approximations 
        double[] payoffsUp = new double[_numPaths];
        double[] payoffsDown = new double[_numPaths];
        double[] payoffsVega = new double[_numPaths];
        double[] payoffsRhoUp = new double[_numPaths];
        double[] payoffsRhoDown = new double[_numPaths];
        double[] payoffsTheta = new double[_numPaths];    

        // monte carlo loop simulates many independent paths 
        // each iteration simulates one full path 
        for (int i = 0; i < _numPaths; i++)
        {
            // generate one sequence of random normal shocks 
            // this will be reused for all bumps
            double[] zPath = new double[_numSteps];

            for (int step = 0; step < _numSteps; step++)
            {
                var (z, _) = RandomGenerators.BoxMuller();
                zPath[step] = z;
            }

            // simulate original option value 
            payoffs[i] = SimulatePath(_option.S, _option.Sigma, dt, drift, zPath);

            // bump stock price to calculate delta and gamma
            // these paths use the same random shocks 
            payoffsUp[i] = SimulatePath(_option.S + dS, _option.Sigma, dt, drift, zPath);
            payoffsDown[i] = SimulatePath(_option.S - dS, _option.Sigma, dt, drift, zPath);

            // change sigma to calculate vega 
            double sigmaUp = _option.Sigma + dSig;
            // drift will also change because it depends on sigma 
            double driftVega = (_option.R - 0.5 * sigmaUp * sigmaUp) * dt;

            // simulate a path with the new sigma 
            payoffsVega[i] = SimulatePath(_option.S, sigmaUp, dt, driftVega, zPath);

            // change the interest rate to calculate rho
            // this affects the drift and the discounting factor 
            double driftUp = ((_option.R + dr) - 0.5 * _option.Sigma * _option.Sigma) * dt;
            double driftDown = ((_option.R - dr) - 0.5 * _option.Sigma * _option.Sigma) * dt;

            // simulate paths that implement the interest rate changes 
            payoffsRhoUp[i] = SimulatePath(_option.S, _option.Sigma, dt, driftUp, zPath);
            payoffsRhoDown[i] = SimulatePath(_option.S, _option.Sigma, dt, driftDown, zPath);

            // find theta by reducing the time to maturity
            if (_option.T > dT)
            {

                double newT = _option.T - dT;
                // recompute dt since total time has changed 
                double dtTheta = newT / _numSteps;
                // recalculate the drift because it depends on dt
                double driftTheta = (_option.R - 0.5 * _option.Sigma * _option.Sigma) * dtTheta;
                // simulate paths with the new dt 
                payoffsTheta[i] = SimulatePath(_option.S, _option.Sigma, dtTheta, driftTheta, zPath);
            }
        }

        // discount expected value to present 
        double discount = Math.Exp(-_option.R * _option.T);

        double price = discount * Mean(payoffs);
        double priceUp = discount * Mean(payoffsUp);
        double priceDown = discount * Mean(payoffsDown);
        double priceVega = discount * Mean(payoffsVega);

        // calculate greeks using the finite difference formulas 
        double delta = (priceUp - priceDown) / (2 * dS);
        double gamma = (priceUp - 2 * price + priceDown) / (dS * dS);
        double vega = (priceVega - price) / dSig;
        double priceRhoUp = Math.Exp(-(_option.R + dr) * _option.T) * Mean(payoffsRhoUp);
        double priceRhoDown = Math.Exp(-(_option.R - dr) * _option.T) * Mean(payoffsRhoDown);
        double rho = (priceRhoUp - priceRhoDown) / (2 * dr);
        double theta = 0;
        if (_option.T > dT)
        {
            double thetaPrice = Math.Exp(-_option.R * (_option.T - dT)) * Mean(payoffsTheta);
            theta = (thetaPrice - price) / dT;
        }

        // calculate standard error of the simulation
        // SE = SD(payoffs) / sqrt(M)
        double stdErr = discount * StdDev(payoffs) / Math.Sqrt(_numPaths);

        return (price, stdErr, delta, gamma, vega, theta, rho);
    }

    // simulate geometric brownian motion path
    private double SimulatePath(double S0, double sigma, double dt, double drift, double[] zPath)
    {
        double S = S0;

        for (int step = 0; step < _numSteps; step++)
        {
            // each step has a random shock
            S *= Math.Exp(drift + sigma * Math.Sqrt(dt) * zPath[step]);
        }

        // find payoff at maturity (depends only on the final price)
        if (_option.Type == "call")
            return Math.Max(S - _option.K, 0);
        else
            return Math.Max(_option.K - S, 0);
    }

    // calculate the mean of an array 
    private double Mean(double[] arr)
    {
        double sum = 0;
        foreach (double v in arr) sum += v;
        return sum / arr.Length;
    }

    // calculate sample standard deviation
    private double StdDev(double[] arr)
    {
        double m = Mean(arr);
        double sumSq = 0;

        foreach (double v in arr)
            sumSq += (v - m) * (v - m);

        return Math.Sqrt(sumSq / (arr.Length - 1));
    }
}