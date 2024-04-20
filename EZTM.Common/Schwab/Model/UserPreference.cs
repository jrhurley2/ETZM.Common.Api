using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZTM.Common.Schwab.Model
{
    public class Offer
    {
        public bool level2Permissions { get; set; }
    }

    public class UserPreference
    {
        public List<Account> accounts { get; set; }
        public List<StreamerInfo> streamerInfo { get; set; }
        public List<Offer> offers { get; set; }
    }

}
