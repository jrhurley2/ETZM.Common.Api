namespace EZTM.Common.Schwab.Model
{
    public class SocketData
    {
        public string service { get; set; }
        public long timestamp { get; set; }
        public string command { get; set; }
        public List<Dictionary<string, object>> content { get; set; }
    }
}
