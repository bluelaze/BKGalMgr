using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Microsoft.Windows.ApplicationModel.Resources;

namespace BKGalMgr.Helpers;

public enum SupportLanguages
{
    system,
    en_US,
    zh_Hans,
}

public class LanguageHelper
{
    private static ResourceLoader _resourceLoader = new();

    public static string GetString(string reswKey)
    {
        return _resourceLoader.GetString(reswKey);
    }

    public static Dictionary<SupportLanguages, string> GetSupportLangugesName()
    {
        Dictionary<SupportLanguages, string> langNames = new();
        langNames.Add(SupportLanguages.system, GetString("Settings_Generic_Language_System"));

        foreach (SupportLanguages item in Enum.GetValues(typeof(SupportLanguages)))
        {
            if (item == SupportLanguages.system)
                continue;
            Windows.Globalization.Language lan = new(item.ToString().Replace("_", "-"));
            langNames.Add(item, lan.NativeName);
        }

        return langNames;
    }
}
