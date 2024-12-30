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

namespace BKGalMgr.Helpers;

public class ColorHelper
{
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

    public static unsafe Color GetImagePrimaryColor(string imagePath)
    {
        using (var bitmap = new Bitmap(imagePath))
        {
            // 使用较小的量化级别来减少颜色数量
            const int colorBits = 5; // 32个级别
            const int colorMask = (1 << colorBits) - 1;

            var colorHistogram = new ConcurrentDictionary<int, int>();

            // 将图片缩放到较小的尺寸以提高性能
            int targetWidth = Math.Min(bitmap.Width, 200);
            int targetHeight = (int)((float)bitmap.Height / bitmap.Width * targetWidth);

            using (var scaledBitmap = new Bitmap(bitmap, targetWidth, targetHeight))
            {
                var rect = new Rectangle(0, 0, scaledBitmap.Width, scaledBitmap.Height);
                var bitmapData = scaledBitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    var ptr = (byte*)bitmapData.Scan0;
                    var stride = bitmapData.Stride;
                    var width = bitmapData.Width;
                    var height = bitmapData.Height;

                    // 使用SIMD和并行处理
                    Parallel.For(
                        0,
                        height,
                        y =>
                        {
                            var row = ptr + (y * stride);
                            for (var x = 0; x < width; x++)
                            {
                                var idx = x * 4;
                                var b = row[idx];
                                var g = row[idx + 1];
                                var r = row[idx + 2];
                                var a = row[idx + 3];

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

            ColorBucket primaryColorBucket = new();
            int takeCount = Math.Min(10, primaryColors.Count);
            for (int i = 0; i < takeCount; i++)
            {
                // 转换回RGB值
                int r = ((primaryColors[i].Key >> (2 * colorBits)) & colorMask) << (8 - colorBits);
                int g = ((primaryColors[i].Key >> colorBits) & colorMask) << (8 - colorBits);
                int b = (primaryColors[i].Key & colorMask) << (8 - colorBits);

                primaryColorBucket.RedSum += r * primaryColors[i].Value;
                primaryColorBucket.GreenSum += g * primaryColors[i].Value;
                primaryColorBucket.BlueSum += b * primaryColors[i].Value;
                primaryColorBucket.Count += primaryColors[i].Value;
            }

            // 平均颜色，且调暗
            var color = primaryColorBucket.GetAverageColor();
            while (!IsColorDarkWithHSL(color))
                color = GenerateLighterOrDarkerColor(color, false);
            return color;
        }
    }

    public static bool IsColorDark(Color color)
    {
        // 将RGB转换为线性RGB (Linear RGB)
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        // 计算亮度 (Luminance)
        // 使用sRGB到亮度的转换公式: L = 0.2126 * R + 0.7152 * G + 0.0722 * B
        double luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;

        // 判断亮度是否小于阈值
        // 这个值可以根据你的需求调整
        return luminance < 0.5;
    }

    public static (double h, double s, double l) ColorToHSL(Color color)
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

    public static bool IsColorDarkWithHSL(Color color)
    {
        (double h, double s, double l) = ColorToHSL(color);

        // 将L转换为百分比形式
        l *= 100;

        // 判断亮度是否小于阈值
        // 这个值可以根据你的需求调整
        return l < 30.0;
    }

    public static Windows.UI.Color ConvertToWindowsColor(Color color)
    {
        return Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public static Color GenerateLighterOrDarkerColor(Color baseColor, bool isLighter = true)
    {
        // 将RGB转换为HSL
        double r = baseColor.R / 255.0;
        double g = baseColor.G / 255.0;
        double b = baseColor.B / 255.0;

        (double h, double s, double l) = ColorToHSL(baseColor);

        // 调整亮度
        // 这个值可以根据你的需求调整
        l = isLighter
            ? Math.Min(l + 0.2, 1.0)
            : // 变亮
            Math.Max(l - 0.2, 0.0); // 变暗

        // 转换回RGB
        if (s == 0)
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

        return Color.FromArgb(255, (int)(r * 255), (int)(g * 255), (int)(b * 255));
    }

    private static double HueToRgb(double p, double q, double t)
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
