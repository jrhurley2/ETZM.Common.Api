using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZTM.Common.Schwab.Model
{

    public class OptionExpirationChain
    {
        public Expirationlist[] expirationList { get; set; }
    }

    public class Expirationlist
    {
        public string expirationDate { get; set; }
        public int daysToExpiration { get; set; }
        public string expirationType { get; set; }
        public bool standard { get; set; }
    }
}
