using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;

namespace BKGalMgr.Converters;

public class UriSafeConverter : IValueConverter
{
    /// <summary>
    /// 当默认的 URI 格式不正确时，回退到的安全 URI（可选）
    /// </summary>
    public string FallbackUri { get; set; } = "about:blank";

    // 从 String 转换到 Uri (绑定 Data -> UI)
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
            return GetFallbackUri();

        string input = value.ToString().Trim();

        if (string.IsNullOrWhiteSpace(input))
            return GetFallbackUri();

        // 检查是否需要对参数进行编码 (如果在 XAML 中传入了 parameter="encode")
        if (parameter?.ToString() == "encode")
        {
            // 注意：EscapeDataString 会转义 '/' 和 ':'，适合转义完整的查询参数值
            // 如果是处理整条网址，请根据需求调整
            input = Uri.EscapeDataString(input);
        }

        // 尝试安全解析 URI
        if (Uri.TryCreate(input, UriKind.Absolute, out Uri resultUri))
        {
            return resultUri;
        }

        // 如果不是绝对路径，尝试作为相对路径解析
        if (Uri.TryCreate(input, UriKind.Relative, out resultUri))
        {
            return resultUri;
        }

        // 都失败了，返回安全回退地址
        return GetFallbackUri();
    }

    // 从 Uri 转换回 String (双向绑定 UI -> Data，通常 WebView2 等控件不需要)
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Uri uri)
        {
            return uri.ToString();
        }
        return string.Empty;
    }

    private Uri GetFallbackUri()
    {
        return Uri.TryCreate(FallbackUri, UriKind.Absolute, out Uri fallback) ? fallback : new Uri("about:blank");
    }
}
