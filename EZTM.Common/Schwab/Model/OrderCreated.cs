using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EZTM.Common.Schwab.Model
{
    //// Base event for all account activity events
    //public class AcctActivityEvent
    //{
    //    public string SchwabOrderID { get; set; }
    //    public string AccountNumber { get; set; }
    //    public BaseEvent BaseEvent { get; set; }
    //}

    //// BaseEvent with EventType and extension data for dynamic event-specific content
    //public class BaseEvent
    //{
    //    [JsonPropertyName("EventType")]
    //    public string EventType { get; set; }

    //    [JsonExtensionData]
    //    public Dictionary<string, System.Text.Json.JsonElement> AdditionalData { get; set; }
    //}

    // Strongly-typed OrderCreated event
    public class OrderCreatedEvent : AcctActivityEvent
    {
        [JsonPropertyName("BaseEvent")]
        public new OrderCreatedBaseEvent BaseEvent { get; set; }
    }

    public class OrderCreatedBaseEvent : BaseEvent
    {
        [JsonPropertyName("OrderCreatedEventEquityOrder")]
        public OrderCreatedEventEquityOrder OrderCreatedEventEquityOrder { get; set; }
    }

    public class OrderCreatedEventEquityOrder
    {
        [JsonPropertyName("EventType")]
        public string EventType { get; set; }

        [JsonPropertyName("Order")]
        public OrderCreatedOrderWrapper Order { get; set; }
    }

    public class OrderCreatedOrderWrapper
    {
        [JsonPropertyName("SchwabOrderID")]
        public string SchwabOrderID { get; set; }

        [JsonPropertyName("AccountNumber")]
        public string AccountNumber { get; set; }

        [JsonPropertyName("Order")]
        public OrderCreatedOrderDetail Order { get; set; }
    }

    // Partial structure for demonstration; expand as needed
    public class OrderCreatedOrderDetail
    {
        [JsonPropertyName("AccountInfo")]
        public AccountInfoDetail AccountInfo { get; set; }

        [JsonPropertyName("ClientChannelInfo")]
        public ClientChannelInfoDetail ClientChannelInfo { get; set; }

        [JsonPropertyName("LifecycleCreatedTimestamp")]
        public DateTimeStringWrapper LifecycleCreatedTimestamp { get; set; }

        [JsonPropertyName("LifecycleSchwabOrderID")]
        public string LifecycleSchwabOrderID { get; set; }

        [JsonPropertyName("EntryTimestamp")]
        public DateTimeStringWrapper EntryTimestamp { get; set; }

        [JsonPropertyName("ExpiryTimeStamp")]
        public DateTimeStringWrapper ExpiryTimeStamp { get; set; }

        [JsonPropertyName("AutoConfirm")]
        public bool AutoConfirm { get; set; }

        [JsonPropertyName("PlanSubmitDate")]
        public DateTimeStringWrapper PlanSubmitDate { get; set; }

        [JsonPropertyName("SourceOMS")]
        public string SourceOMS { get; set; }

        [JsonPropertyName("FirmID")]
        public string FirmID { get; set; }

        [JsonPropertyName("OrderAccount")]
        public string OrderAccount { get; set; }

        [JsonPropertyName("AdvancedOrderInfo")]
        public AdvancedOrderInfoDetail AdvancedOrderInfo { get; set; }

        [JsonPropertyName("AssetOrderEquityOrderLeg")]
        public AssetOrderEquityOrderLegDetail AssetOrderEquityOrderLeg { get; set; }
    }

    public class AccountInfoDetail
    {
        [JsonPropertyName("AccountNumber")]
        public string AccountNumber { get; set; }
        [JsonPropertyName("AccountBranch")]
        public string AccountBranch { get; set; }
        [JsonPropertyName("CustomerOrFirmCode")]
        public string CustomerOrFirmCode { get; set; }
        [JsonPropertyName("OrderPlacementCustomerID")]
        public string OrderPlacementCustomerID { get; set; }
        [JsonPropertyName("AccountState")]
        public string AccountState { get; set; }
        [JsonPropertyName("AccountTypeCode")]
        public string AccountTypeCode { get; set; }
    }

    public class ClientChannelInfoDetail
    {
        [JsonPropertyName("ClientProductCode")]
        public string ClientProductCode { get; set; }
        [JsonPropertyName("EventUserID")]
        public string EventUserID { get; set; }
        [JsonPropertyName("EventUserType")]
        public string EventUserType { get; set; }
    }

    public class DateTimeStringWrapper
    {
        [JsonPropertyName("DateTimeString")]
        public string DateTimeString { get; set; }
    }

    public class AdvancedOrderInfoDetail
    {
        [JsonPropertyName("TriggerSchwabOrderID")]
        public string TriggerSchwabOrderID { get; set; }
        [JsonPropertyName("AllLinkedSchwabOrderIDs")]
        public List<string> AllLinkedSchwabOrderIDs { get; set; }
        [JsonPropertyName("AdvancedOrderGoupID")]
        public string AdvancedOrderGoupID { get; set; }
        [JsonPropertyName("AdvancedOrderDescription")]
        public string AdvancedOrderDescription { get; set; }
        [JsonPropertyName("AdvancedOrderType")]
        public string AdvancedOrderType { get; set; }
        [JsonPropertyName("FillCondition")]
        public bool FillCondition { get; set; }
        [JsonPropertyName("ContingentTimeInForce")]
        public string ContingentTimeInForce { get; set; }
        [JsonPropertyName("ContingentExpiryTimeStamp")]
        public object ContingentExpiryTimeStamp { get; set; }
        [JsonPropertyName("ContingentPlanSubmitDate")]
        public DateTimeStringWrapper ContingentPlanSubmitDate { get; set; }
    }

    public class AssetOrderEquityOrderLegDetail
    {
        [JsonPropertyName("OrderInstruction")]
        public OrderInstructionDetail OrderInstruction { get; set; }
        [JsonPropertyName("CommissionInfo")]
        public CommissionInfoDetail CommissionInfo { get; set; }
        [JsonPropertyName("AssetType")]
        public string AssetType { get; set; }
        [JsonPropertyName("TimeInForce")]
        public string TimeInForce { get; set; }
        [JsonPropertyName("OrderTypeCode")]
        public string OrderTypeCode { get; set; }
        [JsonPropertyName("OrderLegs")]
        public List<OrderLegDetail> OrderLegs { get; set; }
        [JsonPropertyName("OrderCapacityCode")]
        public string OrderCapacityCode { get; set; }
        [JsonPropertyName("SettlementType")]
        public string SettlementType { get; set; }
        [JsonPropertyName("Rule80ACode")]
        public int Rule80ACode { get; set; }
        [JsonPropertyName("SolicitedCode")]
        public string SolicitedCode { get; set; }
        [JsonPropertyName("TradeTag")]
        public string TradeTag { get; set; }
        [JsonPropertyName("EquityOrder")]
        public EquityOrderDetail EquityOrder { get; set; }
    }

    public class OrderInstructionDetail
    {
        [JsonPropertyName("HandlingInstructionCode")]
        public string HandlingInstructionCode { get; set; }
        [JsonPropertyName("ExecutionStrategy")]
        public ExecutionStrategyDetail ExecutionStrategy { get; set; }
        [JsonPropertyName("PreferredRoute")]
        public object PreferredRoute { get; set; }
        [JsonPropertyName("EquityOrderInstruction")]
        public object EquityOrderInstruction { get; set; }
    }

    public class ExecutionStrategyDetail
    {
        [JsonPropertyName("Type")]
        public string Type { get; set; }
        [JsonPropertyName("LimitExecutionStrategy")]
        public LimitExecutionStrategyDetail LimitExecutionStrategy { get; set; }
    }

    public class LimitExecutionStrategyDetail
    {
        [JsonPropertyName("Type")]
        public string Type { get; set; }
        [JsonPropertyName("LimitPrice")]
        public PriceValueDetail LimitPrice { get; set; }
        [JsonPropertyName("LimitPriceUnitCode")]
        public string LimitPriceUnitCode { get; set; }
    }

    public class PriceValueDetail
    {
        [JsonPropertyName("lo")]
        public string Lo { get; set; }
        [JsonPropertyName("signScale")]
        public int SignScale { get; set; }
    }

    public class CommissionInfoDetail
    {
        [JsonPropertyName("EstimatedOrderQuantity")]
        public PriceValueDetail EstimatedOrderQuantity { get; set; }
        [JsonPropertyName("EstimatedPrincipalAmount")]
        public PriceValueDetail EstimatedPrincipalAmount { get; set; }
        [JsonPropertyName("EstimatedCommissionAmount")]
        public PriceValueDetail EstimatedCommissionAmount { get; set; }
    }

    public class OrderLegDetail
    {
        [JsonPropertyName("LegID")]
        public string LegID { get; set; }
        [JsonPropertyName("LegParentSchwabOrderID")]
        public string LegParentSchwabOrderID { get; set; }
        [JsonPropertyName("Quantity")]
        public PriceValueDetail Quantity { get; set; }
        [JsonPropertyName("QuantityUnitCodeType")]
        public string QuantityUnitCodeType { get; set; }
        [JsonPropertyName("LeavesQuantity")]
        public PriceValueDetail LeavesQuantity { get; set; }
        [JsonPropertyName("BuySellCode")]
        public string BuySellCode { get; set; }
        [JsonPropertyName("Security")]
        public SecurityDetail Security { get; set; }
        [JsonPropertyName("QuoteOnOrderAcceptance")]
        public QuoteOnOrderAcceptanceDetail QuoteOnOrderAcceptance { get; set; }
        [JsonPropertyName("LegClientRequestInfo")]
        public LegClientRequestInfoDetail LegClientRequestInfo { get; set; }
        [JsonPropertyName("AccountingRuleCode")]
        public string AccountingRuleCode { get; set; }
        [JsonPropertyName("EstimatedNetAmount")]
        public PriceValueDetail EstimatedNetAmount { get; set; }
        [JsonPropertyName("EstimatedPrincipalAmnt")]
        public PriceValueDetail EstimatedPrincipalAmnt { get; set; }
        [JsonPropertyName("EquityOrderLeg")]
        public object EquityOrderLeg { get; set; }
    }

    public class SecurityDetail
    {
        [JsonPropertyName("SchwabSecurityID")]
        public string SchwabSecurityID { get; set; }
        [JsonPropertyName("Symbol")]
        public string Symbol { get; set; }
        [JsonPropertyName("UnderlyingSymbol")]
        public string UnderlyingSymbol { get; set; }
        [JsonPropertyName("PrimaryExchangeCode")]
        public string PrimaryExchangeCode { get; set; }
        [JsonPropertyName("MajorAssetType")]
        public string MajorAssetType { get; set; }
        [JsonPropertyName("PrimaryMarketSymbol")]
        public string PrimaryMarketSymbol { get; set; }
        [JsonPropertyName("ShortDescriptionText")]
        public string ShortDescriptionText { get; set; }
        [JsonPropertyName("ShortName")]
        public string ShortName { get; set; }
        [JsonPropertyName("CUSIP")]
        public string CUSIP { get; set; }
        [JsonPropertyName("SEDOL")]
        public string SEDOL { get; set; }
        [JsonPropertyName("ISIN")]
        public string ISIN { get; set; }
        [JsonPropertyName("OptionsSecurityInfo")]
        public object OptionsSecurityInfo { get; set; }
    }

    public class QuoteOnOrderAcceptanceDetail
    {
        [JsonPropertyName("Ask")]
        public PriceValueDetail Ask { get; set; }
        [JsonPropertyName("AskSize")]
        public PriceValueDetail AskSize { get; set; }
        [JsonPropertyName("Bid")]
        public PriceValueDetail Bid { get; set; }
        [JsonPropertyName("BidSize")]
        public PriceValueDetail BidSize { get; set; }
        [JsonPropertyName("QuoteTimestamp")]
        public DateTimeStringWrapper QuoteTimestamp { get; set; }
        [JsonPropertyName("Symbol")]
        public string Symbol { get; set; }
        [JsonPropertyName("QuoteTypeCode")]
        public string QuoteTypeCode { get; set; }
        [JsonPropertyName("Mid")]
        public PriceValueDetail Mid { get; set; }
        [JsonPropertyName("SchwabOrderID")]
        public string SchwabOrderID { get; set; }
    }

    public class LegClientRequestInfoDetail
    {
        [JsonPropertyName("SecurityId")]
        public string SecurityId { get; set; }
        [JsonPropertyName("SecurityIdTypeCd")]
        public string SecurityIdTypeCd { get; set; }
    }

    public class EquityOrderDetail
    {
        [JsonPropertyName("TradingSessionCodeOnOrder")]
        public string TradingSessionCodeOnOrder { get; set; }
    }
}