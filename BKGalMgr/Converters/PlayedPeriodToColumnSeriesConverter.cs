using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.ViewModels;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Xaml.Data;
using Windows.Devices.Lights;

namespace BKGalMgr.Converters;

public class PlayedPeriodToColumnSeriesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var pp = value as IEnumerable<PlayedPeriodInfo>;
        if (pp == null)
            return null;

        return new ISeries[]
        {
            new ColumnSeries<double>() { Values = pp.Select(t => t.Coordinate.PrimaryValue).Reverse().ToList() },
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
