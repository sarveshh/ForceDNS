using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using ForceDNS.BusinessLayer;
using ForceDNS.Common;
using ForceDNS.DataAccess;

namespace ForceDNS.Service
{
    public class ServiceHelper
    {

        static ServiceHost host = null;

        public static void Initialize()
        {
            Log.Information("ServiceHelper Initialize called");

            if (UserHasAdminPrivileges() == false)
                return;

            ISettings s = Settings.LoadSettings();

            //TODO remove dns
            //FirewallRule.AllowConnection(true);

            RegistryWrapper.StartRegistryMonitor();

            ForceDNSJob.Begin(s);

            Log.Information("Initializing WCF Host");

            host = new ServiceHost(typeof(WCF), new Uri("http://localhost:8022/FDService"));

            ServiceMetadataBehavior smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>();

            if (smb == null)
                smb = new ServiceMetadataBehavior();

            host.Description.Behaviors.Add(smb);

            host.AddServiceEndpoint(typeof(IWCF), new BasicHttpBinding(), "");

            host.Open();

            Log.Information("WCF Host status = " + host.State);
        }


        public static void Stop()
        {
            Log.Information("ServiceHelper Stop called");

            ISettings s = Settings.LoadSettings();

            CriticalProcessBase.SetProcessAsNotCritical(s);

            //TODO Remove DNS FirewallRule.AllowConnection(true);

            RegistryWrapper.StopRegistryMonitor();
            
            RegistryWrapper.RestoreDefaultSettings(true);

            Log.Information("Closing WCF Host");

            host?.Close();

            if(host is null)
                Log.Information("Host was null");

            if(host != null)
                Log.Information("WCF Host status = " + host.State);

        }

        public static void Refresh()
        {
            Log.Information("ServiceHelper Refresh called");

            RegistryWrapper.RestoreDefaultSettings(true);

            ISettings s = Settings.LoadSettings();

            ForceDNSJob.End();
            ForceDNSJob.Begin(s);

        }


        public static void HandleSessionChanged(SessionChangeReason reason)
        {
            Log.Information("ServiceHelper HandleSessionChanged called");

            Refresh();
        }

        public static Boolean UserHasAdminPrivileges()
        {
            Log.Information("Checking if current user has admin privileges");

            using (System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent())
            {
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);

                Boolean isAdmin = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);

                Log.Information($"{principal.Identity.Name} is admin = {isAdmin}");

                return isAdmin;
            }
        }
    }
}
