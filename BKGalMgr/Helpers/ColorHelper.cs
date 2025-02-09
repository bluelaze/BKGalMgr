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
using Microsoft.UI.Xaml.Controls;

namespace BKGalMgr.Helpers;

public class ColorHelper
{
    public static Windows.UI.Color ConvertToWindowsColor(Color color)
    {
        return Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    internal class ColorBucket
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
            int targetWidth = Math.Min(bitmap.Width, 360);
            int targetHeight = (int)((float)bitmap.Height / bitmap.Width * targetWidth);

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
            // 平均颜色，且调暗
            while (!IsDarkColor(color))
                color = GenerateLighterOrDarkerColor(color, false);
            return color;
        }
    }

    public static bool IsDarkColor(Color color, double luminanceThreshold = 30)
    {
        var hslColor = new HSLColor(color);
        return hslColor.L < luminanceThreshold;
    }

    public static Color GenerateLighterOrDarkerColor(
        Color baseColor,
        bool isLighter = true,
        double luminanceIncrement = 20
    )
    {
        var hslColor = new HSLColor(baseColor);

        // 调整亮度

        hslColor.L = isLighter
            ? Math.Min(hslColor.L + luminanceIncrement, 100)
            : Math.Max(hslColor.L - luminanceIncrement, 0.0);

        // 转换回RGB
        return hslColor.ToColor();
    }

    public class HSLColor
    {
        public double H { get; set; } // 色相 0-360
        public double S { get; set; } // 饱和度 0-100
        public double L { get; set; } // 亮度 0-100

        public HSLColor(double h, double s, double l)
        {
            H = h % 360;
            S = s;
            L = l;
        }

        public HSLColor(Color color)
        {
            (double h, double s, double l) = FromColor(color);
            H = h * 360;
            S = s * 100;
            L = l * 100;
        }

        private (double h, double s, double l) FromColor(Color color)
        {
            // 将RGB转换为HSL
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            double h,
                s,
                l;

            l = (max + min) / 2;

            if (max == min)
            {
                // 灰度颜色，没有色相
                h = 0;
                s = 0;
            }
            else
            {
                double d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

                if (max == r)
                    h = (g - b) / d + (g < b ? 6 : 0);
                else if (max == g)
                    h = (b - r) / d + 2;
                else
                    h = (r - g) / d + 4;

                h /= 6;
            }

            return (h, s, l);
        }

        public Color ToColor()
        {
            var h = H / 360;
            var s = S / 100;
            var l = L / 100;

            double r,
                g,
                b;

            if (Math.Abs(s) < 0.00001)
            {
                r = g = b = l;
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;

                r = HueToRgb(p, q, h + 1.0 / 3.0);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 1.0 / 3.0);
            }

            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        private double HueToRgb(double p, double q, double t)
        {
            if (t < 0)
                t += 1;
            if (t > 1)
                t -= 1;
            if (t < 1.0 / 6.0)
                return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0)
                return q;
            if (t < 2.0 / 3.0)
                return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }
    }

    public static bool IsHarshColor(Color color)
    {
        // 转换为HSL色彩空间
        var hslColor = new HSLColor(color);
        double h = hslColor.H;
        double s = hslColor.S;
        double l = hslColor.L;

        // 基础判断阈值
        double saturationThreshold;
        double lightnessUpperThreshold;
        double lightnessLowerThreshold;

        // 根据色相确定基础阈值
        if (IsInRange(h, 0, 30) || IsInRange(h, 300, 359)) // 红色系
        {
            saturationThreshold = 55;
            lightnessUpperThreshold = 85;
            lightnessLowerThreshold = 30;
        }
        else if (IsInRange(h, 30, 80)) // 黄色系
        {
            saturationThreshold = 50;
            lightnessUpperThreshold = 88;
            lightnessLowerThreshold = 25;
        }
        else if (IsInRange(h, 120, 140)) // 绿色系
        {
            saturationThreshold = 45;
            lightnessUpperThreshold = 82;
            lightnessLowerThreshold = 22;
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
        else if (IsInRange(h, 210, 290)) // 蓝紫色系
        {
            saturationThreshold = 45;
            lightnessUpperThreshold = 80;
            lightnessLowerThreshold = 20;
        }
        else // 其他色相
        {
            // 这里是不是不用判断了，直接return false
            saturationThreshold = 70;
            lightnessUpperThreshold = 80;
            lightnessLowerThreshold = 15;
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
        bool isHighSaturationWithExtremeL =
            s > (saturationThreshold - 10) && (l > lightnessUpperThreshold - 5 || l < lightnessLowerThreshold + 5);

        return (isSaturationTooHigh || isHighSaturationWithExtremeL) && (isLightnessTooHigh || isLightnessTooLow);
    }

    public static Color GenerateLessHarshColor(Color baseColor)
    {
        var hslColor = new HSLColor(baseColor);

        double h = hslColor.H;
        double s = hslColor.S;
        double l = hslColor.L;

        // 如果不刺眼就直接返回
        //if (!ColorChecker.IsEyeStraining(h, s, l))
        //{
        //    return color;
        //}

        // 首先处理特殊色相
        AdjustSpecialHues(hslColor);

        // 然后根据亮度区间进行额外调整
        if (l < 30)
        {
            // 低亮度：提高亮度，额外降低饱和度
            hslColor.L = Math.Min(l + 20, 45);
            hslColor.S = Math.Min(hslColor.S - 10, 60); // 额外降低饱和度
        }
        else if (l < 70)
        {
            // 中等亮度：适当调整
            hslColor.L = Math.Max(l, 45);
            hslColor.S = Math.Min(hslColor.S, 65);
        }
        else
        {
            // 高亮度：降低亮度和饱和度
            hslColor.L = Math.Min(l, 85);
            hslColor.S = Math.Min(hslColor.S - 5, 60); // 额外降低饱和度
        }

        // 最后检查是否仍然刺眼
        //while (ColorChecker.IsEyeStraining(color.H, color.S, color.L) && color.S > 30)
        //{
        //    color.S -= 5;
        //}

        return hslColor.ToColor();
    }

    private static bool IsInRange(double value, double start, double end)
    {
        value = value % 360;
        return value >= start && value <= end;
    }

    private static void AdjustSpecialHues(HSLColor color)
    {
        // 基础饱和度限制
        double baseSaturationLimit;
        if (color.L < 30)
        {
            baseSaturationLimit = 45; // 低亮度时更严格的饱和度限制
        }
        else if (color.L > 70)
        {
            baseSaturationLimit = 50; // 高亮度时的饱和度限制
        }
        else
        {
            baseSaturationLimit = 55; // 中等亮度时的饱和度限制
        }

        // 处理特殊色相
        if (IsInRange(color.H, 0, 30) || IsInRange(color.H, 300, 359))
        {
            // 红色系
            color.S = Math.Min(color.S, baseSaturationLimit - 5);
            if (color.L < 30)
            {
                color.L = Math.Min(Math.Max(color.L + 15, 35), 45);
            }
        }
        else if (IsInRange(color.H, 30, 80))
        {
            // 荧光黄
            color.S = Math.Min(color.S, baseSaturationLimit - 10);
            if (color.L < 30)
            {
                color.L = Math.Min(Math.Max(color.L + 20, 40), 50);
            }
            else if (color.L > 70)
            {
                color.L = Math.Min(color.L, 85);
            }
        }
        else if (IsInRange(color.H, 120, 140))
        {
            // 荧光绿
            color.S = Math.Min(color.S, baseSaturationLimit - 15);
            if (color.L < 30)
            {
                color.L = Math.Min(Math.Max(color.L + 15, 35), 45);
            }
        }
        else if (IsInRange(color.H, 160, 170))
        {
            // 荧光蓝绿
            color.S = Math.Min(color.S, baseSaturationLimit - 18);
            if (color.L < 30)
            {
                color.L = Math.Min(Math.Max(color.L + 15, 35), 45);
            }
        }
        else if (IsInRange(color.H, 180, 190))
        {
            // 荧光青
            color.S = Math.Min(color.S, baseSaturationLimit - 20);
            if (color.L < 30)
            {
                color.L = Math.Min(Math.Max(color.L + 15, 35), 45);
            }
        }
        else if (IsInRange(color.H, 210, 290))
        {
            // 蓝紫色系
            color.S = Math.Min(color.S, baseSaturationLimit - 5);
            if (color.L < 30)
            {
                color.L = Math.Min(Math.Max(color.L + 15, 35), 45);
            }
        }
    }
}
