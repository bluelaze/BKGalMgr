using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.ViewModels;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Xaml.Data;
using Windows.Devices.Lights;

namespace BKGalMgr.Converters;

public class PlayedPeriodToXAxesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var pp = value as IEnumerable<PlayedPeriodInfo>;
        if (pp == null)
            return null;

        return new List<ICartesianAxis>()
        {
            new Axis()
            {
                TextSize = 14,
                Labels = pp.Select(t => t.Coordinate.SecondaryValue.AsDate().ToString()).Reverse().ToList(),
            },
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return DependencyProperty.UnsetValue;
    }
}
