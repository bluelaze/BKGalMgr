using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;

namespace BKGalMgr.Extensions;

public static class WindowExtension
{
    public static void ApplyTheme(this Window window, ElementTheme theme)
    {
        if (window.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme;
        }
    }

    public static ElementTheme RequestedTheme(this Window window)
    {
        if (window.Content is FrameworkElement rootElement)
            return rootElement.RequestedTheme;

        return ElementTheme.Default;
    }

    public static ElementTheme ActualTheme(this Window window)
    {
        if (window.Content is FrameworkElement rootElement)
            return rootElement.ActualTheme;

        return ElementTheme.Default;
    }

    public static IntPtr GetWindowHandle(this Window window)
    {
        return WinRT.Interop.WindowNative.GetWindowHandle(window);
    }

    public static WindowId GetWindowId(this Window window)
    {
        return Win32Interop.GetWindowIdFromWindow(window.GetWindowHandle());
    }

    // https://stackoverflow.com/questions/71546846/open-app-always-in-the-center-of-the-display-windows-11-winui-3
    public static void CenterToScreen(this Window window)
    {
        if (window.AppWindow is not null)
        {
            DisplayArea displayArea = DisplayArea.GetFromWindowId(window.GetWindowId(), DisplayAreaFallback.Nearest);
            if (displayArea is not null)
            {
                var CenteredPosition = window.AppWindow.Position;
                CenteredPosition.X = ((displayArea.WorkArea.Width - window.AppWindow.Size.Width) / 2);
                CenteredPosition.Y = ((displayArea.WorkArea.Height - window.AppWindow.Size.Height) / 2);
                window.AppWindow.Move(CenteredPosition);
            }
        }
    }

    public static double GetRasterizationScaleForElement(this Window window, UIElement element)
    {
        if (element.XamlRoot != null)
        {
            if (element.XamlRoot == window.Content.XamlRoot)
            {
                return element.XamlRoot.RasterizationScale;
            }
        }
        return 1;
    }

    public static bool IsWindowCloaked(IntPtr hWnd)
    {
        DwmGetWindowAttribute(hWnd, 14, out uint isCloaked, Marshal.SizeOf(typeof(uint)));
        return isCloaked != 0;
    }

    // Window attributes
    //enum DWMWINDOWATTRIBUTE
    //{
    //    DWMWA_NCRENDERING_ENABLED = 1,              // [get] Is non-client rendering enabled/disabled
    //    DWMWA_NCRENDERING_POLICY,                   // [set] DWMNCRENDERINGPOLICY - Non-client rendering policy
    //    DWMWA_TRANSITIONS_FORCEDISABLED,            // [set] Potentially enable/forcibly disable transitions
    //    DWMWA_ALLOW_NCPAINT,                        // [set] Allow contents rendered in the non-client area to be visible on the DWM-drawn frame.
    //    DWMWA_CAPTION_BUTTON_BOUNDS,                // [get] Bounds of the caption button area in window-relative space.
    //    DWMWA_NONCLIENT_RTL_LAYOUT,                 // [set] Is non-client content RTL mirrored
    //    DWMWA_FORCE_ICONIC_REPRESENTATION,          // [set] Force this window to display iconic thumbnails.
    //    DWMWA_FLIP3D_POLICY,                        // [set] Designates how Flip3D will treat the window.
    //    DWMWA_EXTENDED_FRAME_BOUNDS,                // [get] Gets the extended frame bounds rectangle in screen space
    //    DWMWA_HAS_ICONIC_BITMAP,                    // [set] Indicates an available bitmap when there is no better thumbnail representation.
    //    DWMWA_DISALLOW_PEEK,                        // [set] Don't invoke Peek on the window.
    //    DWMWA_EXCLUDED_FROM_PEEK,                   // [set] LivePreview exclusion information
    //    DWMWA_CLOAK,                                // [set] Cloak or uncloak the window
    //    DWMWA_CLOAKED,                              // [get] Gets the cloaked state of the window
    //    DWMWA_FREEZE_REPRESENTATION,                // [set] BOOL, Force this window to freeze the thumbnail without live update
    //    DWMWA_PASSIVE_UPDATE_MODE,                  // [set] BOOL, Updates the window only when desktop composition runs for other reasons
    //    DWMWA_USE_HOSTBACKDROPBRUSH,                // [set] BOOL, Allows the use of host backdrop brushes for the window.
    //    DWMWA_USE_IMMERSIVE_DARK_MODE = 20,         // [set] BOOL, Allows a window to either use the accent color, or dark, according to the user Color Mode preferences.
    //    DWMWA_WINDOW_CORNER_PREFERENCE = 33,        // [set] WINDOW_CORNER_PREFERENCE, Controls the policy that rounds top-level window corners
    //    DWMWA_BORDER_COLOR,                         // [set] COLORREF, The color of the thin border around a top-level window
    //    DWMWA_CAPTION_COLOR,                        // [set] COLORREF, The color of the caption
    //    DWMWA_TEXT_COLOR,                           // [set] COLORREF, The color of the caption text
    //    DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,       // [get] UINT, width of the visible border around a thick frame window
    //    DWMWA_SYSTEMBACKDROP_TYPE,                  // [get, set] SYSTEMBACKDROP_TYPE, Controls the system-drawn backdrop material of a window, including behind the non-client area.
    //    DWMWA_REDIRECTIONBITMAP_ALPHA,              // [set] BOOL, GDI redirection bitmap contains premultiplied alpha
    //    DWMWA_BORDER_MARGINS,                       // [set] FRAME_MARGIN, Override location of window border (distance from each edge)
    //    DWMWA_LAST
    //};
    [DllImport("dwmapi.dll")]
    public static extern int DwmGetWindowAttribute(IntPtr hWnd, int dwAttribute, out uint pvAttribute, int cbAttribute);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern int GetDpiForWindow(IntPtr hWnd);

    public static double GetWindowScale(this Window window)
    {
        var dpi = GetDpiForWindow(window.GetWindowHandle());
        var scalingFactor = (double)dpi / 96;

        return scalingFactor;
    }

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetForegroundWindow();
}
