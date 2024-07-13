using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.ViewModels;
using Microsoft.UI.Xaml.Data;

namespace BKGalMgr.Converters;

public class GroupItemSwitchConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value.ToString() != GlobalInfo.GroupItemCase_Add
            ? GlobalInfo.GroupItemCase_Group
            : GlobalInfo.GroupItemCase_Add;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
