using Serilog;
using ForceDNS.BusinessLayer;
using ForceDNS.Common;

namespace ForceDNS.Service
{
    internal class WCF : IWCF
    {
        public SettingsDto GetSettings()
        {
            try
            {
                Log.Information("GetSettings called from WCF Client");

                ISettings s = DataAccess.Settings.LoadSettings();

                return new SettingsDto()
                {
                    RunUntil = s.RunUntil,
                    TimeZoneID = s.TimeZoneID,
                    Unkillable = s.Unkillable,
                    PrimaryDns = s.PrimaryDns,
                    SecondaryDns = s.SecondaryDns,
                    DnsProvider = s.DnsProvider
                };
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "An error occurred while a WCF client called GetSettings");

                return new SettingsDto();
            }
        }

        public void SaveSettings(SettingsDto settings)
        {
            try
            {
                Log.Information($"SaveSettings called from WCF Client. Saving Settings -> {settings}");

                ISettings s = settings;

                DataAccess.Settings.SaveSettings(s);
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "An error occurred while a WCF client called SaveSettings");
            }
        }

        public void SettingsChanged()
        {
            try
            {
                Log.Information("SettingsChanged called from WCF Client");
                
                ServiceHelper.Refresh();
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "An error occurred while a WCF client called SettingsChanged");
            }
        }

        public void ApplyDns(params string[] dns)
        {
            DnsWrapper.AddDnsRule(dns);
        }
        public void RemoveDns()
        {
            DnsWrapper.RemoveDnsRule();
        }
    }
}