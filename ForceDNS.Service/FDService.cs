using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using ForceDNS.BusinessLayer;
using ForceDNS.Common;

namespace ForceDNS.Service
{
    public partial class FDservice : ServiceBase
    {
        public FDservice()
        {
            InitializeComponent();
        }

        internal void OnDebug(string[] args)
        {
            OnStart(args);
        }

        protected override void OnStart(string[] args)
        {
            Log.Information("OnStart Called");

            base.OnStart(args);

            try
            {
                ServiceHelper.Initialize();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while initializing the service");
            }
        }

        protected override void OnShutdown()
        {
            Log.Information("OnShutdown Called");

            base.OnShutdown();

            ServiceHelper.Stop();
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            Log.Information($"OnSessionChange Called, Reason = {changeDescription.Reason}");

            base.OnSessionChange(changeDescription);

            ServiceHelper.HandleSessionChanged(changeDescription.Reason);
        }

        protected override Boolean OnPowerEvent(PowerBroadcastStatus powerStatus)
        {

            switch (powerStatus)
            {
                case PowerBroadcastStatus.OemEvent:
                case PowerBroadcastStatus.Suspend:
                case PowerBroadcastStatus.ResumeSuspend:
                case PowerBroadcastStatus.QuerySuspend:
                case PowerBroadcastStatus.QuerySuspendFailed:
                case PowerBroadcastStatus.ResumeAutomatic:
                case PowerBroadcastStatus.ResumeCritical:
                
                    Log.Information($"OnPowerEvent Called, PowerStatus = {powerStatus}");
                    
                    break;
                default:
                    break;
            }

            return base.OnPowerEvent(powerStatus);
        }
    }
}
