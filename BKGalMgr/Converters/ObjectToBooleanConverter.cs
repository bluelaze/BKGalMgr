using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;

namespace BKGalMgr.Converters;

// reference IsEqualStateTrigger
public class ObjectToBooleanConverter : DependencyObject, IValueConverter
{
    /// <summary>
    /// Identifies the <see cref="TrueValue"/> property.
    /// </summary>
    public static readonly DependencyProperty TrueValueProperty = DependencyProperty.Register(
        nameof(TrueValue),
        typeof(object),
        typeof(ObjectToBooleanConverter),
        new PropertyMetadata(null)
    );

    /// <summary>
    /// Identifies the <see cref="FalseValue"/> property.
    /// </summary>
    public static readonly DependencyProperty FalseValueProperty = DependencyProperty.Register(
        nameof(FalseValue),
        typeof(object),
        typeof(ObjectToBooleanConverter),
        new PropertyMetadata(null)
    );

    /// <summary>
    /// Gets or sets the value to be returned when the boolean is true
    /// </summary>
    public object TrueValue
    {
        get { return GetValue(TrueValueProperty); }
        set { SetValue(TrueValueProperty, value); }
    }

    /// <summary>
    /// Gets or sets the value to be returned when the boolean is false
    /// </summary>
    public object FalseValue
    {
        get { return GetValue(FalseValueProperty); }
        set { SetValue(FalseValueProperty, value); }
    }

    public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
        nameof(To),
        typeof(object),
        typeof(ObjectToBooleanConverter),
        new PropertyMetadata(null)
    );

    public object To
    {
        get { return GetValue(ToProperty); }
        set { SetValue(ToProperty, value); }
    }

    public ObjectToBooleanConverter()
    {
        TrueValue = true;
        FalseValue = false;
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // Negate if needed
        bool invert = TryParseBool(parameter);

        if (object.Equals(value, To))
        {
            return invert ? FalseValue : TrueValue;
        }

        // If they are the same type but fail with Equals check, don't bother with conversion.
        if (value?.GetType() != To?.GetType())
        {
            // Try the conversion in both ways:
            return (ConvertTypeEquals(value, To) || ConvertTypeEquals(To, value)) ^ invert ? TrueValue : FalseValue;
        }
        return invert ? TrueValue : FalseValue;
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
