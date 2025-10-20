using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Data;

namespace BKGalMgr.Converters;

public class StringToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string color)
        {
            return color.ToColor();
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Windows.UI.Color color)
        {
            return color.ToHex();
        }

        return DependencyProperty.UnsetValue;
    }
}
