using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BKGalMgr.ThirdParty;

namespace BKGalMgr.Helpers;

public class HotkeyHelper
{
    public class HotKeyInfo
    {
        public string Name;
        public Key Key;
        public ModifierKeys Modifiers;
        public bool Used;

        public HotKeyInfo(string name, Key key, ModifierKeys modifiers, bool used)
        {
            Name = name;
            Key = key;
            Modifiers = modifiers;
            Used = used;
        }
    }

    private static ModifierKeys _modifierKey1 = ModifierKeys.Shift;
    private static ModifierKeys _modifiersKey2 = ModifierKeys.Alt;

    // only support 12 hotkeys
    // A,S,D
    // W,E,R
    // Z,X,C
    // F,R,V
    private static List<HotKeyInfo> _keys = new()
    {
        new($"{_modifierKey1}+{_modifiersKey2}+{Key.A}", Key.A, _modifierKey1 | _modifiersKey2, false),
        new($"{_modifierKey1}+{_modifiersKey2}+{Key.S}", Key.S, _modifierKey1 | _modifiersKey2, false),
        new($"{_modifierKey1}+{_modifiersKey2}+{Key.D}", Key.D, _modifierKey1 | _modifiersKey2, false),
        new($"{_modifierKey1}+{_modifiersKey2}+{Key.Q}", Key.Q, _modifierKey1 | _modifiersKey2, false),
        new($"{_modifierKey1}+{_modifiersKey2}+{Key.W}", Key.W, _modifierKey1 | _modifiersKey2, false),
        new($"{_modifierKey1}+{_modifiersKey2}+{Key.E}", Key.E, _modifierKey1 | _modifiersKey2, false),
        new($"{_modifierKey1}+{_modifiersKey2}+{Key.Z}", Key.Z, _modifierKey1 | _modifiersKey2, false),
        new($"{_modifierKey1}+{_modifiersKey2}+{Key.X}", Key.X, _modifierKey1 | _modifiersKey2, false),
        new($"{_modifierKey1}+{_modifiersKey2}+{Key.C}", Key.C, _modifierKey1 | _modifiersKey2, false),
        new($"{_modifierKey1}+{_modifiersKey2}+{Key.F}", Key.F, _modifierKey1 | _modifiersKey2, false),
        new($"{_modifierKey1}+{_modifiersKey2}+{Key.R}", Key.R, _modifierKey1 | _modifiersKey2, false),
        new($"{_modifierKey1}+{_modifiersKey2}+{Key.V}", Key.V, _modifierKey1 | _modifiersKey2, false),
    };

    public static HotKeyInfo GetUnusedKey()
    {
        foreach (var key in _keys)
        {
            if (!key.Used)
                return key;
        }
        return null;
    }

    public static string AddOrReplace(Hotkey.OnHothey handler)
    {
        string hotkeyName = "";

        for (HotKeyInfo keyInfo = GetUnusedKey(); keyInfo != null; keyInfo = GetUnusedKey())
        {
            if (Hotkey.AddOrReplace(keyInfo.Name, keyInfo.Key, keyInfo.Modifiers, handler))
            {
                keyInfo.Used = true;
                hotkeyName = keyInfo.Name;
                break;
            }
            else
            {
                // register by others
                keyInfo.Used = true;
            }
        }

        return hotkeyName;
    }

    public static void Remove(string name)
    {
        Hotkey.Remove(name);
        var keyInfo = _keys.Find((k) => k.Name == name);
        if (keyInfo != null)
            keyInfo.Used = false;
    }

    public static bool AddGlobalMouseDown(string name, Hotkey.OnHothey handler)
    {
        return Hotkey.AddGlobalMouseDown(name, handler);
    }

    public static void RemoveGlobalMouseDown(string name)
    {
        Hotkey.RemoveGlobalMouseDown(name);
    }
}
