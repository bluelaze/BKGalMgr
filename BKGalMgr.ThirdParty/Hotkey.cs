using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Gma.System.MouseKeyHook;
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

        public bool IsMouseEvent { get; set; }

        public bool RightButton { get; set; }

        public bool MiddleButton { get; set; }

        public bool LeftButton { get; set; }

        public bool XButton1 { get; set; }

        public bool XButton2 { get; set; }

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
                    var args = new HotkeyEventArgs(e.Name) { IsMouseEvent = false, Handled = e.Handled };
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

    private static Dictionary<string, IKeyboardMouseEvents> _keyboardEvents = new();

    public static bool AddGlobalMouseDown(string name, OnHothey handler)
    {
        if (_keyboardEvents.ContainsKey(name))
        {
            return false;
        }

        var globalHook = Hook.GlobalEvents();
        globalHook.MouseDownExt += (object? sender, MouseEventExtArgs e) =>
        {
            var args = new HotkeyEventArgs(name) { IsMouseEvent = true, Handled = e.Handled };
            switch (e.Button)
            {
                case MouseButtons.None:
                    break;
                case MouseButtons.Left:
                    args.LeftButton = e.Clicked;
                    break;
                case MouseButtons.Right:
                    args.RightButton = e.Clicked;
                    break;
                case MouseButtons.Middle:
                    args.MiddleButton = e.Clicked;
                    break;
                case MouseButtons.XButton1:
                    args.XButton1 = e.Clicked;
                    break;
                case MouseButtons.XButton2:
                    args.XButton2 = e.Clicked;
                    break;
            }
            handler.Invoke(args);
            e.Handled = args.Handled;
        };
        _keyboardEvents.TryAdd(name, globalHook);
        return true;
    }

    public static void RemoveGlobalMouseDown(string name)
    {
        if (_keyboardEvents.ContainsKey(name))
        {
            _keyboardEvents[name].Dispose();
            _keyboardEvents.Remove(name);
        }
    }
}
