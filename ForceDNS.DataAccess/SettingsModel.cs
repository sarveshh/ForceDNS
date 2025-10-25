using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForceDNS.Common;

namespace ForceDNS.DataAccess
{
    class SettingsModel : ISettings
    {
        public String TimeZoneID { get; set; }
        public Boolean Unkillable { get; set; }
        public DateTime RunUntil { get; set; }
        public String PrimaryDns { get; set; }
        public String SecondaryDns { get; set; }
        public DnsProvider DnsProvider { get; set; }
    }
}
