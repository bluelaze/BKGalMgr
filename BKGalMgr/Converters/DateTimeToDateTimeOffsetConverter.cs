using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;

namespace BKGalMgr.Converters;

public class DateTimeToDateTimeOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dt && dt.Ticks > 0)
            return new DateTimeOffset(dt);
        // 这里放回null时，如果不是绑定的不是DateTimeOffset?，用x:Bind会报空引用异常
        // 用Binding就不报异常
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset dto)
            return dto.DateTime;

        return new DateTime();
    }
}
