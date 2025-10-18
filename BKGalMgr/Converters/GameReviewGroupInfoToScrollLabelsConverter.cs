using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace BKGalMgr.Converters;

public class GameReviewGroupInfoToScrollLabelsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var pp = value as IEnumerable<GameReviewGroupInfo>;
        if (pp == null)
            return new List<AnnotatedScrollBarLabel>();

        var itemHeight = -290;
        return pp.Select(g => new AnnotatedScrollBarLabel(g.Label.ToString("yyyy/MM"), itemHeight += 290))
            .Reverse()
            .DistinctBy(g => g.Content.ToString())
            .Reverse()
            .ToList();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return DependencyProperty.UnsetValue;
    }
}
