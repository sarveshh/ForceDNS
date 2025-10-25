using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForceDNS.Common;

namespace ForceDNS.DataAccess
{
    public class Settings
    {

        internal static void KeepFileOpen()
        {
            FileConfigToggler.ToggleFileConfigOpen(FileConfigAction.Open);
        }

        internal static void CloseFile()
        {
            FileConfigToggler.ToggleFileConfigOpen(FileConfigAction.Close);
        }

        public static ISettings LoadSettings()
        {
            CloseFile();

            var runUntil = ApplicationSettingsHelper.GetValue<long>("RunUntil");
            var timeZoneID = ApplicationSettingsHelper.GetValue<String>("TimeZoneID");
            var unkillable = ApplicationSettingsHelper.GetValue<Boolean>("Unkillable");
            var primaryDns = ApplicationSettingsHelper.GetValue<String>("PrimaryDns");
            var secondaryDns = ApplicationSettingsHelper.GetValue<String>("SecondaryDns");
            var dnsProvider = ApplicationSettingsHelper.GetValue<Int32>("DnsProvider");
            KeepFileOpen();

            return new SettingsModel()
            {
                RunUntil = runUntil <= 0 ? DateTime.Now.AddDays(-1) : new DateTime(runUntil),
                TimeZoneID = timeZoneID,
                Unkillable = unkillable,
                DnsProvider = (DnsProvider)dnsProvider,
                PrimaryDns = primaryDns,
                SecondaryDns = secondaryDns
            };
        }

        public static void SaveSettings(ISettings settings)
        {
            CloseFile();

            ApplicationSettingsHelper.UpdateAppSettings("RunUntil", settings.RunUntil.Ticks.ToString());
            ApplicationSettingsHelper.UpdateAppSettings("TimeZoneID", settings.TimeZoneID);
            ApplicationSettingsHelper.UpdateAppSettings("Unkillable", settings.Unkillable == true ? "1" : "0");
            ApplicationSettingsHelper.UpdateAppSettings("PrimaryDns", settings.PrimaryDns);
            ApplicationSettingsHelper.UpdateAppSettings("SecondaryDns", settings.SecondaryDns);
            ApplicationSettingsHelper.UpdateAppSettings("DnsProvider", Convert.ToInt32(settings.DnsProvider).ToString());

            KeepFileOpen();
        }
    }
}
