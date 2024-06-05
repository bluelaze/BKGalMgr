using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NHotkey.Wpf;
using Windows.System;

namespace BKGalMgr.ThirdParty;

public class Hotkey
{
    public class HotkeyEventArgs : EventArgs
    {
        private readonly string _name;

        public HotkeyEventArgs(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public bool Handled { get; set; }
    }

    public delegate void OnHothey(HotkeyEventArgs e);

    public static bool AddOrReplace(string name, Key key, ModifierKeys modifiers, OnHothey handler)
    {
        try
        {
            HotkeyManager.Current.AddOrReplace(
                name,
                key,
                modifiers,
                (_, e) =>
                {
                    var args = new HotkeyEventArgs(e.Name) { Handled = e.Handled };
                    handler.Invoke(args);
                    e.Handled = args.Handled;
                }
            );
        }
        catch (Exception e)
        {
            return false;
        }
        return true;
    }

    public static void Remove(string name)
    {
        HotkeyManager.Current.Remove(name);
    }
}
