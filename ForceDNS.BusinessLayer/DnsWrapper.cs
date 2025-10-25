using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ForceDNS.BusinessLayer
{
    public static class DnsWrapper
    {
        public static List<String> NetworkDescriptions { get; set; }
        private static void FlushDns()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ipconfig",
                Arguments = $"/flushdns",
                CreateNoWindow = true,
                Verb = "runas",
                UseShellExecute = false
            }).WaitForExit();
        }

        public static void AddDnsRule(string[] Dns)
        {
            String[] dns = null;

            if (String.IsNullOrWhiteSpace(Dns[0]) == false && Dns[1] != "...")
                dns = new string[] { Dns[0], Dns[1] };
            else
                dns = new string[] { Dns[0] };

            if (NetworkDescriptions == null)
                NetworkDescriptions = new List<string>();

            NetworkObject networkObject = new NetworkObject();

            var CurrentInterface = networkObject.GetActiveNetworkInterface();

            if (CurrentInterface == null)
            {
                Log.Error("Current Interface is null");
                return;
            }

            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    if (objMO["Description"].ToString().Equals(CurrentInterface.Description))
                    {
                        ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                        if (objdns != null)
                        {
                            //dns è stato eliminato o modificato -> ripristinare dns scelto
                            objdns["DNSServerSearchOrder"] = dns;

                            objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);

                            String networkDescription = NetworkDescriptions.Where(ntwDescr => ntwDescr == CurrentInterface.Description).FirstOrDefault();

                            if (networkDescription is null)
                                NetworkDescriptions.Add(CurrentInterface.Description);

                            FlushDns();
                        }
                    }
                }
            }
        }

        public static void RemoveDnsRule()
        {
            if (NetworkDescriptions == null)
                NetworkDescriptions = new List<string>();

            NetworkObject networkObject = new NetworkObject();

            var CurrentInterface = networkObject.GetActiveNetworkInterface();

            if (CurrentInterface == null)
            {
                //log null network interface
                return;
            }

            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if (NetworkDescriptions.Any(ntwDescription => ntwDescription == objMO["Description"].ToString()))
                {
                    ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                    if (objdns != null)
                    {
                        objdns["DNSServerSearchOrder"] = null;
                        objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                    }
                }
            }

            NetworkDescriptions.Clear();

            FlushDns();

        }
    }

    public class NetworkObject
    {
        public NetworkInterface GetActiveNetworkInterface()
        {
            var asd = NetworkInterface.GetAllNetworkInterfaces();
            return asd.FirstOrDefault(a => a.OperationalStatus == OperationalStatus.Up && (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) && a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));
        }
    }
}