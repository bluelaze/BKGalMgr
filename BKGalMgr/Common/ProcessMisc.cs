using System;
using System.Collections.Generic;
using System.Text;
using BKGalMgr.Enums;
using Microsoft.Win32;

namespace BKGalMgr.Common;

public static class ProcessMisc
{
    private const string RegistryPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";

    public static void SetDpiOverride(string exePath, DpiOverrideMode mode)
    {
        if (string.IsNullOrWhiteSpace(exePath))
            return;

        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath);
        switch (mode)
        {
            case DpiOverrideMode.None:
                key.DeleteValue(exePath, throwOnMissingValue: false);
                break;
            case DpiOverrideMode.Application:
                key.SetValue(exePath, "~ HIGHDPIAWARE", RegistryValueKind.String);
                break;
            case DpiOverrideMode.System:
                key.SetValue(exePath, "~ DPIUNAWARE", RegistryValueKind.String);
                break;
            case DpiOverrideMode.SystemEnhanced:
                key.SetValue(exePath, "~ GDIDPISCALING DPIUNAWARE", RegistryValueKind.String);
                break;
        }
    }

    public static DpiOverrideMode GetDpiOverride(string exePath)
    {
        using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: false);
        string value = key?.GetValue(exePath) as string;

        return value switch
        {
            "~ HIGHDPIAWARE" => DpiOverrideMode.Application,
            "~ DPIUNAWARE" => DpiOverrideMode.System,
            "~ GDIDPISCALING DPIUNAWARE" => DpiOverrideMode.SystemEnhanced,
            _ => DpiOverrideMode.None,
        };
    }
}
