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
        using (var bitmap = new Bitmap(imagePath))
        {
            // 使用较小的量化级别来减少颜色数量
            const int colorBits = 5; // 32个级别
            const int colorMask = (1 << colorBits) - 1;

            var colorHistogram = new ConcurrentDictionary<int, int>();

            // 将图片缩放到较小的尺寸以提高性能
            int targetWidth = bitmap.Width;
            int targetHeight = bitmap.Height;
            if (targetWidth > 360)
            {
                targetWidth = 360;
                targetHeight = (int)((float)bitmap.Height / bitmap.Width * targetWidth);
            }

            using (var scaledBitmap = new Bitmap(bitmap, targetWidth, targetHeight))
            {
                var rect = new Rectangle(0, 0, scaledBitmap.Width, scaledBitmap.Height);
                var bitmapData = scaledBitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    int bytesPerPixel = 4;
                    var stride = bitmapData.Stride;
                    var width = bitmapData.Width;
                    var height = bitmapData.Height;

                    int bytes = Math.Abs(stride) * bitmapData.Height;
                    byte[] rgbValues = new byte[bytes];
                    Marshal.Copy(bitmapData.Scan0, rgbValues, 0, bytes);

                    // 使用SIMD和并行处理
                    Parallel.For(
                        0,
                        height,
                        y =>
                        {
                            for (var x = 0; x < width; x++)
                            {
                                int idx = y * stride + x * bytesPerPixel;
                                var b = rgbValues[idx];
                                var g = rgbValues[idx + 1];
                                var r = rgbValues[idx + 2];
                                var a = rgbValues[idx + 3];

                                // 忽略接近透明的像素
                                if (a < 127)
                                    continue;

                                // 忽略接近白色
                                int threshold = 50;
                                if (r >= 255 - threshold && g >= 255 - threshold && b >= 255 - threshold)
                                    continue;

                                // 量化颜色
                                var quantR = (r >> (8 - colorBits)) & colorMask;
                                var quantG = (g >> (8 - colorBits)) & colorMask;
                                var quantB = (b >> (8 - colorBits)) & colorMask;

                                // 计算颜色
                                var colorKey = (quantR << (2 * colorBits)) | (quantG << colorBits) | quantB;

                                colorHistogram.AddOrUpdate(colorKey, 1, (_, count) => count + 1);
                            }
                        }
                    );
                }
                finally
                {
                    scaledBitmap.UnlockBits(bitmapData);
                }
            }

            // 找出最常见的颜色
            if (!colorHistogram.Any())
                return Color.Black;

            var primaryColors = colorHistogram.OrderByDescending(x => x.Value).ToList();

            ColorBucket primaryColorBucketR = new();
            ColorBucket primaryColorBucketG = new();
            ColorBucket primaryColorBucketB = new();
            int rCount = 0;
            int gCount = 0;
            int bCount = 0;
            int takeCount = Math.Min(300, primaryColors.Count);
            int maxBucketSize = 10;
            for (int i = 0; i < takeCount; i++)
            {
                // 转换回RGB值
                int r = ((primaryColors[i].Key >> (2 * colorBits)) & colorMask) << (8 - colorBits);
                int g = ((primaryColors[i].Key >> colorBits) & colorMask) << (8 - colorBits);
                int b = (primaryColors[i].Key & colorMask) << (8 - colorBits);

                if (r > g && r > b)
                {
                    if (rCount < maxBucketSize)
                    {
                        primaryColorBucketR.RedSum += r * primaryColors[i].Value;
                        primaryColorBucketR.GreenSum += g * primaryColors[i].Value;
                        primaryColorBucketR.BlueSum += b * primaryColors[i].Value;
                        primaryColorBucketR.Count += primaryColors[i].Value;
                    }
                    rCount++;
                }
                else if (g > r && g > b)
                {
                    if (gCount < maxBucketSize)
                    {
                        primaryColorBucketG.RedSum += r * primaryColors[i].Value;
                        primaryColorBucketG.GreenSum += g * primaryColors[i].Value;
                        primaryColorBucketG.BlueSum += b * primaryColors[i].Value;
                        primaryColorBucketG.Count += primaryColors[i].Value;
                    }
                    gCount++;
                }
                else
                {
                    if (bCount < maxBucketSize)
                    {
                        primaryColorBucketB.RedSum += r * primaryColors[i].Value;
                        primaryColorBucketB.GreenSum += g * primaryColors[i].Value;
                        primaryColorBucketB.BlueSum += b * primaryColors[i].Value;
                        primaryColorBucketB.Count += primaryColors[i].Value;
                    }
                    bCount++;
                }
            }
            Color color = new();
            if (rCount > gCount && rCount > bCount)
            {
                color = primaryColorBucketR.GetAverageColor();
            }
            else if (gCount > rCount && gCount > bCount)
            {
                color = primaryColorBucketG.GetAverageColor();
            }
            else
            {
                color = primaryColorBucketB.GetAverageColor();
            }
            return color;
        }
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

    public static bool IsHarshColor(Color color)
    {
        // 转换为HSL色彩空间
        var hslColor = color.ToHsl();
        double h = hslColor.H;
        double s = hslColor.S * 100;
        double l = hslColor.L * 100;

        // 基础判断阈值
        double saturationThreshold;
        double lightnessUpperThreshold;
        double lightnessLowerThreshold;

        // 根据色相确定基础阈值
        if (IsInRange(h, 0, 30) || IsInRange(h, 300, 360)) // 红色系
        {
            saturationThreshold = 55;
            lightnessUpperThreshold = 85;
            lightnessLowerThreshold = 30;
        }
        else if (IsInRange(h, 30, 80)) // 黄色系
        {
            saturationThreshold = 40;
            lightnessUpperThreshold = 88;
            lightnessLowerThreshold = 30;
        }
        else if (IsInRange(h, 110, 140)) // 绿色系
        {
            saturationThreshold = 45;
            lightnessUpperThreshold = 82;
            lightnessLowerThreshold = 30;
        }
        else if (IsInRange(h, 160, 170)) // 蓝绿色系
        {
            saturationThreshold = 42;
            lightnessUpperThreshold = 80;
            lightnessLowerThreshold = 20;
        }
        else if (IsInRange(h, 180, 190)) // 青色系
        {
            saturationThreshold = 40;
            lightnessUpperThreshold = 85;
            lightnessLowerThreshold = 20;
        }
        else if (IsInRange(h, 215, 290)) // 蓝紫色系
        {
            saturationThreshold = 45;
            lightnessUpperThreshold = 80;
            lightnessLowerThreshold = 15;
        }
        else // 其他色相
        {
            // 这里是不是不用判断了，直接return false
            saturationThreshold = 70;
            lightnessUpperThreshold = 80;
            lightnessLowerThreshold = 10;
        }

        // 根据亮度调整饱和度阈值
        if (l < 30)
        {
            // 低亮度时降低饱和度容忍度
            saturationThreshold -= 15;
        }
        else if (l > 70)
        {
            // 高亮度时降低饱和度容忍度
            saturationThreshold -= 10;
        }

        // 判断条件
        bool isSaturationTooHigh = s > saturationThreshold;
        bool isLightnessTooHigh = l > lightnessUpperThreshold;
        bool isLightnessTooLow = l < lightnessLowerThreshold;

        // 特殊组合判断
        //bool isHighSaturationWithExtremeL =
        //    s > (saturationThreshold - 10) && (l > lightnessUpperThreshold - 5 || l < lightnessLowerThreshold + 5);

        return (
                isSaturationTooHigh /*|| isHighSaturationWithExtremeL*/
            ) && (isLightnessTooHigh || isLightnessTooLow);
    }

    public static Color GenerateLessHarshColor(Color baseColor)
    {
        var hslColor = baseColor.ToHsl();
        double h = hslColor.H;
        double s = hslColor.S * 100;
        double l = hslColor.L * 100;

        // 如果不刺眼就直接返回
        //if (!ColorChecker.IsEyeStraining(h, s, l))
        //{
        //    return color;
        //}

        // 首先处理特殊色相
        AdjustSpecialHues(ref h, ref s, ref l);

        // 然后根据亮度区间进行额外调整
        double l2 = hslColor.L * 100;
        if (l2 < 30)
        {
            // 低亮度：提高亮度，额外降低饱和度
            l = Math.Min(l2 + 20, 45);
            s = Math.Min(s - 10, 60); // 额外降低饱和度
        }
        else if (l2 < 70)
        {
            // 中等亮度：适当调整
            l = Math.Max(l2, 45);
            s = Math.Min(s, 65);
        }
        else
        {
            // 高亮度：降低亮度和饱和度
            l = Math.Min(l2, 85);
            s = Math.Min(s - 5, 60); // 额外降低饱和度
        }

        // 最后检查是否仍然刺眼
        //while (ColorChecker.IsEyeStraining(color.H, color.S, color.L) && color.S > 30)
        //{
        //    color.S -= 5;
        //}
        s /= 100;
        l /= 100;
        return (
            new HslColor()
            {
                H = h,
                S = s,
                L = l,
                A = 1,
            }
        ).ToColor();
    }

    private static bool IsInRange(double value, double start, double end)
    {
        value = value % 360;
        return value >= start && value <= end;
    }

    private static void AdjustSpecialHues(ref double H, ref double S, ref double L)
    {
        // 基础饱和度限制
        double baseSaturationLimit;
        if (L < 30)
        {
            baseSaturationLimit = 45; // 低亮度时更严格的饱和度限制
        }
        else if (L > 70)
        {
            baseSaturationLimit = 50; // 高亮度时的饱和度限制
        }
        else
        {
            baseSaturationLimit = 55; // 中等亮度时的饱和度限制
        }

        // 处理特殊色相
        if (IsInRange(H, 0, 30) || IsInRange(H, 300, 360))
        {
            // 红色系
            S = Math.Min(S, baseSaturationLimit - 5);
            if (L < 30)
            {
                L = Math.Min(Math.Max(L + 15, 35), 45);
            }
        }
        else if (IsInRange(H, 30, 80))
        {
            // 荧光黄
            S = Math.Min(S, baseSaturationLimit - 10);
            if (L < 30)
            {
                L = Math.Min(Math.Max(L + 20, 40), 50);
            }
            else if (L > 70)
            {
                L = Math.Min(L, 85);
            }
        }
        else if (IsInRange(H, 110, 140))
        {
            // 荧光绿
            S = Math.Min(S, baseSaturationLimit - 15);
            if (L < 30)
            {
                L = Math.Min(Math.Max(L + 15, 35), 45);
            }
        }
        else if (IsInRange(H, 160, 170))
        {
            // 荧光蓝绿
            S = Math.Min(S, baseSaturationLimit - 18);
            if (L < 30)
            {
                L = Math.Min(Math.Max(L + 15, 35), 45);
            }
        }
        else if (IsInRange(H, 180, 190))
        {
            // 荧光青
            S = Math.Min(S, baseSaturationLimit - 20);
            if (L < 30)
            {
                L = Math.Min(Math.Max(L + 15, 35), 45);
            }
        }
        else if (IsInRange(H, 210, 290))
        {
            // 蓝紫色系
            S = Math.Min(S, baseSaturationLimit - 5);
            if (L < 30)
            {
                L = Math.Min(Math.Max(L + 15, 35), 45);
            }
        }
    }
}
