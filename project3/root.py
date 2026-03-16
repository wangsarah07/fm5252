def bisection(f, a, b, tol = 1e-6, max_iters = 50):
    iterations = 0

    # repeat until we get within tolerance or reach maximum iterations
    while (abs(b - a) > tol) and (iterations < max_iters):
        mid = (b + a) / 2

        # the root lies between a and mid because the function changes signs in that range
        if (f(a) * f(mid)) < 0:
            b = mid

        # else the root lies between mid and b 
        elif (f(b) * f(mid)) < 0:
            a = mid 

        iterations += 1

    if iterations >= max_iters:
        raise RuntimeError("too many iterations lol")

    # return midpoint as the best guess for the root
    return (a + b) / 2





def newton(f, fp, x0, tol = 1e-6, max_iters = 50):
    x = x0

    for _ in range(max_iters):
        fx = f(x)
        fpx = fp(x)

        # once within tolerance, return x
        if abs(fx) < tol:
            return x
        
        # if not, use derivatives to get closer to root
        # if vega is zero (deep in or out of the money), bail out early to avoid divide-by-zero
        if abs(fpx) < 1e-10:
            raise RuntimeError("too many iterations lol")

        x = x - (fx / fpx)

    # if the for loop ends, we have exceeded maximum iterations
    raise RuntimeError("too many iterations lol")