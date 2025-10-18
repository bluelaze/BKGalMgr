using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml.Data;
using SkiaSharp;

namespace BKGalMgr.Converters;

public class GameReviewGroupInfoToYAxesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
            return null;
        return new List<ICartesianAxis>()
        {
            new Axis
            {
                TextSize = 14,
                SeparatorsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(100)),
                Labeler = value =>
                {
                    var t = value.AsTimeSpan();
                    return string.Format("{0:00}:{1:mm}:{1:ss}", (int)t.TotalHours, t);
                },
            },
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return DependencyProperty.UnsetValue;
    }
}
