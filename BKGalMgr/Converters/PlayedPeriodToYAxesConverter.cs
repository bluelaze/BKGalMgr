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
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml.Data;
using SkiaSharp;
using Windows.Devices.Lights;

namespace BKGalMgr.Converters;

public class PlayedPeriodToYAxesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return new List<ICartesianAxis>()
        {
            new Axis
            {
                TextSize = 14,
                SeparatorsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(100)),
                Labeler = value => value.AsTimeSpan().ToString(@"hh\:mm\:ss")
            }
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
