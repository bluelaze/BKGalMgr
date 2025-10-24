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
}
