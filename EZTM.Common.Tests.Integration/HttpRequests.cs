using EZTM.Common.Schwab;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using System.Diagnostics;
using System.Threading.Channels;

namespace EZTM.Common.Tests.Integration
{
    [TestClass]
    public class HttpRequests
    {
        private static SchwabHelper _schwabHelper;
        /// <summary>
        /// Initialize variables for the class
        /// </summary>
        /// 
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var accountInfo = Brokerage.GetAccountInfo();
            _schwabHelper = new SchwabHelper(accountInfo);

                _ = _schwabHelper.RefreshAccessToken().Result;
        }


        #region Accounts
        [TestCategory("Integration")]
        [TestMethod]
        public async Task GetAccountNumberHash()
        {
            var actual = await _schwabHelper.GetAccountNumberHash().ConfigureAwait(true);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Count > 0);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public async Task GetAccountNumbers()
        {
            var actual = await _schwabHelper.GetAccounts().ConfigureAwait(true);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Count > 0);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public async Task GetAccount()
        {
            var accountMap = await _schwabHelper.GetAccountNumberHash().ConfigureAwait(true);
            Assert.IsNotNull(accountMap);

            var actual = await _schwabHelper.GetAccountByAccountId(accountMap[0].hashValue).ConfigureAwait(true);
            Assert.IsNotNull(actual);
        }
        #endregion

        #region UserPrefernce
        [TestCategory("Integration")]
        [TestMethod]
        public async Task GetUserPreference()
        {
            var actual = await _schwabHelper.GetUserPreference().ConfigureAwait(true);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.accounts.Count > 0);
            Assert.IsTrue(actual.streamerInfo.Count > 0);
        }
        #endregion

        #region Orders
        [TestCategory("Integration")]
        [TestMethod]
        public async Task GetOrdersByAccount()
        {
            var accountMap = await _schwabHelper.GetAccountNumberHash().ConfigureAwait(true);
            Assert.IsNotNull(accountMap);
            
            var actual = await _schwabHelper.GetOrdersByAccount(accountMap[0].hashValue).ConfigureAwait(true);
            Assert.IsNotNull(actual);
            if(actual.Count == 0) Assert.Inconclusive("No Orders Present");

            var getOrder = await _schwabHelper.GetOrderByOrderId(accountMap[0].hashValue, actual[0].orderId);
            Assert.IsNotNull(getOrder);


            var getOrders = await _schwabHelper.GetOrders();
            Assert.IsNotNull(getOrders);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public async Task PlaceLimitOrder()
        {
            var order = Brokerage.CreateLimitOrder(Brokerage.BUY, "MSFT", 1, 390.0);

            var accountMap = await _schwabHelper.GetAccountNumberHash().ConfigureAwait(true);
            var actual = await _schwabHelper.PlaceOrder(accountMap[0].hashValue, order).ConfigureAwait(true);
            Assert.IsTrue(actual > 0);

            order = Brokerage.CreateLimitOrder(Brokerage.BUY, "MSFT", 1, 390.1);
            actual = await _schwabHelper.ReplaceOrder(accountMap[0].hashValue, actual.ToString(), order).ConfigureAwait(true);

            Assert.IsTrue(actual > 0);

            await _schwabHelper.CancelOrder(accountMap[0].hashValue, actual.ToString()).ConfigureAwait(true);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public async Task PlaceOCOOrder()
        {
            var order = Brokerage.CreateTriggerOcoOrder("LIMIT", "MSFT", Brokerage.BUY, 1, 400.0, 1, 450.0, 350.0);

            var accountMap = await _schwabHelper.GetAccountNumberHash().ConfigureAwait(true);
            var actual = await _schwabHelper.PlaceOrder(accountMap[0].hashValue, order).ConfigureAwait(true);
            Assert.IsTrue(actual > 0);

            await _schwabHelper.CancelOrder(accountMap[0].hashValue, actual.ToString()).ConfigureAwait(true);

        }
        #endregion

        #region MarketData
        [TestCategory("Integration")]
        [TestMethod]
        public async Task GetPriceHistory()
        {
            var actual = await _schwabHelper.GetPriceHistoryAsync("AAPL").ConfigureAwait(true);
            Assert.IsNotNull(actual);
        }
        #endregion

        [TestCategory("Integration")]
        [TestMethod]
        public async Task GetQuote()
        {
            var actual = await _schwabHelper.GetQuote("AAPL").ConfigureAwait(true);
            Assert.IsNotNull(actual);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public async Task StreamerLogin()
        {
            var sut = new SchwabStreamer(_schwabHelper);
            try
            {
                sut.ConnectSocket();
                sut.Login();
                var isLoggedIn = false;
                var iter = 0;
                while (iter < 30)
                {
                    sut.LoginInfo.Subscribe(li => isLoggedIn = true);
                    if (isLoggedIn) break;
                    await Task.Delay(1000);
                    iter++;

                }
                Assert.IsTrue(isLoggedIn);
            } finally
            {
                sut.Dispose();
            }
        }

        [TestCategory("Integration")]
        [TestMethod]
        public async Task StreamerQuote()
        {
            var sut = new SchwabStreamer(_schwabHelper);
            sut.ConnectSocket();
            sut.Login();
            var haveHeartBeat = false;

            var iter = 0;
            while (iter < 10)
            {
                sut.HeartBeat.Subscribe(hb => haveHeartBeat = true);
                if (haveHeartBeat) break;
                await Task.Delay(1000);
            }

            sut.SubscribeQuote("AAPL");


            var hasQuote = false;
            while (iter < 30)
            {
                sut.StockQuoteReceived.Subscribe(s => hasQuote = s.symbol.Equals("AAPL"));
                //if (hasQuote) break;
                await Task.Delay(1000);
            }

            Assert.IsTrue(hasQuote);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public async Task StreamerTimeAndSales()
        {
            var sut = new SchwabStreamer(_schwabHelper);
            sut.ConnectSocket();
            sut.Login();
            var haveHeartBeat = false;

            var iter = 0;
            while (iter < 10)
            {
                sut.HeartBeat.Subscribe(hb => haveHeartBeat = true);
                if (haveHeartBeat) break;
                await Task.Delay(1000);
            }

            sut.SubscribeTimeAndSales("AAPL");
            sut.SubscribeNasdaqBook();


            var hasQuote = false;
            while (iter < 30)
            {
                sut.StockQuoteReceived.Subscribe(s => hasQuote = s.symbol.Equals("AAPL"));
                //if (hasQuote) break;
                await Task.Delay(1000);
            }

            Assert.IsTrue(hasQuote);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public async Task StreamerChartData()
        {
            var sut = new SchwabStreamer(_schwabHelper);
            sut.ConnectSocket();
            sut.Login();
            var haveHeartBeat = false;

            var iter = 0;
            while (iter < 10)
            {
                sut.HeartBeat.Subscribe(hb => haveHeartBeat = true);
                if (haveHeartBeat) break;
                await Task.Delay(1000);
            }

            sut.SubscribeChartData("AAPL");


            var hasQuote = false;
            while (iter < 30)
            {
                sut.StockQuoteReceived.Subscribe(s => hasQuote = s.symbol.Equals("AAPL"));
                //if (hasQuote) break;
                await Task.Delay(1000);
            }

            Assert.IsTrue(hasQuote);
        }


        [TestCategory("Integration")]
        [TestMethod]
        public async Task StreamerSetQos()
        {
            var sut = new SchwabStreamer(_schwabHelper);
            try
            {
                sut.ConnectSocket();
                sut.Login();


                var isLoggedIn = false;
                var iter = 0;
                while (iter < 30)
                {
                    sut.LoginInfo.Subscribe(li => isLoggedIn = true);
                    if (isLoggedIn) break;
                    await Task.Delay(1000);
                    iter++;
                }


                sut.SetQosLevel(0);

                while (iter < 30)
                {
                    await Task.Delay(1000);
                    iter++;
                }
            }
            finally
            {
                sut.Dispose();
            }
        }

        [TestCategory("Integration")]
        [TestMethod]
        public async Task StreamerSubscribeAcctActivity()
        {
            var sut = new SchwabStreamer(_schwabHelper);
            try
            {
                sut.ConnectSocket();
                sut.Login();


                var isLoggedIn = false;
                var iter = 0;
                while (iter < 30)
                {
                    sut.LoginInfo.Subscribe(li => isLoggedIn = true);
                    if (isLoggedIn) break;
                    await Task.Delay(1000);
                    iter++;
                }

                sut.SubscribeAcctActivity();

                iter = 0;
                var haveAcctActivity = false;
                while (iter < 30)
                {
                    sut.SubscriptionInfo.Subscribe(si =>  haveAcctActivity = si.service.Equals("ACCT_ACTIVITY"));
                    if (haveAcctActivity) break;
                    await Task.Delay(1000);
                    iter++;
                }
                Assert.IsTrue(haveAcctActivity);

                this.PlaceLimitOrder().Wait();

                iter = 0;
                while (iter < 30)
                {
                    await Task.Delay(1000);
                    iter++;
                }

            }
            finally
            {
                sut.Dispose();
            }
        }





    }
}