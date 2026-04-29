// this is the simulation engine for pricing options
// it simulates asset price paths using geometric brownian motion
// computes option prices as a discounted expected payoff
// estimates greeks using finite differences
// computes the standard error of the simulation (not for van der corput)
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
    private readonly bool _useAntithetic;
    private readonly bool _useVanDerCorput;
    private readonly int _base1;
    private readonly int _base2;

    // fixed number of simulations used by van der corput 
    private const int VdcNumPaths = 10000;

    public Engine(Option option, int numPaths, int numSteps, bool useAntithetic, bool useVanDerCorput, int base1, int base2)
    {
        _option = option;
        _numPaths = numPaths;
        _numSteps = numSteps;
        _useAntithetic = useAntithetic;
        _useVanDerCorput = useVanDerCorput;
        _base1 = base1;
        _base2 = base2;
    }

    // pricing method that runs the monte carlo simulation
    // returns null standard error when using van der corput 
    public (double price, double? stdErr, double delta, double gamma, double vega, double theta, double rho) RunSimulation()
    {
        return _useVanDerCorput ? RunVdcSimulation() : RunStandardSimulation();
    }

    // method without using van der corput
    private (double price, double? stdErr, double delta, double gamma, double vega, double theta, double rho) RunStandardSimulation()
    {
        
        // time step size 
        double dt = _option.T / _numSteps;

        // drift term for gbm 
        double drift = (_option.R - 0.5 * _option.Sigma * _option.Sigma) * dt;

        // finite bump sizes used for computing greeks
        double dS = _option.S * 0.01;
        double dSig = _option.Sigma * 0.01;
        double dr = 0.0001;
        double dT = 1.0 / 365.0;

        // arrays will store payoffs for averaging at the end 
        double[] payoffs = new double[_numPaths];
        double[] payoffsUp = new double[_numPaths];
        double[] payoffsDown = new double[_numPaths];
        double[] payoffsVega = new double[_numPaths];
        double[] payoffsRhoUp = new double[_numPaths];
        double[] payoffsRhoDn = new double[_numPaths];
        double[] payoffsTheta = new double[_numPaths];

        // simulate monte carlo paths 
        for (int i = 0; i < _numPaths; i++)
        {
            // generate brownian shocks for one path 
            double[] zPath = new double[_numSteps];
            for (int step = 0; step < _numSteps; step++)
            {
                var (z, _) = RandomGenerators.BoxMuller();
                zPath[step] = z;
            }

            // antithetic mirrored shocks 
            double[] zAnti = new double[_numSteps];
            if (_useAntithetic)
                for (int step = 0; step < _numSteps; step++)
                    zAnti[step] = -zPath[step];

            payoffs[i] = AveragePaths(_option.S, _option.Sigma, dt, drift, zPath, zAnti);

            // calculate delta and gamma by bumping the stock price 
            payoffsUp[i] = AveragePaths(_option.S + dS, _option.Sigma, dt, drift, zPath, zAnti);
            payoffsDown[i] = AveragePaths(_option.S - dS, _option.Sigma, dt, drift, zPath, zAnti);

            // calculate vega by bumping volatility 
            double sigmaUp = _option.Sigma + dSig;
            double driftVega = (_option.R - 0.5 * sigmaUp * sigmaUp) * dt;
            payoffsVega[i] = AveragePaths(_option.S, sigmaUp, dt, driftVega, zPath, zAnti);

            // calculate rho by changing the interet rate 
            double driftRhoUp = ((_option.R + dr) - 0.5 * _option.Sigma * _option.Sigma) * dt;
            double driftRhoDn = ((_option.R - dr) - 0.5 * _option.Sigma * _option.Sigma) * dt;
            payoffsRhoUp[i] = AveragePaths(_option.S, _option.Sigma, dt, driftRhoUp, zPath, zAnti);
            payoffsRhoDn[i] = AveragePaths(_option.S, _option.Sigma, dt, driftRhoDn, zPath, zAnti);

            // find theta by shifting the time to maturity 
            if (_option.T > dT)
            {
                double newT = _option.T - dT;
                double dtTheta = newT / _numSteps;
                double driftTheta = (_option.R - 0.5 * _option.Sigma * _option.Sigma) * dtTheta;
                payoffsTheta[i] = AveragePaths(_option.S, _option.Sigma, dtTheta, driftTheta, zPath, zAnti);
            }
        }

        // discount all expected payoffs to present value 
        double discount = Math.Exp(-_option.R * _option.T);

        double price = discount * Mean(payoffs);
        double priceUp = discount * Mean(payoffsUp);
        double priceDown = discount * Mean(payoffsDown);
        double priceVega = discount * Mean(payoffsVega);

        double delta = (priceUp - priceDown) / (2 * dS);
        double gamma = (priceUp - 2 * price + priceDown) / (dS * dS);
        double vega = (priceVega - price) / dSig;

        double priceRhoUp = Math.Exp(-(_option.R + dr) * _option.T) * Mean(payoffsRhoUp);
        double priceRhoDn = Math.Exp(-(_option.R - dr) * _option.T) * Mean(payoffsRhoDn);
        double rho = (priceRhoUp - priceRhoDn) / (2 * dr);

        double theta = 0;
        if (_option.T > dT)
        {
            double thetaPrice = Math.Exp(-_option.R * (_option.T - dT)) * Mean(payoffsTheta);
            theta = (thetaPrice - price) / dT;
        }

        double stdErr = discount * StdDev(payoffs) / Math.Sqrt(_numPaths);

        return (price, stdErr, delta, gamma, vega, theta, rho);
    }

    // van der corput simulation
    private (double price, double? stdErr, double delta, double gamma, double vega, double theta, double rho) RunVdcSimulation()
    {
        // we only use one step 
        double dt = _option.T;

        // finite difference bumps for calculating greeks 
        double dS = _option.S * 0.01;
        double dSig = _option.Sigma * 0.01;
        double dr = 0.0001;
        double dT = 1.0 / 365.0;

        // arrays will store payoffs for averaging at the end 
        double[] payoffs = new double[VdcNumPaths];
        double[] payoffsUp = new double[VdcNumPaths];
        double[] payoffsDown = new double[VdcNumPaths];
        double[] payoffsVega = new double[VdcNumPaths];
        double[] payoffsRhoUp = new double[VdcNumPaths];
        double[] payoffsRhoDn = new double[VdcNumPaths];
        double[] payoffsTheta = new double[VdcNumPaths];

        for (int i = 0; i < VdcNumPaths; i++)
        {
    
            // generate using van der corput
            var (z, _) = RandomGenerators.BoxMullerVanDerCorput(i + 1, _base1, _base2);

            double drift = (_option.R - 0.5 * _option.Sigma * _option.Sigma) * dt;

            // payoff evaluation
            payoffs[i] = SimulateOneStep(_option.S, _option.Sigma, dt, drift, z);

            payoffsUp[i] = SimulateOneStep(_option.S + dS, _option.Sigma, dt, drift, z);
            payoffsDown[i] = SimulateOneStep(_option.S - dS, _option.Sigma, dt, drift, z);

            double sigmaUp = _option.Sigma + dSig;
            double driftVega = (_option.R - 0.5 * sigmaUp * sigmaUp) * dt;
            payoffsVega[i] = SimulateOneStep(_option.S, sigmaUp, dt, driftVega, z);

            double driftRhoUp = ((_option.R + dr) - 0.5 * _option.Sigma * _option.Sigma) * dt;
            double driftRhoDn = ((_option.R - dr) - 0.5 * _option.Sigma * _option.Sigma) * dt;
            payoffsRhoUp[i] = SimulateOneStep(_option.S, _option.Sigma, dt, driftRhoUp, z);
            payoffsRhoDn[i] = SimulateOneStep(_option.S, _option.Sigma, dt, driftRhoDn, z);

            if (_option.T > dT)
            {
                double newT = _option.T - dT;
                double driftTheta = (_option.R - 0.5 * _option.Sigma * _option.Sigma) * newT;
                payoffsTheta[i] = SimulateOneStep(_option.S, _option.Sigma, newT, driftTheta, z);
            }
        }

        double discount = Math.Exp(-_option.R * _option.T);

        double price = discount * Mean(payoffs);
        double priceUp = discount * Mean(payoffsUp);
        double priceDown = discount * Mean(payoffsDown);
        double priceVega = discount * Mean(payoffsVega);

        double delta = (priceUp - priceDown) / (2 * dS);
        double gamma = (priceUp - 2 * price + priceDown) / (dS * dS);
        double vega = (priceVega - price) / dSig;

        double priceRhoUp = Math.Exp(-(_option.R + dr) * _option.T) * Mean(payoffsRhoUp);
        double priceRhoDn = Math.Exp(-(_option.R - dr) * _option.T) * Mean(payoffsRhoDn);
        double rho = (priceRhoUp - priceRhoDn) / (2 * dr);

        double theta = 0;
        if (_option.T > dT)
        {
            double thetaPrice = Math.Exp(-_option.R * (_option.T - dT)) * Mean(payoffsTheta);
            theta = (thetaPrice - price) / dT;
        }

        // no standard error for using van der corput
        return (price, null, delta, gamma, vega, theta, rho);
    }

    // simulate one gbm step and return the payoff
    private double SimulateOneStep(double S0, double sigma, double dt, double drift, double z)
    {
        double S = S0 * Math.Exp(drift + sigma * Math.Sqrt(dt) * z);

        if (_option.Type == "call")
        {return Math.Max(S - _option.K, 0);}
        else
        {return Math.Max(_option.K - S, 0);}
    }

    // simulate a full gbm path and return the payoff
    private double SimulatePath(double S0, double sigma, double dt, double drift, double[] zPath)
    {
        double S = S0;
        for (int step = 0; step < _numSteps; step++)
            S *= Math.Exp(drift + sigma * Math.Sqrt(dt) * zPath[step]);

        if (_option.Type == "call")
        {return Math.Max(S - _option.K, 0);}
        else
        {return Math.Max(_option.K - S, 0);}
    }

    // returns the antithetic average if enabled, otherwise just the base payoff
    private double AveragePaths(double S0, double sigma, double dt, double drift, double[] zPath, double[] zAnti)
    {
        double p1 = SimulatePath(S0, sigma, dt, drift, zPath);
        if (!_useAntithetic) return p1;

        double p2 = SimulatePath(S0, sigma, dt, drift, zAnti);
        return 0.5 * (p1 + p2);
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
