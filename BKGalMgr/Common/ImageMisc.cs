using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.Enums;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace BKGalMgr.Common;

public static class ImageMisc
{
    public static async Task<InMemoryRandomAccessStream> CreateMosaicAsync(
        IRandomAccessStream imageStream,
        MosaicType mosaicType = MosaicType.Small,
        int mosaicSize = 15
    )
    {
        // 1. 解码原始图片流
        var decoder = await BitmapDecoder.CreateAsync(imageStream);
        uint width = decoder.PixelWidth;
        uint height = decoder.PixelHeight;

        // 2. 获取像素数据 (格式必须统一指定为 Bgra8 方便处理)
        var pixelData = await decoder.GetPixelDataAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            new BitmapTransform(),
            ExifOrientationMode.RespectExifOrientation,
            ColorManagementMode.ColorManageToSRgb
        );

        byte[] pixels = pixelData.DetachPixelData();

        // 3. 计算马赛克区域
        int startX = 0;
        int startY = 0;
        int endX = 0;
        int endY = 0;
        switch (mosaicType)
        {
            case MosaicType.Small:
                startY = (int)height / 3;
                endX = (int)width;
                endY = startY * 2;
                break;
            case MosaicType.Medium:
                startY = (int)height / 4;
                endX = (int)width;
                endY = startY * 3;
                break;
            case MosaicType.Big:
                startY = (int)height / 6;
                endX = (int)width;
                endY = startY * 5;
                break;
        }
        if (startY != 0)
        {
            // 需要处理马赛克的区域一般偏下
            int offsetY = (int)height / 8;
            startY += offsetY;
            endY += offsetY;
        }

        // 4. 原生马赛克算法
        for (int y = startY; y < endY; y += mosaicSize)
        {
            for (int x = startX; x < endX; x += mosaicSize)
            {
                // 找到当前区块左上角第一个像素的索引位置 (Y * 宽 + X) * 4字节
                int sampleIndex = (y * (int)width + x) * 4;
                if (sampleIndex + 3 >= pixels.Length)
                    continue;

                // 提取该像素的 B, G, R, A 颜色值
                byte b = pixels[sampleIndex];
                byte g = pixels[sampleIndex + 1];
                byte r = pixels[sampleIndex + 2];
                byte a = pixels[sampleIndex + 3];

                // 涂满整个区块
                for (int blockY = 0; blockY < mosaicSize && y + blockY < endY; blockY++)
                {
                    for (int blockX = 0; blockX < mosaicSize && x + blockX < endX; blockX++)
                    {
                        int targetIndex = ((y + blockY) * (int)width + (x + blockX)) * 4;
                        if (targetIndex + 3 < pixels.Length)
                        {
                            pixels[targetIndex] = b;
                            pixels[targetIndex + 1] = g;
                            pixels[targetIndex + 2] = r;
                            pixels[targetIndex + 3] = a;
                        }
                    }
                }
            }
        }

        // 5. 将处理后的像素数组重新编码为内存中的新图片流
        var outStream = new InMemoryRandomAccessStream();
        // 这里以 PNG 格式重新编码，确保无损
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outStream);

        encoder.SetPixelData(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            width,
            height,
            decoder.DpiX,
            decoder.DpiY,
            pixels
        );

        await encoder.FlushAsync();
        outStream.Seek(0); // 将流指针归零，准备给 BitmapImage 读取

        return outStream;
    }
}
