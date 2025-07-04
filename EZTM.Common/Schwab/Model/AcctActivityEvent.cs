using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EZTM.Common.Schwab.Model
{
    public class AcctActivityEvent
    {
        public string SchwabOrderID { get; set; }
        public string AccountNumber { get; set; }
        public BaseEvent BaseEvent { get; set; }
    }

    public class BaseEvent
    {
        [JsonPropertyName("EventType")]
        public string EventType { get; set; }

        // All other event-type-specific data is captured here
        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; }
    }
}