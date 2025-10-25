using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ForceDNS.Common;

namespace ForceDNS.UI
{
    public class WCFClient : ClientBase<IWCF>, IWCF
    {
        public WCFClient(BasicHttpBinding binding, EndpointAddress address)
         : base(binding, address)
        {

        }

        public void ApplyDns(params String[] dns)
        {
            Channel.ApplyDns(dns);
        }

        public SettingsDto GetSettings()
        {
            return Channel.GetSettings();
        }

        public void RemoveDns()
        {
            Channel.RemoveDns();
        }

        public void SaveSettings(SettingsDto settings)
        {
            Channel.SaveSettings(settings);
        }

        public void SettingsChanged()
        {
            Channel.SettingsChanged();
        }
    }
}
