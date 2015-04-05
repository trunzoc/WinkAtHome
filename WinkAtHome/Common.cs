using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WinkAtHome
{
    public class Common
    {
        public static DateTime FromUnixTime(string unixTime)
        {
            Double longTime;
            bool converted = Double.TryParse(unixTime, out longTime);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(longTime);
        }
    }
}