using System;

namespace EZTM.Common
{
    public class ImpliedVolatilityCalculator
    {
        /// <summary>
        /// Calculates the implied volatility of an option using the Black-Scholes model.
        /// </summary>
        /// <param name="currentStockPrice">Current price of the underlying stock (S).</param>
        /// <param name="strikePrice">Strike price of the option (K).</param>
        /// <param name="daysToExpiration">Number of days until the option expires.</param>
        /// <param name="riskFreeRate">Risk-free interest rate (r).</param>
        /// <param name="marketPrice">Market price of the option (C).</param>
        /// <param name="initialGuess">Initial guess for the implied volatility (default is 0.2).</param>
        /// <param name="tolerance">Tolerance level for convergence (default is 1e-5).</param>
        /// <param name="maxIterations">Maximum number of iterations (default is 100).</param>
        /// <returns>Implied volatility.</returns>
        public static double CalculateImpliedVolatility(double currentStockPrice, double strikePrice, int daysToExpiration, double riskFreeRate, double marketPrice, double initialGuess = 0.2, double tolerance = 1e-5, int maxIterations = 100)
        {
            double timeToExpiration = daysToExpiration / 365.0; // Convert days to years
            double volatility = initialGuess;
            for (int i = 0; i < maxIterations; i++)
            {
                double optionPrice = BlackScholesPrice(currentStockPrice, strikePrice, timeToExpiration, riskFreeRate, volatility);
                double vega = BlackScholesVega(currentStockPrice, strikePrice, timeToExpiration, riskFreeRate, volatility);

                double priceDifference = optionPrice - marketPrice;
                if (Math.Abs(priceDifference) < tolerance)
                {
                    return volatility;
                }

                volatility -= priceDifference / vega;
            }

            throw new Exception("Implied volatility did not converge");
        }

        /// <summary>
        /// Calculates the Black-Scholes price of an option.
        /// </summary>
        public static double BlackScholesPrice(double currentStockPrice, double strikePrice, double timeToExpiration, double riskFreeRate, double volatility)
        {
            double d1 = (Math.Log(currentStockPrice / strikePrice) + (riskFreeRate + 0.5 * volatility * volatility) * timeToExpiration) / (volatility * Math.Sqrt(timeToExpiration));
            double d2 = d1 - volatility * Math.Sqrt(timeToExpiration);

            return currentStockPrice * Cdf(d1) - strikePrice * Math.Exp(-riskFreeRate * timeToExpiration) * Cdf(d2);
        }

        /// <summary>
        /// Calculates the Vega of an option using the Black-Scholes model.
        /// </summary>
        public static double BlackScholesVega(double currentStockPrice, double strikePrice, double timeToExpiration, double riskFreeRate, double volatility)
        {
            double d1 = (Math.Log(currentStockPrice / strikePrice) + (riskFreeRate + 0.5 * volatility * volatility) * timeToExpiration) / (volatility * Math.Sqrt(timeToExpiration));
            return currentStockPrice * Pdf(d1) * Math.Sqrt(timeToExpiration);
        }

        /// <summary>
        /// Calculates the Delta of an option using the Black-Scholes model.
        /// </summary>
        public static double Delta(double currentStockPrice, double strikePrice, double timeToExpiration, double riskFreeRate, double volatility)
        {
            double d1 = (Math.Log(currentStockPrice / strikePrice) + (riskFreeRate + 0.5 * volatility * volatility) * timeToExpiration) / (volatility * Math.Sqrt(timeToExpiration));
            return Cdf(d1);
        }

        /// <summary>
        /// Calculates the Gamma of an option using the Black-Scholes model.
        /// </summary>
        public static double Gamma(double currentStockPrice, double strikePrice, double timeToExpiration, double riskFreeRate, double volatility)
        {
            double d1 = (Math.Log(currentStockPrice / strikePrice) + (riskFreeRate + 0.5 * volatility * volatility) * timeToExpiration) / (volatility * Math.Sqrt(timeToExpiration));
            return Pdf(d1) / (currentStockPrice * volatility * Math.Sqrt(timeToExpiration));
        }

        /// <summary>
        /// Calculates the Theta of an option using the Black-Scholes model.
        /// </summary>
        public static double Theta(double currentStockPrice, double strikePrice, double timeToExpiration, double riskFreeRate, double volatility)
        {
            double d1 = (Math.Log(currentStockPrice / strikePrice) + (riskFreeRate + 0.5 * volatility * volatility) * timeToExpiration) / (volatility * Math.Sqrt(timeToExpiration));
            double d2 = d1 - volatility * Math.Sqrt(timeToExpiration);

            double term1 = -currentStockPrice * Pdf(d1) * volatility / (2 * Math.Sqrt(timeToExpiration));
            double term2 = riskFreeRate * strikePrice * Math.Exp(-riskFreeRate * timeToExpiration) * Cdf(d2);

            return term1 - term2;
        }

        /// <summary>
        /// Calculates the Rho of an option using the Black-Scholes model.
        /// </summary>
        public static double Rho(double currentStockPrice, double strikePrice, double timeToExpiration, double riskFreeRate, double volatility)
        {
            double d1 = (Math.Log(currentStockPrice / strikePrice) + (riskFreeRate + 0.5 * volatility * volatility) * timeToExpiration) / (volatility * Math.Sqrt(timeToExpiration));
            double d2 = d1 - volatility * Math.Sqrt(timeToExpiration);

            return strikePrice * timeToExpiration * Math.Exp(-riskFreeRate * timeToExpiration) * Cdf(d2);
        }

        /// <summary>
        /// Cumulative distribution function for the standard normal distribution.
        /// </summary>
        private static double Cdf(double x)
        {
            // Constants for the approximation
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = x < 0 ? -1 : 1;
            x = Math.Abs(x) / Math.Sqrt(2.0);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return 0.5 * (1.0 + sign * y);
        }

        /// <summary>
        /// Probability density function for the standard normal distribution.
        /// </summary>
        private static double Pdf(double x)
        {
            return Math.Exp(-0.5 * x * x) / Math.Sqrt(2.0 * Math.PI);
        }
    }
}