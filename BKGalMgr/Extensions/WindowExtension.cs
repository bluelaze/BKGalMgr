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

    [DllImport("User32.dll")]
    internal static extern int GetDpiForWindow(IntPtr hwnd);

    public static double GetWindowScale(this Window window)
    {
        var dpi = GetDpiForWindow(window.GetWindowHandle());
        var scalingFactor = (double)dpi / 96;

        return scalingFactor;
    }
}
