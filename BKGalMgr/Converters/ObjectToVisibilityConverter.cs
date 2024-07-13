using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;
using Newtonsoft.Json.Linq;

namespace BKGalMgr.Converters;

// reference IsEqualStateTrigger
public class ObjectToVisibilityConverter : DependencyObject, IValueConverter
{
    public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
        nameof(To),
        typeof(object),
        typeof(ObjectToVisibilityConverter),
        new PropertyMetadata(null)
    );

    public object To
    {
        get { return GetValue(ToProperty); }
        set { SetValue(ToProperty, value); }
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (object.Equals(value, To))
        {
            return true;
        }

        // If they are the same type but fail with Equals check, don't bother with conversion.
        if (value?.GetType() != To?.GetType())
        {
            // Try the conversion in both ways:
            return ConvertTypeEquals(value, To) || ConvertTypeEquals(To, value);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }

    private static bool ConvertTypeEquals(object value1, object value2)
    {
        // Let's see if we can convert:
        if (value2 is Enum)
        {
            value1 = ConvertToEnum(value2.GetType(), value1);
        }
        else
        {
            value1 = System.Convert.ChangeType(value1, value2.GetType(), CultureInfo.InvariantCulture);
        }

        return value2.Equals(value1);
    }

    private static object ConvertToEnum(Type enumType, object value)
    {
        // value cannot be the same type of enum now
        return value switch
        {
            string str => Enum.TryParse(enumType, str, out var e) ? e : null,

            int or uint or byte or sbyte or long or ulong or short or ushort => Enum.ToObject(enumType, value),
            _ => null
        };
    }
}
