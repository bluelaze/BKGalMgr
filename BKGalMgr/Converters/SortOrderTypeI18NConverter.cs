using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.Helpers;
using Microsoft.UI.Xaml.Data;

namespace BKGalMgr.Converters;

public class SortOrderTypeI18NConverterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return LanguageHelper.GetString($"Library_SortOrderType_{value.ToString()}/Text");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
