Explanation of files in this project:

blackscholes.py
- contains the black scholes formula for pricing calls and puts
- contains d1, d2, and greek functions (including vega)

root.py
- implements both bisecion and newton method 
- used to support the calculation of implied volatility

impliedvolatility.py
- contains functions for calculating implied volatility from market prices assuming black scholes

impliedvolatility.ipynb
- contains demonstrations on how to use the implied volatility functions
- implements examples using market data on nvda options

data.csv 
- contains option data (strikes, prices, and type) for nvda

gatheral.py
- contains functions for working a gatheral svi model
- computes total implied volatility given parameters
- fits svi parameters

gatheral.ipynb
- demonstrates how to fit gatheral svi volatility surface to market data
- uses nvda options to estimate svi parameters and plot the skew