﻿namespace EZTM.Common.Tda.Model
{
    public class SocketResponse
    {
        public string service { get; set; }
        public string requestId { get; set; }
        public string command { get; set; }
        public Dictionary<string, string> content { get; set; }
    }
}
