using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.Extensions;

public static class TimeSpanExtensions
{
    public static string Format(this TimeSpan ts, string format)
    {
        if (format == "hhmmss")
            return string.Format(@"{0:00}{1}", (int)ts.TotalHours, ts.ToString(@"\:mm\:ss"));
        else
            return ts.ToString(format);
    }
}
