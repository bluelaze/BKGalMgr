using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
