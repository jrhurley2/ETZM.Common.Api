﻿namespace EZTM.Common.Tda.Model
{
    public class SocketRequest
    {
        public string service { get; set; }
        public string command { get; set; }
        public int requestid { get; set; }
        public string account { get; set; }
        public string source { get; set; }
        public Parameters parameters { get; set; }
    }
    public class Parameters
    {
        public Credentials credentials { get; set; }
        public string token { get; set; }
        public string version { get; set; }
    }
}
