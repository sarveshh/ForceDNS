using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ForceDNS.Common
{
    [ServiceContract]
    public interface IWCF
    {
        [OperationContract]
        void SettingsChanged();

        [OperationContract]
        SettingsDto GetSettings();

        [OperationContract]
        void SaveSettings(SettingsDto settings);

        [OperationContract]
        void ApplyDns(params string[] dns);

        [OperationContract]
        void RemoveDns();

    }
}
