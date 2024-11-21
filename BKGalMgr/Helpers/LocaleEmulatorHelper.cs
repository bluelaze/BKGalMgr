using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BKGalMgr.ThirdParty;
using BKGalMgr.ViewModels;
using Microsoft.Win32;

namespace BKGalMgr.Helpers;

// https://github.com/xupefei/Locale-Emulator
public class LocaleEmulatorHelper
{
    private static string _clsid = "{C52B9871-E5E9-41FD-B84D-C5ACADBEC7AE}";
    private static string _keyName = $@"SOFTWARE\Classes\CLSID\{_clsid}\InprocServer32";

    public static string GetLEProcInstalledPath()
    {
        var register = Registry.CurrentUser.OpenSubKey(_keyName, false);
        if (register == null)
            register = Registry.LocalMachine.OpenSubKey(_keyName, false);

        if (register != null)
        {
            var handlerPath = register.GetValue("CodeBase") as string;
            if (handlerPath != null)
            {
                if (handlerPath.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                    handlerPath = handlerPath.Substring(8);
                handlerPath = Path.GetDirectoryName(handlerPath);
                return Path.Combine(handlerPath, "LEProc.exe");
            }
        }

        return string.Empty;
    }

    public static string GetAppPath(string path)
    {
        if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            return ShortcutHelpers.GetShortcutTargetPath(path);
        return path;
    }

    public static Process RunAs(string leprocPath, string appPath, string guid)
    {
        return Process.Start(leprocPath, $"-runas \"{guid}\" \"{GetAppPath(appPath)}\"");
    }

    public static Process RunDefault(string leprocPath, string appPath)
    {
        return Process.Start(leprocPath, $"-run \"{GetAppPath(appPath)}\"");
    }

    public static Process ManageApp(string leprocPath, string appPath)
    {
        return Process.Start(leprocPath, $"-manage \"{GetAppPath(appPath)}\"");
    }

    public static Process ManageAll(string leprocPath)
    {
        return Process.Start(leprocPath, $"-global");
    }

    public static List<LEProfileInfo> GetProfiles(string leprocPath)
    {
        try
        {
            var dict = XDocument.Load(Path.Combine(Path.GetDirectoryName(leprocPath), "LEConfig.xml"));

            var pros = from i in dict.Descendants("LEConfig").Elements("Profiles").Elements() select i;

            var profiles = pros.Select(p => new LEProfileInfo()
                {
                    Name = p.Attribute("Name").Value,
                    Guid = p.Attribute("Guid").Value
                })
                .ToList();
            // csharpier-ignore-start
            profiles.Add(new LEProfileInfo() { IsSeparator = true, Guid = LEProfileInfo.SeparatorGuid });
            profiles.Add(new LEProfileInfo() { Name = LanguageHelper.GetString("LocalEmulator_Menu_RunDefault"), Guid = LEProfileInfo.RunGuid });
            profiles.Add(new LEProfileInfo() { Name = LanguageHelper.GetString("LocalEmulator_Menu_ManageApp"), Guid = LEProfileInfo.ManageGuid });
            // csharpier-ignore-end

            return profiles;
        }
        catch
        {
            return null;
        }
    }
}
