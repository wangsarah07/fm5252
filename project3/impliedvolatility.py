from blackscholes import call_price, put_price, vega
from root import bisection, newton

def impliedvol_bisection(S, K, r, T, price, type):
    '''
    find the implied volatility using black scholes and the bisection method
        
        - S: initial stock price
        - K: strike price
        - r: risk free rate
        - T: total time to maturity
        - price: market price of option
        - type: 'call' or 'put'
    '''

    # difference between the black scholes price and the market price 
    def price_diff(sigma): 
        if type == 'call':
            # we want this difference to be 0
            return call_price(K, S, T, sigma, r) - price
        if type == 'put':
            return put_price(K, S, T, sigma, r) - price
        
    try:
        # run bisection to find sigma where the price difference = 0 
        return bisection(price_diff, a = 1e-6, b = 5.0, tol = 1e-6, max_iters = 100)
    
    except RuntimeError as e:
        print("bisection failed too many iterations")
        return None

def impliedvol_newton(S, K, r, T, price, type):
    '''
    find the implied volatility using black scholes and newton's method

        - S: initial stock price
        - K: strike price
        - r: risk free rate
        - T: total time to maturity
        - price: market price of option
        - type: 'call' or 'put'
    '''

    # difference between the black scholes price and the market price
    def price_diff(sigma):
        if type == 'call':
            # we want this difference to be 0
            return call_price(K, S, T, sigma, r) - price
        if type == 'put':
            return put_price(K, S, T, sigma, r) - price
    
    def price_vega(sigma):
        # vega is the first derivative with respect to volatility 
        return vega(K, S, T, sigma, r)
    
    try:
        # run newton's method to find sigma where the price difference = 0
        return newton(price_diff, price_vega, x0 = 0.5, tol = 1e-6, max_iters = 100)
    
    except RuntimeError as e:
        print("newton failed too many iterations")
        return None
