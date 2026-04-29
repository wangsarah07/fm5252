// this is a data container for the option parameters
// it stores: option type (call or put), stock price, strike price, time to maturity, risk free rate and volatility

public class Option
{

    public string Type {get;} 
    public double S {get;} 
    public double K {get;}
    public double T {get;} 
    public double R {get;} 
    public double Sigma {get;} 

    public Option(string type, double s, double k, double t, double r, double sigma)
    {
        Type = type;
        S = s;
        K = k;
        T = t;
        R = r;
        Sigma = sigma;
    }
}