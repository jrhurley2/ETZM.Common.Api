using EZTM.Common.Interfaces;
using EZTM.Common.Model;
using EZTM.Common.Schwab.Model;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Securitiesaccount = EZTM.Common.Schwab.Model.Securitiesaccount;
using StockQuote = EZTM.Common.Schwab.Model.StockQuote;

namespace EZTM.Common.Schwab
{
    public class SchwabHelper : Brokerage
    {
        private string accountId;

        private StreamWriter _replayFile;

        public const string ACCESSTOKENCONTAINER = "schwab-accesstokencontainer.json";
        public static HttpClient _httpClient = new HttpClient();
        public static Uri BaseUri = new Uri("https://api.schwabapi.com");


        public const string routeGetToken = "v1/oauth/token";

        public const string routeGetAccountNumbers = "trader/v1/accounts/accountNumbers";
        public const string routeGetAccounts = "trader/v1/accounts";
        public const string routeGetAccountByAccountId = "trader/v1/accounts/{0}?fields=positions";
        public const string routeGetUserPreferences = "trader/v1/userPreference";


        public const string routeGetOrdersByAccount = "trader/v1/accounts/{0}/orders?fromEnteredTime={1}&toEnteredTime={2}";
        public const string routeGetOrderByOrderId = "trader/v1/accounts/{0}/orders/{1}";
        public const string routePlaceOrder = "trader/v1/accounts/{0}/orders";
        public const string routeReplaceOrder = "trader/v1/accounts/{0}/orders/{1}";
        public const string routeCancelOrder = "trader/v1/accounts/{0}/orders/{1}";
        public const string routeGetOrders = "trader/v1/orders?fromEnteredTime={0}&toEnteredTime={1}";

        //public const string routeGetAccountByAccountId = "v1/accounts/{0}?fields=positions,orders";
        public const string routeGetQuote = "marketdata/v1/quotes?symbols={0}";
        //public const string routeGetPriceHistory = "v1/marketdata/{0}/pricehistory?periodType=day&period=2&frequencyType=minute&frequency=1&needExtendedHoursData=true&startDate={1}&endDate={2}";
        public const string routeGetPriceHistory = "marketdata/v1/pricehistory?symbol={0}&periodType=day&frequencyType=minute&frequency=1&needExtendedHoursData=true&startDate={1}&endDate={2}";


        public const string routeGetUserPrincipals = "v1/userprincipals?fields=streamerSubscriptionKeys,streamerConnectionInfo";
        public const string routeGetStreamerSubscriptionKeys = "v1/userprincipals/streamersubscriptionkeys?accountIds={0}";
        public const string routeGetTransactions = "v1/accounts/{0}/transactions";

        public override AccountInfo AccountInfo { get; set; }

        private static Securitiesaccount _securitiesaccount;
        private readonly Subject<Securitiesaccount> _securitiesAccountSubject = new Subject<Securitiesaccount>();
        public override IObservable<Securitiesaccount> SecuritiesAccountUpdated => _securitiesAccountSubject.AsObservable();


        private Dictionary<string, StockQuote> _stockQuotes = new();
        private AccessTokenContainer accessTokenContainer;
        private List<AccountNumberHash> _accountNumberHashes;

        public SchwabHelper(AccountInfo ai)
        {
            AccountInfo = ai;
        }


        public async override void Initialize()
        {
            _ = RefreshAccessToken().Result;
            //ConnectSocket();

            Task.Run(CheckTokenRefresh);
        }




        private async Task CheckTokenRefresh()
        {
            while (true)
            {
                if (AccessTokenContainer.ExpiresIn < 100)
                {
                    Debug.WriteLine("Refreshing Access Token");
                    await RefreshAccessToken();
                }

                // Wait for an hour before checking again
                await Task.Delay(TimeSpan.FromMilliseconds(30000));
            }
        }


        private readonly object _lockSecuritiesAccount = new object();

        public override Securitiesaccount Securitiesaccount
        {
            get
            {
                lock (_lockSecuritiesAccount)
                {
                    return _securitiesaccount;
                }
            }
            set
            {
                lock (_lockSecuritiesAccount)
                {
                    _securitiesaccount = value;
                }
            }
        }

        public override AccessTokenContainer AccessTokenContainer
        {
            get
            {
                if (accessTokenContainer == null)
                {
                    accessTokenContainer = GetAccessTokenContainer(ACCESSTOKENCONTAINER);
                }
                return accessTokenContainer;
            }
            set
            {
                Debug.WriteLine("***********************ACCESSTOKENCONTAINER BEING SET");
                accessTokenContainer = value;
            }
        }

        public override string LoginUri
        {
            get
            {
                return $"https://api.schwabapi.com/v1/oauth/authorize?response_type=code&client_id={AccountInfo.SchwabClientId}&scope=readonly&redirect_uri=https://127.0.0.1";
            }
        }

        public override bool NeedTokenRefreshed
        {
            get
            {
                //return (AccessTokenContainer == null || (AccessTokenContainer.TokenSystem == AccessTokenContainer.EnumTokenSystem.TDA && (AccessTokenContainer.IsRefreshTokenExpired || AccessTokenContainer.RefreshTokenExpiresInDays < 5)));
                return (AccessTokenContainer == null);// || (AccessTokenContainer.TokenSystem == AccessTokenContainer.EnumTokenSystem.TDA && (AccessTokenContainer.IsRefreshTokenExpired || AccessTokenContainer.RefreshTokenExpiresInDays < 5)));
            }
        }

        public override string AccountId
        {
            get
            {
                return accountId;
            }
        }


        #region Tokens

        /// <summary>
        /// Get access token.  This call is called only when a refresh token is needed.  TS should never expire and TDA expires once every 90 days.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public override async Task<AccessTokenContainer> GetAccessToken(string authToken)
        {
            try
            {
                var accountInfo = GetAccountInfo();


                // Set your Base64-encoded Client ID and Client Secret
                string base64Credentials = Base64Credentials(accountInfo);

                // Set the redirect URI
                string redirectUri = "https://127.0.0.1"; // Replace with actual value

                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
                postData.Add(new KeyValuePair<string, string>("code", $"{authToken}"));
                postData.Add(new KeyValuePair<string, string>("redirect_uri", $"{redirectUri}"));

                FormUrlEncodedContent content = new FormUrlEncodedContent(postData);
                var rawSTring = await content.ReadAsStringAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseUri, routeGetToken))
                {
                    Method = HttpMethod.Post,
                    Content = content
                };
                request.Headers.Clear();
                request.Headers.Add("Authorization", $"Basic {base64Credentials}");

                var response = await _httpClient.SendAsync(request);

                AccessTokenContainer = DeserializeJsonFromStream<AccessTokenContainer>(await response.Content.ReadAsStreamAsync());
                AccessTokenContainer.TokenSystem = AccessTokenContainer.EnumTokenSystem.TDA;

                //Write the access token container, this should ahve the refresh token
                SaveAccessTokenContainer(ACCESSTOKENCONTAINER, AccessTokenContainer);

                return AccessTokenContainer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                throw;
            }
        }

        public async Task<AccessTokenContainer> RefreshRefreshToken()
        {
            try
            {
                var accountInfo = GetAccountInfo();


                // Set your Base64-encoded Client ID and Client Secret
                string base64Credentials = Base64Credentials(accountInfo);

                // Set the redirect URI
                string redirectUri = "https://127.0.0.1"; // Replace with actual value

                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
                postData.Add(new KeyValuePair<string, string>("refresh_token", AccessTokenContainer.RefreshToken));
                postData.Add(new KeyValuePair<string, string>("access_type", "offline"));
                postData.Add(new KeyValuePair<string, string>("redirect_uri", $"{redirectUri}"));

                FormUrlEncodedContent content = new FormUrlEncodedContent(postData);
                var rawSTring = await content.ReadAsStringAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseUri, routeGetToken))
                {
                    Method = HttpMethod.Post,
                    Content = content
                };
                request.Headers.Clear();
                request.Headers.Add("Authorization", $"Basic {base64Credentials}");

                var response = await _httpClient.SendAsync(request);

                AccessTokenContainer = DeserializeJsonFromStream<AccessTokenContainer>(await response.Content.ReadAsStreamAsync());
                AccessTokenContainer.TokenSystem = AccessTokenContainer.EnumTokenSystem.TDA;

                //Write the access token container, this should ahve the refresh token
                SaveAccessTokenContainer(ACCESSTOKENCONTAINER, AccessTokenContainer);

                return AccessTokenContainer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                throw;
            }
        }


        public override async Task<AccessTokenContainer> RefreshAccessToken()
        {
            try
            {
                Debug.WriteLine($"Calling RefreshAccessToken:  {DateTime.Now.ToShortTimeString()}");
                var accountInfo = GetAccountInfo();

                // Set your Base64-encoded Client ID and Client Secret
                string base64Credentials = Base64Credentials(accountInfo);

                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
                postData.Add(new KeyValuePair<string, string>("refresh_token", AccessTokenContainer.RefreshToken));

                FormUrlEncodedContent content = new FormUrlEncodedContent(postData);
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseUri, routeGetToken))
                {
                    Method = HttpMethod.Post,
                    Content = content
                };

                request.Headers.Clear();
                request.Headers.Add("Authorization", $"Basic {base64Credentials}");

                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

                //Add code to handle the response status code.

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Debug.WriteLine($"RefreshAccessToken failed: {response.StatusCode}");
                    Debug.WriteLine($"RefreshAccessToken failed: {await response.Content.ReadAsStringAsync()}");
                    throw new Exception($"RefreshAccessToken failed: {response.StatusCode}");
                }

                var newAccessTokenContainer = DeserializeJsonFromStream<AccessTokenContainer>(await response.Content.ReadAsStreamAsync());

                //Add the refresh token back as it doesn't come back with the payload.
                newAccessTokenContainer.RefreshToken = AccessTokenContainer.RefreshToken;
                newAccessTokenContainer.TokenSystem = AccessTokenContainer.EnumTokenSystem.TDA;

                AccessTokenContainer = newAccessTokenContainer;
                return AccessTokenContainer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                throw;
            }
        }

        public override async Task<UserPreference> GetUserPreference()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUri, routeGetUserPreferences))
            {
                Method = HttpMethod.Get,
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessTokenContainer.AccessToken);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var userPrefernces = JsonConvert.DeserializeObject<UserPreference>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    return userPrefernces;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    throw;
                }
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }
        #endregion

        #region Accounts
        public async Task<List<AccountNumberHash>> GetAccountNumberHash()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUri, routeGetAccountNumbers))
            {
                Method = HttpMethod.Get,
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessTokenContainer.AccessToken);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var accountNumbers = DeserializeJsonFromStream<List<AccountNumberHash>>(await response.Content.ReadAsStreamAsync());

            return accountNumbers;
        }

        public override async Task<Securitiesaccount> GetAccountByAccountId(string accountId)
        {
            Securitiesaccount securitiesaccount = null;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUri, string.Format(routeGetAccountByAccountId, accountId)))
                {
                    Method = HttpMethod.Get,
                };

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessTokenContainer.AccessToken);

                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);


                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        //todo: put this back
                        securitiesaccount = Securitiesaccount.ParseAccount(await response.Content.ReadAsStringAsync());
                        Debug.WriteLine(JsonConvert.SerializeObject(securitiesaccount));

                        //Store it in tdhelper class.
                        Securitiesaccount = securitiesaccount;
                        //TODO:  When 2 windows are open this will cause the app to hang.
                        _securitiesAccountSubject.OnNext(securitiesaccount);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine($"Message Content: {await response.Content.ReadAsStringAsync()} ");
                    }
                }
                else
                {
                    Debug.WriteLine("Call to get Securities Account failed!");
                    Debug.WriteLine($"GetAccount Response {response.StatusCode}: {response.Content}");
                    Debug.WriteLine($"AccessContainerToken.ExpiresIn: {AccessTokenContainer.ExpiresIn}");
                    Debug.WriteLine($"AccessTokenContainer.IsTokenExpired: {AccessTokenContainer.IsTokenExpired}");
                    Debug.WriteLine($"{await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return Securitiesaccount;
        }

        public async Task<List<Securitiesaccount>> GetAccounts()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUri, routeGetAccounts))
            {
                Method = HttpMethod.Get,
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessTokenContainer.AccessToken);

            var response = await _httpClient.SendAsync(request);

            var accounts = Securitiesaccount.ParseAccounts(await response.Content.ReadAsStringAsync());

            return accounts;
        }
        #endregion

        #region Orders
        public async Task<List<Order>> GetOrdersByAccount(string accountId)
        {

            var today = DateTimeOffset.Now;
            var startDate = new DateTimeOffset(today.Year, today.Month, today.Day, 4, 00, 00, new TimeSpan(-4, 0, 0));
            var endDate = new DateTimeOffset(today.Year, today.Month, today.Day, 23, 00, 00, new TimeSpan(-4, 0, 0));

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUri, string.Format(routeGetOrdersByAccount, accountId, startDate.ToString("O"), endDate.ToString("O"))))
            {
                Method = HttpMethod.Get,
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessTokenContainer.AccessToken);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var orders = JsonConvert.DeserializeObject<List<Order>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    return orders;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    throw;
                }
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }


        public override async Task<ulong> PlaceOrder(string accountId, Order order)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUri, string.Format(routePlaceOrder, GetHashValue())))
            {
                Method = HttpMethod.Post,
                Content = Serialize(order)
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessTokenContainer.AccessToken);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(true);
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                Debug.Write(order);
                throw new Exception($"Error Creating Order {await response.Content.ReadAsStringAsync()} ");
            };

            var orderNumberString = response.Headers.Location.PathAndQuery.Substring(response.Headers.Location.PathAndQuery.LastIndexOf("/") + 1);
            var orderNumber = ulong.Parse(orderNumberString);
            Debug.WriteLine(JsonConvert.SerializeObject(order));

            return orderNumber;
        }

        private string GetHashValue()
        {
            if (_accountNumberHashes == null)
            {
                _accountNumberHashes = GetAccountNumberHash().Result;
            }

            return _accountNumberHashes.First(a => a.accountNumber.Equals(AccountInfo.SchwabAccountNumber)).hashValue;
        }

        public async Task<Order> GetOrderByOrderId(string orderId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUri, string.Format(routeGetOrderByOrderId, accountId, orderId)))
            {
                Method = HttpMethod.Get,
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessTokenContainer.AccessToken);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var order = JsonConvert.DeserializeObject<Order>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    return order;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    throw;
                }
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        public override async Task CancelOrder(Order order)
        {
            await CancelOrder(order.orderId);
        }

        public async Task CancelOrder(string orderId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUri, string.Format(routeCancelOrder, GetHashValue(), orderId)))
            {
                Method = HttpMethod.Delete,
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessTokenContainer.AccessToken);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(true);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Debug.Write(orderId);
                throw new Exception($"Error deleting Order {await response.Content.ReadAsStringAsync()} ");
            };
        }

        public override async Task<ulong> ReplaceOrder(string accountId, string orderId, Order newOrder)
        {
            var uri = new Uri(BaseUri, string.Format(routeReplaceOrder, accountId, orderId));

            var request = new HttpRequestMessage(HttpMethod.Put, uri)
            {
                Method = HttpMethod.Put,
                Content = Serialize(newOrder)
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessTokenContainer.AccessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            var response = await _httpClient.SendAsync(request).ConfigureAwait(true);
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                Debug.Write(newOrder);
                throw new Exception($"Error Replacing Order {await response.Content.ReadAsStringAsync()} ");
            };

            var orderNumberString = response.Headers.Location.PathAndQuery.Substring(response.Headers.Location.PathAndQuery.LastIndexOf("/") + 1);
            var orderNumber = ulong.Parse(orderNumberString);
            Debug.WriteLine(JsonConvert.SerializeObject(newOrder));

            return orderNumber;


        }

        public async Task<List<Model.Order>> GetOrders()
        {

            var today = DateTimeOffset.Now;
            var startDate = new DateTimeOffset(today.Year, today.Month, today.Day, 4, 00, 00, new TimeSpan(-4, 0, 0));
            var endDate = new DateTimeOffset(today.Year, today.Month, today.Day, 19, 00, 00, new TimeSpan(-4, 0, 0));

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUri, string.Format(routeGetOrders, startDate.ToString("O"), endDate.ToString("O"))))
            {
                Method = HttpMethod.Get,
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessTokenContainer.AccessToken);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var orders = JsonConvert.DeserializeObject<List<Order>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    return orders;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    throw;
                }
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        #endregion

        #region Transactions
        public async Task<Securitiesaccount> GetTransactions(string accountId, string symbol)
        {

            var today = DateTimeOffset.Now;
            var startDate = new DateTimeOffset(today.Year, today.Month, today.Day, 4, 00, 00, new TimeSpan(-4, 0, 0));
            var endDate = new DateTimeOffset(today.Year, today.Month, today.Day, 19, 00, 00, new TimeSpan(-4, 0, 0));


            Securitiesaccount securitiesaccount = null;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUri, string.Format(routeGetTransactions, accountId, startDate.ToString("O"), endDate.ToString("O"))))
                {
                    Method = HttpMethod.Get,
                };

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessTokenContainer.AccessToken);

                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);


                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        //todo: put this bck
                        //securitiesaccount = Securitiesaccount.ParseAccounts(await response.Content.ReadAsStringAsync());
                        Debug.WriteLine(JsonConvert.SerializeObject(securitiesaccount));

                        //Store it in tdhelper class.
                        Securitiesaccount = securitiesaccount;
                        //TODO:  When 2 windows are open this will cause the app to hang.
                        _securitiesAccountSubject.OnNext(securitiesaccount);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine($"Message Content: {await response.Content.ReadAsStringAsync()} ");
                    }
                }
                else
                {
                    Debug.WriteLine("Call to get Securities Account failed!");
                    Debug.WriteLine($"GetAccount Response {response.StatusCode}: {response.Content}");
                    Debug.WriteLine($"AccessContainerToken.ExpiresIn: {AccessTokenContainer.ExpiresIn}");
                    Debug.WriteLine($"AccessTokenContainer.IsTokenExpired: {AccessTokenContainer.IsTokenExpired}");
                    Debug.WriteLine($"{await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return Securitiesaccount;
        }
        #endregion


        #region Quotes
        public async Task<Dictionary<string, StockDetailedQuote>> GetQuote(string symbol)
        {
            StockQuote quote = null;
            Dictionary<string, StockDetailedQuote> quotes = null;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUri, string.Format(routeGetQuote, symbol)))
                {
                    Method = HttpMethod.Get,
                };

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessTokenContainer.AccessToken);

                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);


                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        quotes = DeserializeJsonFromStream<Dictionary<string, StockDetailedQuote>>(await response.Content.ReadAsStreamAsync());

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine($"Message Content: {await response.Content.ReadAsStringAsync()} ");
                    }
                }
                else
                {
                    Debug.WriteLine("Call to get Securities Account failed!");
                    Debug.WriteLine($"GetAccount Response {response.StatusCode}: {response.Content}");
                    Debug.WriteLine($"AccessContainerToken.ExpiresIn: {AccessTokenContainer.ExpiresIn}");
                    Debug.WriteLine($"AccessTokenContainer.IsTokenExpired: {AccessTokenContainer.IsTokenExpired}");
                    Debug.WriteLine($"{await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return quotes;

        }
        #endregion


        public async Task<CandleList> GetPriceHistoryAsync(string symbol)
        {
            var today = DateTimeOffset.Now;
            var startDate = new DateTimeOffset(today.Year, today.Month, today.Day, 4, 00, 00, new TimeSpan(-4, 0, 0));
            var endDate = new DateTimeOffset(today.Year, today.Month, today.Day, 19, 00, 00, new TimeSpan(-4, 0, 0));
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUri, string.Format(routeGetPriceHistory, symbol, startDate.ToUnixTimeMilliseconds(), endDate.ToUnixTimeMilliseconds())))
            {
                Method = HttpMethod.Get,
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessTokenContainer.AccessToken);

            var response = await _httpClient.SendAsync(request);
            var candleList = DeserializeJsonFromStream<CandleList>(await response.Content.ReadAsStreamAsync());

            foreach (var candle in candleList.candles)
            {
                Debug.WriteLine($"{candle.DateTime},{candle.open},{candle.close},{candle.high},{candle.low},{candle.volume}");
            }

            return candleList;
        }




        private static StringContent Serialize(Order order)
        {
            return new StringContent(JsonConvert.SerializeObject(order,
                                                                                    Formatting.None,
                                                                                    new JsonSerializerSettings
                                                                                    {
                                                                                        NullValueHandling = NullValueHandling.Ignore,
                                                                                        DefaultValueHandling = DefaultValueHandling.Ignore
                                                                                    }),
                            Encoding.UTF8, "application/json");
        }

        public override Order GetInitialLimitOrder(Securitiesaccount securitiesaccount, Order triggerOrder)
        {
            var lmitOrder = triggerOrder.childOrderStrategies[0].childOrderStrategies.Where(o => (o.status == "QUEUED" || o.status == "WORKING" || o.status == "PENDING_ACTIVATION" || o.status == "AWAITING_PARENT_ORDER") && o.orderLegCollection[0].instrument.symbol.ToUpper() == triggerOrder.orderLegCollection[0].instrument.symbol.ToUpper() && o.orderType == "LIMIT").FirstOrDefault();
            return lmitOrder;
        }

        public override StockQuote SetStockQuote(StockQuote stockQuote)
        {
            if (!_stockQuotes.ContainsKey(stockQuote.symbol))
            {
                _stockQuotes.Add(stockQuote.symbol, stockQuote);
            }

            _stockQuotes[stockQuote.symbol] = _stockQuotes[stockQuote.symbol].Update(stockQuote);

            return _stockQuotes[stockQuote.symbol];
        }

        public override StockQuote GetStockQuote(string symbol)
        {
            if (!_stockQuotes.ContainsKey(symbol)) { return null; }

            return _stockQuotes[symbol];
        }

        public override async Task CancelAll(string symbol)
        {
            var orders = await this.GetOrdersByAccount(GetHashValue());

            if (orders != null)
            {

                var openOrders = Brokerage.GetOpenOrders(Order.FlattenOrders(orders), symbol);

                var tasks = new List<Task>();
                foreach (var order in openOrders)
                {
                    var task = this.CancelOrder(order);
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks).ConfigureAwait(true);
            }
            else
            {
                Debug.WriteLine("CancelAll - securitiesAccount is null");
            }
        }



        public override async Task<IStreamer> GetStreamer()
        {
            return new SchwabStreamer(this);
        }


        public static string ToQueryString(Dictionary<string, object> dict)
        {
            if (dict.Count == 0) return string.Empty;

            var buffer = new StringBuilder();
            int count = 0;
            bool end = false;

            foreach (var key in dict.Keys)
            {
                if (count == dict.Count - 1) end = true;

                if (end)
                    buffer.AppendFormat("{0}={1}", key, dict[key]);
                else
                    buffer.AppendFormat("{0}={1}&", key, dict[key]);

                count++;
            }

            return buffer.ToString();
        }

        static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }


        private static string Base64Credentials(AccountInfo accountInfo)
        {
            return Base64Encode($"{accountInfo.SchwabClientId}:{accountInfo.SchwabClientSecret}");
        }
    }

}
