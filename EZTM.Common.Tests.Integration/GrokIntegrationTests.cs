using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EZTM.Common.Tests
{
    [TestClass]
    public class GrokIntegrationTests
    {
        [TestMethod]
        public async Task PostAsync_WithShortStranglePayload_ReturnsSuccess()
        {
            // Arrange
            var grok = new Grok(); // Replace with your actual API key
            //var grok = new Grok("your_api_key"); // Replace with your actual API key
            var payload = new
            {
                model = "grok-3",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You are a financial analyst specializing in options trading. Use the provided AAPL options chain data to design a Short Strangle strategy. Assume delta approximates the probability of being in-the-money at expiration."
                    },
                    new
                    {
                        role = "user",
                        content = @"Using the following AAPL options chain data for expiration June 6, 2025 (6 days away, underlying price $200.355), create a Short Strangle with an ~80% chance of not being assigned (i.e., stock price stays between strikes). Select OTM call and put strikes with deltas ~0.10–0.20. Provide: strike prices, premiums (bid prices), total credit, breakeven points, approximate probability of success (based on deltas), and max loss. Data summary (strike, bid, ask, delta):
Calls:
- $207.5: $0.81, $0.85, 0.200
- $210.0: $0.44, $0.47, 0.123
- $212.5: $0.24, $0.27, 0.075
Puts:
- $190.0: $0.49, $0.52, -0.110
- $192.5: $0.77, $0.80, -0.163
- $195.0: $1.21, $1.25, -0.238
Explain calculations."
                    }
                },
                temperature = 0.2
            };

            // Act
            var result = await grok.PostAsync("chat/completions", payload);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            StringAssert.Contains(result, "Short Strangle", StringComparison.OrdinalIgnoreCase);
            StringAssert.Contains(result, "strike", StringComparison.OrdinalIgnoreCase);
            StringAssert.Contains(result, "premium", StringComparison.OrdinalIgnoreCase);
        }
    }
}
