using Newtonsoft.Json;

namespace EZTM.Common.Schwab.Model
{
    public class Account
    {
        public string accountNumber { get; set; }
        public bool primaryAccount { get; set; }
        public string type { get; set; }
        public string nickName { get; set; }
        public string accountColor { get; set; }
        public string displayAcctId { get; set; }
        public bool autoPositionEffect { get; set; }
    }
}
