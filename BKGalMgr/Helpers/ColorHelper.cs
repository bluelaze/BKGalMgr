using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Helpers;
using Windows.UI;

namespace BKGalMgr.Helpers;

public static class ColorHelper
{
    public static Color ToColor(this System.Drawing.Color color)
    {
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public static System.Drawing.Color ToColor(this Color color)
    {
        color.ToString();
        return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public static HslColor ToHsl(this System.Drawing.Color color)
    {
        return CommunityToolkit.WinUI.Helpers.ColorHelper.ToHsl(color.ToColor());
    }

    public static Color ToColor(this HslColor hslColor)
    {
        return CommunityToolkit.WinUI.Helpers.ColorHelper.FromHsl(hslColor.H, hslColor.S, hslColor.L, hslColor.A);
    }

    public static Color GetImagePrimaryColor(string imagePath, bool isDark = false)
    {
        using var bitmap = new System.Drawing.Bitmap(imagePath);
        var colorThief = new ColorThiefDotNet.ColorThief();
        var qColors = colorThief.GetPalette(bitmap, 8);
        var qColor = isDark ? qColors.FirstOrDefault(c => c.IsDark) : qColors.FirstOrDefault();
        var retColor = Color.FromArgb(qColor.Color.A, qColor.Color.R, qColor.Color.G, qColor.Color.B);
        if (!qColor.IsDark && isDark)
        {
            retColor = DevWinUI.ColorHelper.DarkenColor(retColor, 0.5f);
        }
        return retColor;
    }

    public static Color GenerateLighterOrDarkerColor(Color color, bool isLighter, float percent = 0.2f)
    {
        return isLighter
            ? DevWinUI.ColorHelper.LightenColor(color, percent)
            : DevWinUI.ColorHelper.DarkenColor(color, percent);
    }
}
