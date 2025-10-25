using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ForceDNS.Common
{
    public interface ISettings
    {
        String TimeZoneID { get; set; }
        Boolean Unkillable { get; set; }
        DateTime RunUntil { get; set; }
        String PrimaryDns { get; set; }
        String SecondaryDns { get; set; }
        DnsProvider DnsProvider { get; set; }
    }
}
