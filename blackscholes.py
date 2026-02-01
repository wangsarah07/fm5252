import numpy as np
import matplotlib.pyplot as plt
from scipy.stats import norm

d1 = lambda K, S, t, sigma, r: (np.log(S / K) + t * (0.5 * sigma ** 2 + r)) / (sigma * np.sqrt(t))
d2 = lambda K, S, t, sigma, r: d1(K, S, t, sigma, r) - sigma * np.sqrt(t)

call_price = lambda K, S, t, sigma, r: (S * norm.cdf(d1(K, S, t, sigma, r)) - K * np.exp(-r * t) * norm.cdf(d2(K, S, t, sigma, r)))
put_price = lambda K, S, t, sigma, r: (K * np.exp(-r * t) * norm.cdf(-d2(K, S, t, sigma, r)) - S * norm.cdf(-d1(K, S, t, sigma, r)))

# call measures the change in the option price given a change in the underlying
call_delta = lambda K, S, t, sigma, r: norm.cdf(d1(K, S, t, sigma, r))
put_delta = lambda K, S, t, sigma, r: norm.cdf(d1(K, S, t, sigma, r)) - 1

# gamma is the partial derivative of delta with respect to the underlying price
gamma = lambda K, S, t, sigma, r: (norm.pdf(d1(K, S, t, sigma, r)) / (S * sigma * np.sqrt(t)))

# vega is the partial derivative of the option price with respect to volatility
vega = lambda K, S, t, sigma, r: (S * norm.pdf(d1(K, S, t, sigma, r)) * np.sqrt(t))

# theta measures how the price of an option changes as time passes
call_theta = lambda K, S, t, sigma, r: (-norm.pdf(d1(K, S, t, sigma, r)) * sigma * S / (2 * np.sqrt(t)) - r * K * np.exp(-r * t) * norm.cdf(d2(K, S, t, sigma, r)))
put_theta = lambda K, S, t, sigma, r: (-norm.pdf(d1(K, S, t, sigma, r)) * sigma * S / (2 * np.sqrt(t)) + r * K * np.exp(-r * t) * norm.cdf(-d2(K, S, t, sigma, r)))

# rho measures how the prices change as interest rates change 
call_rho = lambda K, S, t, sigma, r: (K * t * np.exp(-r * t) * norm.cdf(d2(K, S, t, sigma, r)))
put_rho = lambda K, S, t, sigma, r: (-K * t * np.exp(-r * t) * norm.cdf(-d2(K, S, t, sigma, r)))

# inputs used in the graphs below 
S = 100
t = 1
sigma = 0.2
r = 0.05

# creates 400 strike prices between 50 and 150
K = np.linspace(50, 150, 400)

# creates 400 underlying prices between 50 and 150
S_grid = np.linspace(50, 150, 400)

# creates 400 volatility values between 5% and 60%
sigma_grid = np.linspace(0.05, 0.6, 400)

# call price vs strike
plt.figure()
plt.plot(K, call_price(K, S, t, sigma, r))
plt.xlabel("strike price")
plt.ylabel("call price")
plt.show()

# put price vs strike
plt.figure()
plt.plot(K, put_price(K, S, t, sigma, r))
plt.xlabel("strike price")
plt.ylabel("put price")
plt.show()

# gamma vs underlying price
plt.figure()
plt.plot(S_grid, gamma(K = 100, S = S_grid, t = t, sigma = sigma, r = r))
plt.xlabel("underlying price")
plt.ylabel("gamma")
plt.show()

# vega vs volatility
plt.figure()
plt.plot(sigma_grid, vega(K = 100, S = 100, t = t, sigma = sigma_grid, r = r))
plt.xlabel("volatility")
plt.ylabel("vega")
plt.show()

# call delta vs underlying price
plt.figure()
plt.plot(S_grid, call_delta(K = 100, S = S_grid, t = t, sigma = sigma, r = r))
plt.xlabel("underlying price")
plt.ylabel("call delta")
plt.show()