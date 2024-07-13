using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.ViewModels;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml.Data;

namespace BKGalMgr.Converters;

public class IEnumerableToMetadataItemsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var list = value as IEnumerable<string>;
        if (list == null)
            return null;

        ObservableCollection<MetadataItem> items = new();
        foreach (var item in list)
            items.Add(new MetadataItem() { Label = item });

        return items;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
