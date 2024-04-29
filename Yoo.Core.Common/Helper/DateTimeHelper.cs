using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Common.Helper
{
    public static class DateTimeConverter
    {
        public  static readonly TimeZoneInfo gmt8 = TimeZoneInfo.CreateCustomTimeZone("GMT+8", TimeSpan.FromHours(8), "China Standard Time", "(UTC+8)China Standard Time");

        public static long ToUnixTime(DateTime datetime)
        {
            DateTime dateTimeUtc = datetime;
            if (datetime.Kind != DateTimeKind.Utc)
            {
                dateTimeUtc = datetime.ToUniversalTime();
            }

            if (dateTimeUtc.ToUniversalTime() <= DateTime.UnixEpoch)
            {
                return 0;
            }

            return (long)(dateTimeUtc - DateTime.UnixEpoch).TotalMilliseconds;
        }

        public static DateTime ToDateTime(double unixTimestamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime time = dtDateTime.AddSeconds(unixTimestamp);
            return TimeZoneInfo.ConvertTimeFromUtc(time, gmt8);
        }

        public static DateTime ToDateTime(double unixTimestamp, int timezone)
        {
            DateTime time = DateTime.UnixEpoch.AddMilliseconds(unixTimestamp);
            return time.AddHours(timezone);
        }
    }
}
