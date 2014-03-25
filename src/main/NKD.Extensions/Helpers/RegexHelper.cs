using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace NKD.Helpers
{
    public static class RegexHelper
    {
        public static bool IsMobile(this string mobile)
        {
            Regex r = new Regex(@"^\+[0-9]*$");
            return r.IsMatch(mobile);
        }

        public static bool IsEmail(this string email)
        {
            Regex r = new Regex(@"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
                                 + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
                                 + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
                                 + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,40})$");
            return r.IsMatch(email);
        }

    }
}