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

namespace BKGalMgr.Converters;

public class GameReviewGroupInfoToXAxesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var pp = value as IEnumerable<GameReviewGroupInfo>;
        if (pp == null)
            return null;

        return new List<ICartesianAxis>()
        {
            new Axis()
            {
                TextSize = 14,
                MaxLimit = pp.Count(),
                MinLimit = pp.Count() - 30 < 0 ? 0 : pp.Count() - 30,
                Labels = pp.Select(t => t.Label.ToString("d", CultureInfo.CurrentUICulture)).Reverse().ToList()
            }
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
