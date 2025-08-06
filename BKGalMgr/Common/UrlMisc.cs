using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.Common;

public class UrlMisc
{
    public static void OpenUrl(string url)
    {
        // https://stackoverflow.com/a/43232486/25003254
        url = url.Replace("&", "^&");
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
