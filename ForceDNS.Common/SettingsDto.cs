using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ForceDNS.Common
{
    [DataContract]
    public class SettingsDto : ISettings
    {
        [DataMember]
        public String TimeZoneID { get; set; }

        [DataMember]
        public Boolean Unkillable { get; set; }

        [DataMember]
        public DateTime RunUntil { get; set; }

        [DataMember]
        public String PrimaryDns { get; set; }
        [DataMember]
        public String SecondaryDns { get; set; }
        [DataMember]
        public DnsProvider DnsProvider { get; set; }

        public override String ToString()
        {
            return $"TimeZone = '{this.TimeZoneID}', " +
                $"Unkillable = '{this.Unkillable}', " +
                $"Primary DNS = '{this.PrimaryDns}', " +
                $"Secondary DNS = '{this.SecondaryDns}', " +
                $"Dns Provider = '{this.DnsProvider}', " +
                $"RunUntil = '{this.RunUntil}'";
        }

    }
}
