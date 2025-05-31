using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EZTM.Common;

namespace Tests
{
    [TestClass()]
    public class ImpliedVolatilityCalculatorTests
    {
        [TestMethod()]
        public void CalculateImpliedVolatilityTest()
        {
            double S = 400; // Underlying asset price
            double K = 490; // Strike price
            int daysToExpiration = 50; // Days to expiration
            double r = 0.04; // Risk-free interest rate
            double C = 9.28; // Market price of the option
            double tolerance = 1e-5; // Tolerance for convergence
            double result = ImpliedVolatilityCalculator.CalculateImpliedVolatility(S, K, daysToExpiration, r, C, tolerance: tolerance);

            double delta = ImpliedVolatilityCalculator.Delta(S, K, daysToExpiration, r, result);
            Assert.IsTrue(Math.Abs(result - 0.2) < tolerance);


        }
    }
}