using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NKD.Helpers
{
    public static class DateHelper
    {
        public const string DefaultDateFormat = "[yyyyMMdd-hhmmss]";
        public static string OnlineDateFormat { get { return DefaultDateFormat; } }
        public static string NowInOnlineFormat { get { return DateTime.UtcNow.ToString(OnlineDateFormat); } }
        /// <summary>
        /// Gets Unix Timestamp
        /// </summary>
        /// <param name="dateTime">UTC!</param>
        /// <returns></returns>
        public static double DateTimeTotimeUnixTimestamp(DateTime dateTime) { return (dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds; }
        public static double NowToUnixTimestamp() { return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds; }
        public static int Timestamp
        {
            get
            {
                return (int)NowToUnixTimestamp();
            }
        }
    }
}