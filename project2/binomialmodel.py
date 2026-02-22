import numpy as np

def binomial_price(S, K, T, r, n, q, sigma, type: str, style: str = 'european'):
    '''
    this function will calculate the price using a binomial model

    inputs: 
        - S: initial stock price
        - K: strike price
        - T: total time to maturity
        - r: risk free rate
        - n: number of steps
        - q: continuous dividend yield
        - sigma: volatility
        - type: 'call' or 'put'
        - style: 'european' or 'american'

    '''

    dt = T / n
    u = np.exp(sigma * np.sqrt(dt))
    d = 1 / u

    # risk neutral probability of an up move
    p = (np.exp((r - q) * dt) - d) / (u - d)
    discount = np.exp(-(r - q) * dt)
    
    def option_value(S_current, step):
        # calculate the payoffs when we reach the terminal node
        if step == n:
            if type == 'call':
                return max(S_current - K, 0)
            elif type == 'put':
                return max(K - S_current, 0)
        
        # calculate the value at the current node 
        up_value = option_value(S_current * u, step + 1)
        down_value = option_value(S_current * d, step + 1)

        # expected discounted value
        continuation_value = discount * (p * up_value + (1 - p) * down_value)
        
        # for american options, check if early exercise is better
        if style == 'american':
            if type == 'call':
                intrinsic_value = max(S_current - K, 0)
            elif type == 'put':
                intrinsic_value = max(K - S_current, 0)
            # compare the intrinsic value to the discounted values 
            return max(continuation_value, intrinsic_value)
        
        return continuation_value
    
    # start the recursion at step 0 using the initial stock price 
    return option_value(S, 0)


def binomial_delta(S, K, T, r, n, q, sigma, type: str, style: str = 'european'):
    # first derivative with respect to a change in the stock price 

    dt = T / n
    u = np.exp(sigma * np.sqrt(dt))
    d = 1 / u
    
    # find the stock prices a step after the initial price 
    S_1_1 = S * u
    S_1_0 = S * d

    # find the value of the option at two nodes 
    V_1_1 = binomial_price(S_1_1, K, T - dt, r, n - 1, q, sigma, type, style)
    V_1_0 = binomial_price(S_1_0, K, T - dt, r, n - 1, q, sigma, type, style)
    
    # use finite difference method to calculate delta 
    return (V_1_1 - V_1_0) / (S_1_1 - S_1_0)


def binomial_gamma(S, K, T, r, n, q, sigma, type: str, style: str = 'european'):
    # second derivative with respect to stock price

    dt = T / n
    u = np.exp(sigma * np.sqrt(dt))
    d = 1 / u
    
    # find the prices at two steps 
    S_2_2 = S * u * u
    S_2_1 = S * u * d
    S_2_0 = S * d * d
    
    # option prices at two steps in 
    V_2_2 = binomial_price(S_2_2, K, T - 2*dt, r, n - 2, q, sigma, type, style)
    V_2_1 = binomial_price(S_2_1, K, T - 2*dt, r, n - 2, q, sigma, type, style)
    V_2_0 = binomial_price(S_2_0, K, T - 2*dt, r, n - 2, q, sigma, type, style)
    
    # approximate delta
    gamma_up = (V_2_2 - V_2_1) / (S_2_2 - S_2_1)
    gamma_down = (V_2_1 - V_2_0) / (S_2_1 - S_2_0)
    
    # finite difference approximation of gamma
    return (gamma_up - gamma_down) / (0.5 * (S_2_2 - S_2_0))


def binomial_vega(S, K, T, r, n, q, sigma, type: str, style: str = 'european'):
    # derivative with respect to volatility for 1% change

    delta_sigma = 0.0001
    V_plus = binomial_price(S, K, T, r, n, q, sigma + delta_sigma, type, style)
    V_minus = binomial_price(S, K, T, r, n, q, sigma - delta_sigma, type, style)
    
    # find vega by repricing the same option with slightly different volatilities
    # multiply by 100 to get vega for 1% change in volatility
    return (V_plus - V_minus) / (2 * delta_sigma) * 0.01


def binomial_theta(S, K, T, r, n, q, sigma, type: str, style: str = 'european'):
    # measure how the option value changes with respect to time per day
    
    delta_t = 1/365  # 1 day in years
    V_plus = binomial_price(S, K, T - delta_t, r, n, q, sigma, type, style)
    V_0 = binomial_price(S, K, T, r, n, q, sigma, type, style)
    
    return V_plus - V_0


def binomial_rho(S, K, T, r, n, q, sigma, type: str, style: str = 'european'):
    # measure how the option value changes when interest rates change for 1% change
    delta_r = 0.0001
    V_plus = binomial_price(S, K, T, r + delta_r, n, q, sigma, type, style)
    V_minus = binomial_price(S, K, T, r - delta_r, n, q, sigma, type, style)
    
    # reprice the same option with slightly different interest rates
    # multiply by 100 to get rho for 1% change in interest rate
    return (V_plus - V_minus) / (2 * delta_r) * 0.01

def binomial_model(S, K, T, r, n, q, sigma, type: str, style: str = 'european'):
    # this function computes the price + all the greeks 

    price = binomial_price(S, K, T, r, n, q, sigma, type, style)
    delta = binomial_delta(S, K, T, r, n, q, sigma, type)
    gamma = binomial_gamma(S, K, T, r, n, q, sigma, type)
    vega = binomial_vega(S, K, T, r, n, q, sigma, type)
    theta = binomial_theta(S, K, T, r, n, q, sigma, type)
    rho = binomial_rho(S, K, T, r, n, q, sigma, type)

    print("price:", price)
    print("delta:", delta)
    print("gamma:", gamma)
    print("vega:", vega)
    print("theta:", theta)
    print("rho:", rho)
