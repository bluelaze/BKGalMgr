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
        if (value == null)
            return null;

        DateTime date = (DateTime)value;
        return new DateTimeOffset(date);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
            return null;

        DateTimeOffset dto = (DateTimeOffset)value;
        return dto.DateTime;
    }
}
