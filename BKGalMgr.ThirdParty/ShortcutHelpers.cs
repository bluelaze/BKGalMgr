using System;
using System.IO;
using IWshRuntimeLibrary;

namespace BKGalMgr.ThirdParty;

public static class ShortcutHelpers
{
    public static string GetShortcutTargetPath(string shortcutPath)
    {
        WshShell shell = new WshShell();
        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
        return shortcut.TargetPath;
    }
}
