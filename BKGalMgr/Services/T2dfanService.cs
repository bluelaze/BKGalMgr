using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.Services;

public class T2DFanService
{
    private static string _2dfanWebsit = "https://2dfan.com";

    public static void OpenSubjectPage(string subjectId)
    {
        Process.Start("explorer.exe", $"{_2dfanWebsit}/subjects/{subjectId}");
    }
}
