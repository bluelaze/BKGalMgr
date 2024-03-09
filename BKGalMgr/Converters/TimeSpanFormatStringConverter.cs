using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.Converters;

public class TimeSpanFormatStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        TimeSpan? ts = (TimeSpan?)value;
        if (ts == null) return null;

        return ts?.ToString(parameter as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
