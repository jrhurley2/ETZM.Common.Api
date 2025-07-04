using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;

namespace EZTM.Common.Schwab.Model
{


    public class Root
    {
        [JsonProperty("securitiesAccount")]
        public Securitiesaccount SecuritiesAccount { get; set; }
    }

    public class Securitiesaccount
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("roundTrips")]
        public int RoundTrips { get; set; }

        [JsonProperty("isDayTrader")]
        public bool IsDayTrader { get; set; }

        [JsonProperty("isClosingOnlyRestricted")]
        public bool IsClosingOnlyRestricted { get; set; }

        [JsonProperty("pfcbFlag")]
        public bool PfcbFlag { get; set; }

        [JsonProperty("initialBalances")]
        public Initialbalances InitialBalances { get; set; }

        [JsonProperty("currentBalances")]
        public Currentbalances CurrentBalances { get; set; }

        [JsonProperty("projectedBalances")]
        public Projectedbalances ProjectedBalances { get; set; }

        [JsonProperty("orderStrategies")]
        public Order[] OrderStrategies { get; set; }

        [JsonProperty("positions")]
        public Position[] Positions { get; set; }

        public static List<Securitiesaccount> ParseAccounts(string json)
        {
            var rootList = JsonConvert.DeserializeObject<List<Root>>(json);
            return rootList.Select(r => r.SecuritiesAccount).ToList();
        }

        public static Securitiesaccount ParseAccount(string json)
        {
            var root = JsonConvert.DeserializeObject<Root>(json);
            return root?.SecuritiesAccount;
        }

        public List<Order> FlatOrders
        {
            get
            {
                return OrderStrategies != null ? FlattenOrders(OrderStrategies.ToList()) : new List<Order>();
            }
        }

        public float DailyPnL
        {
            get
            {
                if (CurrentBalances == null || InitialBalances == null) return 0.0F;
                return 0.0F; // CurrentBalances.liquidationValue - InitialBalances.liquidationValue;
            }
        }

        private static List<Order> FlattenOrders(List<Order> orders)
        {
            List<Order> result = new List<Order>();
            foreach (var order in orders)
            {
                result.Add(order);
                if (order.childOrderStrategies != null && order.childOrderStrategies.Count > 0)
                {
                    var childOrders = FlattenOrders(order.childOrderStrategies);
                    result.AddRange(childOrders);
                }
            }
            return result;
        }
    }

    public class Initialbalances
    {
        [JsonProperty("accruedInterest")]
        public float AccruedInterest { get; set; }
        [JsonProperty("availableFundsNonMarginableTrade")]
        public float AvailableFundsNonMarginableTrade { get; set; }
        [JsonProperty("bondValue")]
        public float BondValue { get; set; }
        [JsonProperty("buyingPower")]
        public float BuyingPower { get; set; }
        [JsonProperty("cashBalance")]
        public float CashBalance { get; set; }
        [JsonProperty("cashAvailableForTrading")]
        public float CashAvailableForTrading { get; set; }
        [JsonProperty("cashReceipts")]
        public float CashReceipts { get; set; }
        [JsonProperty("dayTradingBuyingPower")]
        public float DayTradingBuyingPower { get; set; }
        [JsonProperty("dayTradingBuyingPowerCall")]
        public float DayTradingBuyingPowerCall { get; set; }
        [JsonProperty("dayTradingEquityCall")]
        public float DayTradingEquityCall { get; set; }
        [JsonProperty("equity")]
        public float Equity { get; set; }
        [JsonProperty("equityPercentage")]
        public float EquityPercentage { get; set; }
        [JsonProperty("liquidationValue")]
        public float LiquidationValue { get; set; }
        [JsonProperty("longMarginValue")]
        public float LongMarginValue { get; set; }
        [JsonProperty("longOptionMarketValue")]
        public float LongOptionMarketValue { get; set; }
        [JsonProperty("longStockValue")]
        public float LongStockValue { get; set; }
        [JsonProperty("maintenanceCall")]
        public float MaintenanceCall { get; set; }
        [JsonProperty("maintenanceRequirement")]
        public float MaintenanceRequirement { get; set; }
        [JsonProperty("margin")]
        public float Margin { get; set; }
        [JsonProperty("marginEquity")]
        public float MarginEquity { get; set; }
        [JsonProperty("moneyMarketFund")]
        public float MoneyMarketFund { get; set; }
        [JsonProperty("mutualFundValue")]
        public float MutualFundValue { get; set; }
        [JsonProperty("regTCall")]
        public float RegTCall { get; set; }
        [JsonProperty("shortMarginValue")]
        public float ShortMarginValue { get; set; }
        [JsonProperty("shortOptionMarketValue")]
        public float ShortOptionMarketValue { get; set; }
        [JsonProperty("shortStockValue")]
        public float ShortStockValue { get; set; }
        [JsonProperty("totalCash")]
        public float TotalCash { get; set; }
        [JsonProperty("isInCall")]
        public bool IsInCall { get; set; }
        [JsonProperty("pendingDeposits")]
        public float PendingDeposits { get; set; }
        [JsonProperty("marginBalance")]
        public float MarginBalance { get; set; }
        [JsonProperty("shortBalance")]
        public float ShortBalance { get; set; }
        [JsonProperty("accountValue")]
        public float AccountValue { get; set; }
    }

    public class Currentbalances
    {
        [JsonProperty("accruedInterest")]
        public float AccruedInterest { get; set; }
        [JsonProperty("cashBalance")]
        public float CashBalance { get; set; }
        [JsonProperty("cashReceipts")]
        public float CashReceipts { get; set; }
        [JsonProperty("longOptionMarketValue")]
        public float LongOptionMarketValue { get; set; }
        [JsonProperty("liquidationValue")]
        public float LiquidationValue { get; set; }
        [JsonProperty("longMarketValue")]
        public float LongMarketValue { get; set; }
        [JsonProperty("moneyMarketFund")]
        public float MoneyMarketFund { get; set; }
        [JsonProperty("savings")]
        public float Savings { get; set; }
        [JsonProperty("shortMarketValue")]
        public float ShortMarketValue { get; set; }
        [JsonProperty("pendingDeposits")]
        public float PendingDeposits { get; set; }
        [JsonProperty("mutualFundValue")]
        public float MutualFundValue { get; set; }
        [JsonProperty("bondValue")]
        public float BondValue { get; set; }
        [JsonProperty("shortOptionMarketValue")]
        public float ShortOptionMarketValue { get; set; }
        [JsonProperty("availableFunds")]
        public float AvailableFunds { get; set; }
        [JsonProperty("availableFundsNonMarginableTrade")]
        public float AvailableFundsNonMarginableTrade { get; set; }
        [JsonProperty("buyingPower")]
        public float BuyingPower { get; set; }
        [JsonProperty("buyingPowerNonMarginableTrade")]
        public float BuyingPowerNonMarginableTrade { get; set; }
        [JsonProperty("dayTradingBuyingPower")]
        public float DayTradingBuyingPower { get; set; }
        [JsonProperty("equity")]
        public float Equity { get; set; }
        [JsonProperty("equityPercentage")]
        public float EquityPercentage { get; set; }
        [JsonProperty("longMarginValue")]
        public float LongMarginValue { get; set; }
        [JsonProperty("maintenanceCall")]
        public float MaintenanceCall { get; set; }
        [JsonProperty("maintenanceRequirement")]
        public float MaintenanceRequirement { get; set; }
        [JsonProperty("marginBalance")]
        public float MarginBalance { get; set; }
        [JsonProperty("regTCall")]
        public float RegTCall { get; set; }
        [JsonProperty("shortBalance")]
        public float ShortBalance { get; set; }
        [JsonProperty("shortMarginValue")]
        public float ShortMarginValue { get; set; }
        [JsonProperty("sma")]
        public float Sma { get; set; }
    }

    public class Projectedbalances
    {
        [JsonProperty("availableFunds")]
        public float AvailableFunds { get; set; }
        [JsonProperty("availableFundsNonMarginableTrade")]
        public float AvailableFundsNonMarginableTrade { get; set; }
        [JsonProperty("buyingPower")]
        public float BuyingPower { get; set; }
        [JsonProperty("dayTradingBuyingPower")]
        public float DayTradingBuyingPower { get; set; }
        [JsonProperty("dayTradingBuyingPowerCall")]
        public float DayTradingBuyingPowerCall { get; set; }
        [JsonProperty("maintenanceCall")]
        public float MaintenanceCall { get; set; }
        [JsonProperty("regTCall")]
        public float RegTCall { get; set; }
        [JsonProperty("isInCall")]
        public bool IsInCall { get; set; }
        [JsonProperty("stockBuyingPower")]
        public float StockBuyingPower { get; set; }
    }

}
