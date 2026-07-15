using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Helpers;

namespace BKGalMgr.Helpers;

public static class ColorHelper
{
    public static Windows.UI.Color ToWindowsUIColor(this Color color)
    {
        return Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public static Color ToColor(this Windows.UI.Color color)
    {
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public static HslColor ToHsl(this Color color)
    {
        return color.ToWindowsUIColor().ToHsl();
    }

    public static Color ToColor(this HslColor hslColor)
    {
        return CommunityToolkit
            .WinUI.Helpers.ColorHelper.FromHsl(hslColor.H, hslColor.S, hslColor.L, hslColor.A)
            .ToColor();
    }

    private class ColorBucket
    {
        public long Count { get; set; }
        public long RedSum { get; set; }
        public long GreenSum { get; set; }
        public long BlueSum { get; set; }

        public Color GetAverageColor()
        {
            if (Count == 0)
                return Color.Black;
            return Color.FromArgb((int)(RedSum / Count), (int)(GreenSum / Count), (int)(BlueSum / Count));
        }
    }

    public static Color GetImagePrimaryColor(string imagePath)
    {
        using var bitmap = new Bitmap(imagePath);
        var colorThief = new ColorThiefDotNet.ColorThief();
        var qColor =
            colorThief.GetPalette(bitmap, 8).Where(t => t.IsDark)?.FirstOrDefault() ?? colorThief.GetColor(bitmap);
        return Color.FromArgb(qColor.Color.A, qColor.Color.R, qColor.Color.G, qColor.Color.B);
    }

    public static bool IsDarkColor(Color color, double luminanceThreshold = 0.3)
    {
        var hslColor = color.ToHsl();
        return hslColor.L < luminanceThreshold;
    }

    public static Color GenerateLighterOrDarkerColor(
        Color baseColor,
        bool isLighter = true,
        double luminanceIncrement = 0.2
    )
    {
        var hslColor = baseColor.ToHsl();

        // 调整亮度
        hslColor.L = isLighter
            ? Math.Min(hslColor.L + luminanceIncrement, 1)
            : Math.Max(hslColor.L - luminanceIncrement, 0.0);

        // 转换回RGB
        return hslColor.ToColor();
    }
}
