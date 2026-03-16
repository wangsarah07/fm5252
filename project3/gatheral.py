import numpy as np
import scipy.optimize
from impliedvolatility import impliedvol_newton

def svi_raw(k, a, b, rho, m, sigma):
    # calculates the total implied variance
    return a + b * (rho * (k - m) + np.sqrt((k - m) ** 2 + sigma ** 2))

def fit_svi(strikes, implied_vols, T, S):
    """
    fits svi parameters to the implied volatility data

    inputs: 
        - strikes: array of strike prices
        - implied_vols: array of implied volatilities
        - T: time to maturity
        - S: current stock price
    
    return: 
        - params: [a, b, rho, m, sigma]
    """

    # convert to log moneyness
    k = np.log(strikes / S)
    
    # convert implied vols to total variance
    w_market = (implied_vols ** 2) * T
    
    # sum of squared errors
    def objective(params):
        a, b, rho, m, sigma = params

        # enforce parameter constraints
        if b < 0 or sigma <= 0 or abs(rho) >= 1:
            return 1e10  # large penalty if constraints violated

        w_fit = svi_raw(k, a, b, rho, m, sigma)
        return np.sum((w_fit - w_market) ** 2)
    
    # create initial guesses & optimize
    x0 = [0.04, 0.1, -0.4, 0.0, 0.1]
    result = scipy.optimize.fmin(func = objective, x0 = x0, disp = False)
    
    return result

def svi_variance(strike, params, T, S):
    # calculate total implied variance for a given strike using svi parameters    
    k = np.log(strike / S)
    a, b, rho, m, sigma = params
    return svi_raw(k, a, b, rho, m, sigma)

def svi_volatility(strike, params, T, S):
    
    # calculate implied volatility for a given strike using svi parameters
    w = svi_variance(strike, params, T, S)
    return np.sqrt(w / T)
