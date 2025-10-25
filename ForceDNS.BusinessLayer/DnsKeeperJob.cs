using ForceDNS.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ForceDNS.BusinessLayer
{
    public static class DnsKeeperJob 
    {
        static Timer DnsKeeperTimer;
        static String[] Dns;
        public static void Begin(ISettings settings)
        {
            Log.Information("DnsKeeper Job Starting");

            DnsKeeperTimer = new Timer
            {
                AutoReset = false,
                Interval = 10 * 1000
            };

            Dns = new string[] { settings.PrimaryDns, settings.SecondaryDns };

            DnsKeeperTimer.Elapsed += DnsKeeperJob_Elapsed;

            DnsKeeperTimer.Enabled = true;

            Log.Information("DnsKeeper Job Started = " + DnsKeeperTimer.Enabled);

        }

        private static void DnsKeeperJob_Elapsed(Object sender, ElapsedEventArgs e)
        {
            DnsKeeperTimer.Enabled = false;

            DnsWrapper.AddDnsRule(Dns);

            DnsKeeperTimer.Enabled = true;
        }

        public static void End()
        {
            Log.Information("DnsKeeper Job Ending");

            if(DnsKeeperTimer != null)
                DnsKeeperTimer.Enabled = false;

            DnsWrapper.RemoveDnsRule();
            DnsKeeperTimer?.Stop();
            DnsKeeperTimer?.Dispose();
            DnsKeeperTimer?.Close();
        }
    }
}
