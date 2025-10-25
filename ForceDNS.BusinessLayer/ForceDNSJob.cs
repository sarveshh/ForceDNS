using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ForceDNS.Common;

namespace ForceDNS.BusinessLayer
{
    public static class ForceDNSJob
    {
        static Timer ForceDNSTimer;
        static ISettings _settings;

        public static void Begin(ISettings settings)
        {
            Log.Information($"ForceDNSJob Starting with the following settings: " +
               $"TimeZone = '{settings.TimeZoneID}', " +
                $"Unkillable = '{settings.Unkillable}', " +
                $"Primary DNS = '{settings.PrimaryDns}', " +
                $"Secondary DNS = '{settings.SecondaryDns}', " +
                $"Dns Provider = '{settings.DnsProvider}', " +
                $"RunUntil = '{settings.RunUntil}'");

            if (settings.RunUntil <= DateTime.Now)
            {
                End();
                return;
            }

            _settings = settings;

            ForceDNSTimer = new Timer
            {
                AutoReset = false,
                Interval = 1 * 1000
            };

            ForceDNSTimer.Elapsed += ForceDNSTimer_Elapsed;
            
            ForceDNSTimer.Enabled = true;

            Log.Information("ForceDNSJob Started = " + ForceDNSTimer.Enabled);

        }

        private static void ForceDNSTimer_Elapsed(Object sender, ElapsedEventArgs e)
        {
            ForceDNSTimer.Enabled = false;

            Log.Information("ForceDNSJob Elapsed, Checking Status...");

            var response = AppStatusService.CheckStatus(_settings).Result;

            CriticalProcessBase.StatusChanged(response);

            Log.Information("ForceDNSJob Responded With: " + response.ToString());

            if(response.Interval == null)
            {
                End();

                return;
            }

            if(response.Status == AppStatus.DNSON)
            {
                DnsKeeperJob.Begin(_settings);
            }
            else if(response.Status == AppStatus.DNSOFF)
            {
                DnsKeeperJob.End();
            }
            else
            {
                Log.Error("Could Not Retrieve Date Time From NTP. Checking again in 30 seconds...");
            }

            ForceDNSTimer.Interval = response.Interval.Value;
            ForceDNSTimer.Enabled = true;
        }

        public static void End()
        {
            Log.Information("ForceDNSJob Ending");

            DnsKeeperJob.End();

            ForceDNSTimer?.Stop();
            ForceDNSTimer?.Dispose();
            ForceDNSTimer?.Close();
        }
    }
}
