using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Data;

namespace BKGalMgr.Converters;

public class EnumNameConveretr : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // x:Bind时，targetType是正确的type，parameter则会是null，没能通过EnumTypeExtension传进来type；
        // Binding时则相反，targetType是个system object，而parameter才是正确的type。
        // 查了下是个bug，x:Bind时，ConverterParameter就是不能通过MarkupExtension传参
        // https://github.com/microsoft/microsoft-ui-xaml/issues/8397
        if (Enum.TryParse(parameter as Type ?? targetType, value as string, out object result))
        {
            return result;
        }
        return DependencyProperty.UnsetValue;
    }
}
