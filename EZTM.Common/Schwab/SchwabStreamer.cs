using EZTM.Common.Interfaces;
using EZTM.Common.Schwab.Model;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Websocket.Client;
using Websocket.Client.Models;

namespace EZTM.Common.Schwab
{
    public class SchwabStreamer : IStreamer
    {
        private StreamWriter _replayFile;
        private IBrokerage _brokerage;

        private bool _isConnected = false;
        private UserPreference _userPreference;
        private bool _isDisposing;
        private StreamerSettings.Request _loginRequest;

        private WebsocketClient _ws;
        private List<string> _quoteSymbols = new List<string>();
        private List<string> _timeAndSalesSymbols = new List<string>();
        public int _quoteRequestId = 0;


        private readonly Subject<Model.StockQuote> _stockQuoteRecievedSubject = new Subject<StockQuote>();
        public IObservable<StockQuote> StockQuoteReceived => _stockQuoteRecievedSubject.AsObservable();

        private readonly Subject<Model.StockQuote> _futureQuoteRecievedSubject = new Subject<StockQuote>();
        public IObservable<StockQuote> FutureQuoteReceived => _futureQuoteRecievedSubject.AsObservable();

        private readonly Subject<AcctActivity> _acctActivity = new Subject<AcctActivity>();
        public IObservable<AcctActivity> AcctActivity => _acctActivity.AsObservable();

        private readonly Subject<OrderEntryRequestMessage> _orderEntryRequestMessage = new Subject<OrderEntryRequestMessage>();
        public IObservable<OrderEntryRequestMessage> OrderRecieved => _orderEntryRequestMessage.AsObservable();

        private readonly Subject<OrderFillMessage> _orderFillMessage = new Subject<OrderFillMessage>();
        public IObservable<OrderFillMessage> OrderFilled => _orderFillMessage.AsObservable();

        private readonly Subject<SocketNotify> _socketNotify = new Subject<SocketNotify>();
        public IObservable<SocketNotify> HeartBeat => _socketNotify.AsObservable();

        private readonly Subject<DisconnectionInfo> _disconnectionInfo = new Subject<DisconnectionInfo>();
        public IObservable<DisconnectionInfo> Disconnection => _disconnectionInfo.AsObservable();

        private readonly Subject<ReconnectionInfo> _reconnectionInfo = new Subject<ReconnectionInfo>();
        public IObservable<ReconnectionInfo> Reconnection => _reconnectionInfo.AsObservable();

        private readonly Subject<SocketResponse> _loginInfo = new Subject<SocketResponse>();
        public IObservable<SocketResponse> LoginInfo => _loginInfo.AsObservable();

        private readonly Subject<SocketResponse> _subscriptionInfo = new Subject<SocketResponse>();
        public IObservable<SocketResponse> SubscriptionInfo => _subscriptionInfo.AsObservable();


        public SchwabStreamer(IBrokerage brokerage)
        {
            _brokerage = brokerage;
            string currentPath = Directory.GetCurrentDirectory();
            string replayFolder = Path.Combine(currentPath, "replays");
            if (!Directory.Exists(replayFolder))
                Directory.CreateDirectory(replayFolder);

            _replayFile = new StreamWriter($"{replayFolder}\\{DateTime.Now.ToString("yyyyMMdd-HHmmss")}replay.txt");

        }
        public async Task<IStreamer> GetStreamer(IBrokerage brokerage)
        {
            _brokerage = brokerage;
            var userPreference = _brokerage.GetUserPreference().Result;
            return this; // new TDStreamer(this);
        }

        public WebsocketClient WebsocketClient
        {
            get
            {
                return _ws;
            }
        }

        /// <summary>
        /// Connect to the Schwab Streamer
        /// </summary>
        public void ConnectSocket()
        {
            _userPreference = _brokerage.GetUserPreference().Result;
            var url = new Uri(_userPreference.streamerInfo[0].streamerSocketUrl);

            _ws = new WebsocketClient(url);
            _ws.ReconnectTimeout = TimeSpan.FromSeconds(30);

            SubscribeWebSocketMessages(_userPreference, _ws);

            _ws.Start();
        }


        /// <summary>
        /// Login to the Schwab Streamer
        /// </summary>
        public void  Login()
        {
            _userPreference = _brokerage.GetUserPreference().Result;

            var _reqs = new List<StreamerSettings.Request>();

            _loginRequest = new StreamerSettings.Request
            {
                service = "ADMIN",
                requestid = "0",
                command = "LOGIN",
                SchwabClientCorrelId = _userPreference.streamerInfo[0].schwabClientCorrelId,
                SchwabClientCustomerId = _userPreference.streamerInfo[0].schwabClientCustomerId,
                parameters = new StreamerSettings.Parameters
                {
                    Authorization = _brokerage.AccessTokenContainer.AccessToken,
                    SchwabClientChannel = _userPreference.streamerInfo[0].schwabClientChannel,
                    SchwabClientFunctionId = _userPreference.streamerInfo[0].schwabClientFunctionId,
                    version = "1.0",
                    qoslevel = "0"
                }
            };


            _reqs.Add(_loginRequest);

            var request = new StreamerSettings.Requests()
            {
                requests = _reqs.ToArray()
            };

            var req = JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            _ws.Send(req);

        }

        public void SetQosLevel(int qosLevel)
        {
            _userPreference = _brokerage.GetUserPreference().Result;
            var _reqs = new List<StreamerSettings.Request>();

            _loginRequest = new StreamerSettings.Request
            {
                service = "ADMIN",
                requestid = "1",
                command = "QOS",
                SchwabClientCorrelId = _userPreference.streamerInfo[0].schwabClientCorrelId,
                SchwabClientCustomerId = _userPreference.streamerInfo[0].schwabClientCustomerId,
                parameters = new StreamerSettings.Parameters
                {
                    SchwabClientChannel = _userPreference.streamerInfo[0].schwabClientChannel,
                    SchwabClientFunctionId = _userPreference.streamerInfo[0].schwabClientFunctionId,
                    qoslevel = qosLevel.ToString()
                }
            };


            _reqs.Add(_loginRequest);

            var request = new StreamerSettings.Requests()
            {
                requests = _reqs.ToArray()
            };

            var req = JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            _ws.Send(req);


        }

        /// <summary>
        /// Subscribe to Account Activity Service
        /// </summary>
        public void SubscribeAcctActivity()
        {
            var service = "ACCT_ACTIVITY";
            var requestid = 2;
        
            var streamingParameters = new StreamerSettings.Parameters
            {
                keys = "Account Activity",
                fields = "0,1,2,3"
            };

            SubscribeService(service, requestid, streamingParameters);
        }

        public void SubscribeQuote(string tickerSymbol)
        {
            if (!_quoteSymbols.Contains(tickerSymbol.ToUpper()))
            {
                _quoteSymbols.Add(tickerSymbol.ToUpper());
            }

            SubscribeQuote();
        }

        private void SubscribeQuote()
        {
            var symbols = string.Join(",", _quoteSymbols);
            var service = "LEVELONE_EQUITIES";
           
            var requestid = 1;

            var streamingParameters = new StreamerSettings.Parameters
            {
                keys = symbols,
                fields = string.Join(",", Enumerable.Range(0, 53))
            };

            SubscribeService(service, requestid, streamingParameters);
        }


        public void SubscribeTimeAndSales(string tickerSymbol)
        {
            if (!_timeAndSalesSymbols.Contains(tickerSymbol.ToUpper()))
            {
                _timeAndSalesSymbols.Add(tickerSymbol.ToUpper());
            }

            SubscribeTimeAndSales();
        }

        private void SubscribeTimeAndSales()
        {
            var symbols = string.Join(",", _timeAndSalesSymbols);

            var service = "TIMESALE_EQUITY";
            var requestid = 2;

            var streamingParameters = new StreamerSettings.Parameters
            {
                keys = symbols,
                fields = "0,1,2,3,4"
            };

            SubscribeService(service, requestid, streamingParameters);
        }

        public void SubscribeNasdaqBook()
        {
            //var symbols = string.Join(",", _timeAndSalesSymbols);

            var symbols = "AAPL";
            var service = "NASDAQ_BOOK";
            var requestid = 2;

            var streamingParameters = new StreamerSettings.Parameters
            {
                keys = symbols,
                fields = "0,1,2,3,4"
            };

            SubscribeService(service, requestid, streamingParameters);
        }

        public void SubscribeChartData(string tickerSymbol)
        {
            var service = "CHART_EQUITY";
            var requestid = 2;

            var streamingParameters = new StreamerSettings.Parameters
            {
                keys = tickerSymbol,
                fields = "0,1,2,3,4"
            };

            SubscribeService(service, requestid, streamingParameters);
        }



        private void SubscribeService(string service, int requestid, StreamerSettings.Parameters streamingParameters)
        {
            var _reqs = new List<StreamerSettings.Request>();

            var quoteRequest = new StreamerSettings.Request
            {
                service = service,
                command = "SUBS",
                requestid = requestid.ToString(),
                SchwabClientCorrelId = _userPreference.streamerInfo[0].schwabClientCorrelId,
                SchwabClientCustomerId = _userPreference.streamerInfo[0].schwabClientCustomerId,
                parameters = streamingParameters
            };
            _reqs.Add(quoteRequest);
            //}
            var request = new StreamerSettings.Requests()
            {
                requests = _reqs.ToArray()
            };

            var req = JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            _ws.Send(req);
        }






        List<string> _futureSymbols = new List<string>();

        public void SubscribeFuture(string tickerSymbol)
        {
            if (!_futureSymbols.Contains(tickerSymbol.ToUpper()))
            {
                _futureSymbols.Add(tickerSymbol.ToUpper());
            }

            var symbols = string.Join(",", _futureSymbols);

            var _reqs = new List<StreamerSettings.Request>();

            _quoteRequestId++;
            var quoteRequest = new StreamerSettings.Request
            {
                service = "LEVELONE_FUTURES",
                command = "SUBS",
                requestid = "1", //_quoteRequestId.ToString(),
                SchwabClientCorrelId = _userPreference.streamerInfo[0].schwabClientCorrelId,
                SchwabClientCustomerId = _userPreference.streamerInfo[0].schwabClientCustomerId,
                parameters = new StreamerSettings.Parameters
                {
                    keys = symbols,
                    fields = "0,1,2,3,4"
                }
            };
            _reqs.Add(quoteRequest);
            //}
            var request = new StreamerSettings.Requests()
            {
                requests = _reqs.ToArray()
            };

            var req = JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            _ws.Send(req);
        }

        //List<string> _futureSymbols= new List<string>();

        //public void SubscribeFuture(UserPrincipal userPreference, string tickerSymbol)
        //{
        //    if (!_futureSymbols.Contains(tickerSymbol.ToUpper()))
        //    {
        //        _futureSymbols.Add(tickerSymbol.ToUpper());
        //    }

        //    var symbols = string.Join(",", _futureSymbols);

        //    var _reqs = new List<StreamerSettings.Request>();

        //    int requestId = 0;
        //    //foreach (var symbol in _quoteSymbols)
        //    //{
        //    //requestId++;
        //    _quoteRequestId++;
        //    var quoteRequest = new StreamerSettings.Request
        //    {
        //        service = "LEVELONE_FUTURES",
        //        command = "SUBS",
        //        requestid = "1", //_quoteRequestId.ToString(),
        //        account = userPreference.accounts[0].accountId,
        //        source = userPreference.streamerInfo.appId,
        //        parameters = new StreamerSettings.Parameters
        //        {
        //            keys = symbols,
        //            fields = "0,1,2,3,4"
        //        }
        //    };
        //    _reqs.Add(quoteRequest);
        //    //}
        //    var request = new StreamerSettings.Requests()
        //    {
        //        requests = _reqs.ToArray()
        //    };

        //    var req = JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        //    _ws.Send(req);
        //}




        private int reconnectionCount = 0;
        private void SubscribeWebSocketMessages(UserPreference userPreference, WebsocketClient client)
        {
            client.MessageReceived.Subscribe(msg =>
            {
                _socketNotify.OnNext(new SocketNotify());
                Debug.WriteLine(msg.Text);
                try
                {
                    if (_replayFile != null)
                        _replayFile.WriteLine(msg.Text);
                    //_replayFile.WriteLine(SanatizeAccountNumbers(userPreference, msg.Text));

                }
                catch
                {
                    Debug.WriteLine("error writing to replay file");
                }

                var v = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg.Text);

                if (v.ContainsKey("response"))
                {
                    Debug.WriteLine(msg.Text);
                    var r = v["response"];
                    var response = JsonConvert.DeserializeObject<List<SocketResponse>>(v["response"].ToString());

                    var command = response[0].command;
                    if (command == "LOGIN")
                    {
                        _loginInfo.OnNext(response[0]);
                        Console.WriteLine(msg);
                    }
                    else if (command == "SUBS")
                    {
                        _subscriptionInfo.OnNext(response[0]);
                        Debug.WriteLine(msg.Text);
                        Console.WriteLine(msg);
                    }
                    else
                    {
                        Debug.WriteLine(msg.Text);    
                    }
                }
                else if (v.ContainsKey("notify"))
                {
                    var notifyList = JsonConvert.DeserializeObject<List<SocketNotify>>(v["notify"].ToString());
                    foreach (var notify in notifyList)
                    {
                        _socketNotify.OnNext(notify);
                    }
                }
                else if (v.ContainsKey("data"))
                {
                    var r = v["data"];
                    Debug.WriteLine(r);


                    var data = JsonConvert.DeserializeObject<List<SocketData>>(v["data"].ToString());

                    foreach (var socketData in data)
                    {
                        var command = socketData.command;
                        var service = socketData.service;
                        if (service == "LEVELONE_EQUITIES")
                        {
                            foreach (var content in socketData.content)
                            {
                                var quoteJson = content.ToDictionary(k => k.Key, k => k.Value.ToString());
                                var stockQuote = new Model.StockQuote(quoteJson);
                                _stockQuoteRecievedSubject.OnNext(stockQuote);
                                //Debug.WriteLine("TDStreamer:" + JsonConvert.SerializeObject(stockQuote));
                            }
                        }
                        else if (service == "LEVELONE_FUTURES")
                        {
                            foreach (var content in socketData.content)
                            {
                                var quoteJson = content.ToDictionary(k => k.Key, k => k.Value.ToString());
                                var futureQuote = new Model.StockQuote(quoteJson);
                                _futureQuoteRecievedSubject.OnNext(futureQuote);
                                Debug.WriteLine(JsonConvert.SerializeObject(futureQuote));
                            }
                            Console.WriteLine($"Futures {socketData.content}");
                        }
                        else if (service == "ACCT_ACTIVITY")
                        {
                            //Signal we have Account Activity
                            _acctActivity.OnNext(new AcctActivity());

                            foreach (var content in socketData.content)
                            {

                                //if (content["2"] == "OrderEntryRequest")
                                //{
                                //    try
                                //    {
                                //        //Parsing was inconsitnat don't have a complete XML Schema, and wasn't using it on the other side.
                                //        //var orderEntryRequestMessage = OrderEntryRequestMessage.ParseXml(content["3"]);
                                //        var orderEntryRequestMessage = new OrderEntryRequestMessage();
                                //        _orderEntryRequestMessage.OnNext(orderEntryRequestMessage);
                                //    }
                                //    catch (Exception ex)
                                //    {
                                //        Debug.WriteLine(ex.Message);
                                //        Debug.WriteLine(ex.StackTrace);
                                //    }
                                //}
                                //if (content["2"] == "OrderFill")
                                //{
                                //    try
                                //    {
                                //        Debug.WriteLine(content["3"]);
                                //        //Check that the order is a stock order, will throw excption if it is options, etc...
                                //        if (content["3"].Contains("EquityOrderT"))
                                //        {
                                //            var orderFillMessage = OrderFillMessage.ParseXml(content["3"]);
                                //            _orderFillMessage.OnNext(orderFillMessage);
                                //        }
                                //        else
                                //        {
                                //            Debug.WriteLine("We don't handle messages other than EquityOrderT");
                                //        }
                                //    }
                                //    catch (Exception ex)
                                //    {
                                //        Debug.WriteLine(ex.Message);
                                //        Debug.WriteLine(ex.StackTrace);
                                //    }
                                //}
                            }
                        }
                        //else if (service == "CHART_EQUITY")
                        //{
                        //    var stockChartBar = new StockChartBar(socketData.content[0]);
                        //    Debug.WriteLine(msg.Text);
                        //}
                        else if (service.Equals("NASDAQ_BOOK"))
                        {

                        }
                        else
                        {
                            Debug.WriteLine(msg.Text);
                        }
                        Console.WriteLine(msg);
                    }
                }
                Console.WriteLine($"Message received: {msg}");
            });

            client.DisconnectionHappened.Subscribe(dis =>
            {
                try
                {
                    _disconnectionInfo.OnNext(dis);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to connect {ex.Message}");
                    Debug.WriteLine(ex);
                }

            });

            client.ReconnectionHappened.Subscribe(info =>
            {
                Debug.WriteLine($"Reconnection happened, type: {info.Type}");
                reconnectionCount = 0;
                _reconnectionInfo.OnNext(info);
            });


        }



        private static string SanatizeAccountNumbers(UserPrincipal userPrincipal, string message)
        {
            int index = 0;
            string newMessage = message;
            foreach (var account in userPrincipal.accounts)
            {
                newMessage = newMessage.Replace(account.accountId, $"sanatizedAccountNumber{index}");
            }

            return newMessage;
        }


        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalMilliseconds);
        }

        public void Dispose()
        {
            if (_ws != null) _ws.Dispose();
            if (_replayFile != null)
            {
                _replayFile.Flush();
                _replayFile.Dispose();
            }
            _stockQuoteRecievedSubject.Dispose();
        }

    }
}
