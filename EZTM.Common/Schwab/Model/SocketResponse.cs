namespace EZTM.Common.Schwab.Model
{
    public class SocketResponse
    {
        public string service { get; set; }
        public string requestId { get; set; }
        public string command { get; set; }
        public Dictionary<string, object> content { get; set; }
    }



}
