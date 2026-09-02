using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BKGalMgr.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BKGalMgr.Helpers;

public static class ImageLoadHelper
{
    // 全局内存缓存，设置最多保留 1000 张小图
    private static readonly MemoryCache Cache = new(new MemoryCacheOptions { SizeLimit = 256 });

    // 滑动过期时间：10分钟内如果没有再次被渲染展示，自动失效释放
    private static readonly TimeSpan SlidingExpirationTime = TimeSpan.FromMinutes(10);

    // 绝对过期时间：无论是否在使用，最多只在内存中留存 1 小时（到期后强制重新读取磁盘）
    private static readonly TimeSpan AbsoluteExpirationTime = TimeSpan.FromHours(1);

    public static readonly DependencyProperty UriSourceProperty = DependencyProperty.RegisterAttached(
        "UriSource",
        typeof(string),
        typeof(ImageLoadHelper),
        new PropertyMetadata(null, OnPropertyChanged)
    );

    public static string GetUriSource(Image element)
    {
        return (string)element.GetValue(UriSourceProperty);
    }

    public static void SetUriSource(Image element, string value)
    {
        element.SetValue(UriSourceProperty, value);
    }

    public static readonly DependencyProperty DecodePixelWidthProperty = DependencyProperty.RegisterAttached(
        "DecodePixelWidth",
        typeof(int),
        typeof(ImageLoadHelper),
        new PropertyMetadata(0)
    );

    public static int GetDecodePixelWidth(Image element)
    {
        return (int)element.GetValue(DecodePixelWidthProperty);
    }

    public static void SetDecodePixelWidth(Image element, int value)
    {
        element.SetValue(DecodePixelWidthProperty, value);
    }

    public static readonly DependencyProperty DecodePixelHeightProperty = DependencyProperty.RegisterAttached(
        "DecodePixelHeight",
        typeof(int),
        typeof(ImageLoadHelper),
        new PropertyMetadata(0)
    );

    public static int GetDecodePixelHeight(Image element)
    {
        return (int)element.GetValue(DecodePixelHeightProperty);
    }

    public static void SetDecodePixelHeight(Image element, int value)
    {
        element.SetValue(DecodePixelHeightProperty, value);
    }

    public static readonly DependencyProperty CreateOptionsProperty = DependencyProperty.RegisterAttached(
        "CreateOptions",
        typeof(BitmapCreateOptions),
        typeof(ImageLoadHelper),
        new PropertyMetadata(BitmapCreateOptions.None)
    );

    public static BitmapCreateOptions GetCreateOptions(Image element)
    {
        return (BitmapCreateOptions)element.GetValue(CreateOptionsProperty);
    }

    public static void SetCreateOptions(Image element, BitmapCreateOptions value)
    {
        element.SetValue(CreateOptionsProperty, value);
    }

    private static readonly DependencyProperty MosaicTypeProperty = DependencyProperty.RegisterAttached(
        "MosaicType",
        typeof(MosaicType),
        typeof(ImageLoadHelper),
        new PropertyMetadata(MosaicType.None, OnPropertyChanged)
    );

    public static MosaicType GetMosaicType(Image element)
    {
        return (MosaicType)element.GetValue(MosaicTypeProperty);
    }

    public static void SetMosaicType(Image element, MosaicType value)
    {
        element.SetValue(MosaicTypeProperty, value);
    }

    private static readonly DependencyProperty LoadingCtsProperty = DependencyProperty.RegisterAttached(
        "LoadingCts",
        typeof(CancellationTokenSource),
        typeof(ImageLoadHelper),
        new PropertyMetadata(null)
    );

    private static CancellationTokenSource GetLoadingCts(Image element)
    {
        return (CancellationTokenSource)element.GetValue(LoadingCtsProperty);
    }

    private static void SetLoadingCts(Image element, CancellationTokenSource value)
    {
        element.SetValue(LoadingCtsProperty, value);
    }

    private static async void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Image image)
            return;

        // 1. 取消上一次在当前 Image 控件上执行的旧加载任务（针对 GridView 容器复用）
        GetLoadingCts(image)?.Cancel();
        SetLoadingCts(image, null);

        string filePath = GetUriSource(image);
        int decodeWidth = GetDecodePixelWidth(image);
        int decodeHeight = GetDecodePixelHeight(image);
        MosaicType mosaicType = GetMosaicType(image);
        BitmapCreateOptions bitmapCreateOptions = GetCreateOptions(image);

        // 非标准路径直接清空旧图，避免抛出异常
        if (!Uri.TryCreate(filePath, UriKind.Absolute, out _))
        {
            image.Source = null;
            return;
        }

        // 2. 查询缓存（若已超时失效，TryGetValue 会直接返回 false）
        string cacheKey = $"{filePath}_{decodeWidth}_{decodeHeight}_{mosaicType}";
        if (
            bitmapCreateOptions != BitmapCreateOptions.IgnoreImageCache
            && Cache.TryGetValue(cacheKey, out WeakReference<BitmapImage> weakRef)
            && weakRef.TryGetTarget(out BitmapImage cachedBitmap)
        )
        {
            image.Source = cachedBitmap;
            return;
        }

        image.Source = null; // 未命中缓存，清空旧图

        // 3. 开启异步加载
        var newCts = new CancellationTokenSource();
        SetLoadingCts(image, newCts);
        var token = newCts.Token;
        try
        {
            BitmapImage loadedBitmap = await LoadAndDecodeAsync(filePath, decodeWidth, decodeHeight, mosaicType, token);

            if (!token.IsCancellationRequested && loadedBitmap != null)
            {
                // 4. 配置缓存的自动失效时间
                var options = new MemoryCacheEntryOptions()
                    .SetSize(1) // 每张图计入 1 个单位大小
                    .SetSlidingExpiration(SlidingExpirationTime) // 滑动超时：只要不看它，10 分钟后自动销毁
                    .SetAbsoluteExpiration(AbsoluteExpirationTime); // 绝对超时：最多存 1 小时

                if (bitmapCreateOptions != BitmapCreateOptions.IgnoreImageCache)
                    Cache.Set(cacheKey, new WeakReference<BitmapImage>(loadedBitmap), options);

                image.Source = loadedBitmap;
            }
        }
        catch
        {
            if (!token.IsCancellationRequested)
            {
                image.Source = null;
            }
        }

        if (GetLoadingCts(image) == newCts)
        {
            SetLoadingCts(image, null);
        }
        newCts.Dispose();
    }

    private static async Task<BitmapImage> LoadAndDecodeAsync(
        string filePath,
        int targetWidth,
        int targetHeight,
        MosaicType mosaicType,
        CancellationToken token
    )
    {
        // 如果是网络图片，直接使用 BitmapImage 的 UriSource 加载
        if (filePath.StartsWith("http"))
        {
            return new BitmapImage(new Uri(filePath))
            {
                DecodePixelWidth = (int)(targetWidth * 1.5),
                DecodePixelHeight = (int)(targetHeight * 1.5),
                DecodePixelType = DecodePixelType.Logical,
            };
        }

        // 只支持本地文件
        if (!File.Exists(filePath))
            return null;

        // 改用标准 .NET FileStream，比 StorageFile 更稳定且不易抛出 Native WinRT 异常
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        using var randomAccessStream = fileStream.AsRandomAccessStream();

        if (token.IsCancellationRequested)
            return null;

        // 设置 BitmapImage 的解码像素大小，放大 1.5 倍以提高显示质量
        // 笔电一般屏幕 DPI 在 150% 左右，就默认1.5倍，避免每次都去获取屏幕DPI
        var bitmap = new BitmapImage
        {
            DecodePixelWidth = (int)(targetWidth * 1.5),
            DecodePixelHeight = (int)(targetHeight * 1.5),
            DecodePixelType = DecodePixelType.Logical,
        };

        if (mosaicType == MosaicType.None)
        {
            await bitmap.SetSourceAsync(randomAccessStream);
        }
        else
        {
            using var mosaicStream = await ImageMisc.CreateMosaicAsync(randomAccessStream, mosaicType);
            await bitmap.SetSourceAsync(mosaicStream);
        }

        if (token.IsCancellationRequested)
            return null;

        return bitmap;
    }
}
