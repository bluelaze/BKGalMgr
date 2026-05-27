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
        // 这里放回null时，用x:Bind会报空引用异常
        // 但如果返回default(DateTimeOffset)，就会默认设置个值
        // 所以如果用这个Converter，只能用Binding
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset dto)
            return dto.DateTime;

        return null;
    }
}
