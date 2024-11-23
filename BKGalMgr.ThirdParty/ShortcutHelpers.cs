using System;
using System.IO;
using IWshRuntimeLibrary;

namespace BKGalMgr.ThirdParty;

public static class ShortcutHelpers
{
    public static bool CreateShortcut(string path, string pathToTarget)
    {
        return ShareX.HelpersLib.ShortcutHelpers.SetShortcut(true, GetShortcutPath(path), pathToTarget);
    }

    public static string GetShortcutTargetPath(string shortcutPath)
    {
        WshShell shell = new WshShell();
        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
        return shortcut.TargetPath;
    }

    public static string GetShortcutPath(string path)
    {
        if (!path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            path += ".lnk";

        return path;
    }
}
