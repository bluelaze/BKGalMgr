using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.Extensions;

public static class BitmapExtensions
{
    public static System.Windows.Media.Imaging.BitmapImage ToSystemBitmapImage(this Bitmap bitmap)
    {
        // https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        using (var memory = new MemoryStream())
        {
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }

    public static Microsoft.UI.Xaml.Media.Imaging.BitmapImage ToWinUIBitmapImage(this Bitmap bitmap)
    {
        // https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        using (var memory = new MemoryStream())
        {
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
            bitmapImage.SetSource(memory.AsRandomAccessStream());

            return bitmapImage;
        }
    }

    /// <summary>
    /// 检测并去除图片对称的黑边，使其恢复 16:9 比例
    /// </summary>
    /// <param name="original">原始 Bitmap 图像</param>
    /// <returns>裁剪后的新 Bitmap 图像，不满足条件则返回null</returns>
    public static Bitmap CropBlackBordersTo16x9(this Bitmap original)
    {
        if (original == null)
            return null;

        int width = original.Width;
        int height = original.Height;

        // 1. 判断宽高比是否不是 16:9
        bool isNot16x9TB = (width * 9) < (height * 16); // 上下黑边
        bool isNot16x9LR = (width * 9) > (height * 16); // 左右黑边

        // 2. 判断第一个像素 (0,0) 是否为黑色
        Color firstPixel = original.GetPixel(0, 0);
        // 优化：如果是 JPEG 等有损压缩格式，黑边像素可能不是绝对的 RGB(0,0,0)
        // 这里设置一个阈值（例如 RGB 都小于 10 则认为是黑色），比严格的 == 0 更健壮
        bool isFirstPixelBlack = firstPixel.R < 10 && firstPixel.G < 10 && firstPixel.B < 10;

        // 满足条件：比例不是16:9 且 第一个像素是黑色
        if ((isNot16x9TB || isNot16x9LR) && isFirstPixelBlack)
        {
            // 上下黑边
            int targetHeight = isNot16x9TB ? (int)Math.Round((double)width * 9 / 16) : height;
            int blackBorderHeight = isNot16x9TB ? (height - targetHeight) / 2 : 0;
            // 左右黑边
            int targetWidth = isNot16x9LR ? (int)Math.Round((double)height * 16 / 9) : width;
            int blackBorderWidth = isNot16x9TB ? (width - targetWidth) / 2 : 0;

            // 定义需要截取的目标区域 (X, Y, Width, Height)
            Rectangle cropArea = new Rectangle(blackBorderWidth, blackBorderHeight, targetWidth, targetHeight);

            // 执行裁剪并返回新的 Bitmap
            Bitmap croppedBmp = original.Clone(cropArea, original.PixelFormat);
            return croppedBmp;
        }

        return null;
    }
}
