using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForceDNS.Common;

namespace ForceDNS.BusinessLayer
{
    public class AppStatusService
    {
        public async static Task<StatusResponse> CheckStatus(ISettings settings)
        {
            return await Task.Run(() =>
            {
                StatusResponse sr = new StatusResponse();

                sr.TimeStamp = DateTime.Now;

                DateTime? now = RemoteDateTime.Now(settings.TimeZoneID);

                if (now.HasValue == false)
                {
                    //offline, check back in 30 sec
                    sr.Interval = 30000;
                    return sr;
                }

                sr.TimeStamp = now.Value;

                if (settings.RunUntil > now.Value)
                {
                    TimeSpan ts = settings.RunUntil - now.Value;

                    sr.Status = AppStatus.DNSON;
                    sr.Interval = ToSafeInterval(ts.TotalMilliseconds);

                }
                else
                {
                    sr.Status = AppStatus.DNSOFF;
                    sr.Interval = null;
                }

                return sr;
            });
        }

        private static Double ToSafeInterval(Double calculatedMilliseconds)
        {
            if (calculatedMilliseconds < 100)
                return 100;

            if (calculatedMilliseconds > Int32.MaxValue)
                return Int32.MaxValue;
            else
                return calculatedMilliseconds;
        }
    }
}
