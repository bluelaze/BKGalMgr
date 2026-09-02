using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Data;

namespace BKGalMgr.Converters;

public class EnumCompareConveretr : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value.Equals(ConvertToEnum(value.GetType(), parameter));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (TryParseBool(value))
        {
            return ConvertToEnum(targetType, parameter);
        }
        // TODO: 需要双向绑定，而枚举值一对多不好返回，先返回-1
        // 目前会导致打码时，效果生效，但是复选框没选中，后续如果有崩溃再考虑怎么处理
        return -1;
    }

    private static object ConvertToEnum(Type enumType, object value)
    {
        // value cannot be the same type of enum now
        return value switch
        {
            string str => Enum.TryParse(enumType, str, out var e) ? e : null,

            int or uint or byte or sbyte or long or ulong or short or ushort => Enum.ToObject(enumType, value),
            _ => null,
        };
    }

    private static bool TryParseBool(object parameter)
    {
        var parsed = false;
        if (parameter != null)
        {
            bool.TryParse(parameter.ToString(), out parsed);
        }

        return parsed;
    }
}
