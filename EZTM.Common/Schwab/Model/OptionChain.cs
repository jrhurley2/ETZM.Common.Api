using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EZTM.Common.Schwab.Model
{
    public class OptionChain
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("strategy")]
        public string Strategy { get; set; }

        [JsonProperty("interval")]
        public decimal Interval { get; set; }

        [JsonProperty("isDelayed")]
        public bool IsDelayed { get; set; }

        [JsonProperty("isIndex")]
        public bool IsIndex { get; set; }

        [JsonProperty("interestRate")]
        public decimal InterestRate { get; set; }

        [JsonProperty("underlyingPrice")]
        public decimal UnderlyingPrice { get; set; }

        [JsonProperty("volatility")]
        public decimal Volatility { get; set; }

        [JsonProperty("daysToExpiration")]
        public decimal DaysToExpiration { get; set; }

        [JsonProperty("dividendYield")]
        public decimal DividendYield { get; set; }

        [JsonProperty("numberOfContracts")]
        public int NumberOfContracts { get; set; }

        [JsonProperty("assetMainType")]
        public string AssetMainType { get; set; }

        [JsonProperty("assetSubType")]
        public string AssetSubType { get; set; }

        [JsonProperty("isChainTruncated")]
        public bool IsChainTruncated { get; set; }

        [JsonProperty("callExpDateMap")]
        public Dictionary<string, Dictionary<string, List<OptionContract>>> CallExpDateMap { get; set; }

        [JsonProperty("putExpDateMap")]
        public Dictionary<string, Dictionary<string, List<OptionContract>>> PutExpDateMap { get; set; }
    }

    public class OptionDeliverable
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("assetType")]
        public string AssetType { get; set; }

        [JsonProperty("deliverableUnits")]
        public decimal DeliverableUnits { get; set; }
    }

    public class OptionContract
    {
        [JsonProperty("putCall")]
        public string PutCall { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("exchangeName")]
        public string ExchangeName { get; set; }

        [JsonProperty("bid")]
        public decimal Bid { get; set; }

        [JsonProperty("ask")]
        public decimal Ask { get; set; }

        [JsonProperty("last")]
        public decimal Last { get; set; }

        [JsonProperty("mark")]
        public decimal Mark { get; set; }

        [JsonProperty("bidSize")]
        public int BidSize { get; set; }

        [JsonProperty("askSize")]
        public int AskSize { get; set; }

        [JsonProperty("bidAskSize")]
        public string BidAskSize { get; set; }

        [JsonProperty("lastSize")]
        public int LastSize { get; set; }

        [JsonProperty("highPrice")]
        public decimal HighPrice { get; set; }

        [JsonProperty("lowPrice")]
        public decimal LowPrice { get; set; }

        [JsonProperty("openPrice")]
        public decimal OpenPrice { get; set; }

        [JsonProperty("closePrice")]
        public decimal ClosePrice { get; set; }

        [JsonProperty("totalVolume")]
        public int TotalVolume { get; set; }

        [JsonProperty("tradeTimeInLong")]
        public long TradeTimeInLong { get; set; }

        [JsonProperty("quoteTimeInLong")]
        public long QuoteTimeInLong { get; set; }

        [JsonProperty("netChange")]
        public decimal NetChange { get; set; }

        [JsonProperty("volatility")]
        public decimal Volatility { get; set; }

        [JsonProperty("delta")]
        public decimal Delta { get; set; }

        [JsonProperty("gamma")]
        public decimal Gamma { get; set; }

        [JsonProperty("theta")]
        public decimal Theta { get; set; }

        [JsonProperty("vega")]
        public decimal Vega { get; set; }

        [JsonProperty("rho")]
        public decimal Rho { get; set; }

        [JsonProperty("openInterest")]
        public int OpenInterest { get; set; }

        [JsonProperty("timeValue")]
        public decimal TimeValue { get; set; }

        [JsonProperty("theoreticalOptionValue")]
        public decimal TheoreticalOptionValue { get; set; }

        [JsonProperty("theoreticalVolatility")]
        public decimal TheoreticalVolatility { get; set; }

        [JsonProperty("optionDeliverablesList")]
        public List<OptionDeliverable> OptionDeliverablesList { get; set; }

        [JsonProperty("strikePrice")]
        public decimal StrikePrice { get; set; }

        [JsonProperty("expirationDate")]
        public string ExpirationDate { get; set; }

        [JsonProperty("daysToExpiration")]
        public int DaysToExpiration { get; set; }

        [JsonProperty("expirationType")]
        public string ExpirationType { get; set; }

        [JsonProperty("lastTradingDay")]
        public long LastTradingDay { get; set; }

        [JsonProperty("multiplier")]
        public decimal Multiplier { get; set; }

        [JsonProperty("settlementType")]
        public string SettlementType { get; set; }

        [JsonProperty("deliverableNote")]
        public string DeliverableNote { get; set; }

        [JsonProperty("percentChange")]
        public decimal PercentChange { get; set; }

        [JsonProperty("markChange")]
        public decimal MarkChange { get; set; }

        [JsonProperty("markPercentChange")]
        public decimal MarkPercentChange { get; set; }

        [JsonProperty("intrinsicValue")]
        public decimal IntrinsicValue { get; set; }

        [JsonProperty("extrinsicValue")]
        public decimal ExtrinsicValue { get; set; }

        [JsonProperty("optionRoot")]
        public string OptionRoot { get; set; }

        [JsonProperty("exerciseType")]
        public string ExerciseType { get; set; }

        [JsonProperty("high52Week")]
        public decimal High52Week { get; set; }

        [JsonProperty("low52Week")]
        public decimal Low52Week { get; set; }

        [JsonProperty("pennyPilot")]
        public bool PennyPilot { get; set; }

        [JsonProperty("inTheMoney")]
        public bool InTheMoney { get; set; }

        [JsonProperty("mini")]
        public bool Mini { get; set; }

        [JsonProperty("nonStandard")]
        public bool NonStandard { get; set; }
    }

    public class Underlying
    {
        public string symbol { get; set; }
        public string description { get; set; }
        public float change { get; set; }
        public float percentChange { get; set; }
        public float close { get; set; }
        public long quoteTime { get; set; }
        public long tradeTime { get; set; }
        public float bid { get; set; }
        public float ask { get; set; }
        public int last { get; set; }
        public float mark { get; set; }
        public float markChange { get; set; }
        public float markPercentChange { get; set; }
        public int bidSize { get; set; }
        public int askSize { get; set; }
        public float highPrice { get; set; }
        public float lowPrice { get; set; }
        public float openPrice { get; set; }
        public int totalVolume { get; set; }
        public string exchangeName { get; set; }
        public float fiftyTwoWeekHigh { get; set; }
        public float fiftyTwoWeekLow { get; set; }
        public bool delayed { get; set; }
    }
}
