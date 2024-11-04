using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.ViewModels;
using Microsoft.UI.Xaml.Data;

namespace BKGalMgr.Converters;

public class PlayedPeriodStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        PlayedPeriodInfo pp = value as PlayedPeriodInfo;
        if (pp == null)
            return null;

        return pp.Period.ToString(@"hh\:mm\:ss") + $" {pp.BenginTime} ~ {pp.EndTime}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
