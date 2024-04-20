using EZTM.Common.Model;
using EZTM.Common.Schwab.Model;
using Websocket.Client;
using Websocket.Client.Models;

namespace EZTM.Common.Interfaces
{
    public interface IStreamer
    {
        IObservable<AcctActivity> AcctActivity { get; }
        IObservable<DisconnectionInfo> Disconnection { get; }
        IObservable<StockQuote> FutureQuoteReceived { get; }
        IObservable<SocketNotify> HeartBeat { get; }
        IObservable<OrderFillMessage> OrderFilled { get; }
        IObservable<OrderEntryRequestMessage> OrderRecieved { get; }
        IObservable<ReconnectionInfo> Reconnection { get; }
        IObservable<StockQuote> StockQuoteReceived { get; }
        WebsocketClient WebsocketClient { get; }

        void Dispose();
        void SubscribeChartData(string tickerSymbol);
        void SubscribeFuture(string tickerSymbol);
        void SubscribeQuote(string tickerSymbol);
    }
}