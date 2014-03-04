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

    }
}