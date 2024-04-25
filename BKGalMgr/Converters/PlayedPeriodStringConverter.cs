using BKGalMgr.ViewModels;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.Converters;

public class PlayedPeriodStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        PlayedPeriod pp = value as PlayedPeriod;
        if (pp == null) return null;

        return (pp.endTime - pp.benginTime).ToString(@"hh\:mm\:ss") + $" {pp.benginTime} ~ {pp.endTime}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
