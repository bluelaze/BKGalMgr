using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;

namespace BKGalMgr.Converters;

public class TimeSpanFormatStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is TimeSpan ts)
        {
            return ts.Format(parameter as string);
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        var format = parameter as string;
        if (format == "hhmmss")
        {
            var arr = (value as string)?.Split(':');
            if (arr?.Length != 3)
                return DependencyProperty.UnsetValue;

            if (!int.TryParse(arr[0], out int hh))
                return DependencyProperty.UnsetValue;
            if (!int.TryParse(arr[1], out int mm))
                return DependencyProperty.UnsetValue;
            if (!int.TryParse(arr[2], out int ss))
                return DependencyProperty.UnsetValue;
            return new TimeSpan(hh, mm, ss);
        }
        else if (
            TimeSpan.TryParseExact(
                value as string,
                format,
                CultureInfo.InvariantCulture,
                TimeSpanStyles.None,
                out var time
            )
        )
        {
            return time;
        }
        return DependencyProperty.UnsetValue;
    }
}
