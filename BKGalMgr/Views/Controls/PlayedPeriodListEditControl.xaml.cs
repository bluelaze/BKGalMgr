using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BKGalMgr.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BKGalMgr.Views.Controls;

public sealed partial class PlayedPeriodListEditControl : UserControl
{
    public ObservableCollection<PlayedPeriodInfo> PlayedPeriods { get; set; }

    public PlayedPeriodListEditControl()
    {
        InitializeComponent();
    }

    private void add_Button_Click(object sender, RoutedEventArgs e)
    {
        PlayedPeriods.Insert(0, new PlayedPeriodInfo(DateTime.Now, DateTime.Now, new()));
    }

    private void delete_Button_Click(object sender, RoutedEventArgs e)
    {
        PlayedPeriods.Remove((sender as Button).DataContext as PlayedPeriodInfo);
    }

    private void datetime_TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textbox = sender as TextBox;
        if (
            DateTime.TryParseExact(
                textbox.Text,
                "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date
            )
        )
        {
            textbox.BorderBrush = App.Current.Resources["TextControlBorderBrush"] as SolidColorBrush;
        }
        else
        {
            textbox.BorderBrush = App.Current.Resources["SystemFillColorCriticalBrush"] as SolidColorBrush;
        }
    }

    private void timespan_TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textbox = sender as TextBox;
        var arr = (textbox.Text as string)?.Split(':');
        do
        {
            if (arr?.Length != 3)
                break;

            if (!int.TryParse(arr[0], out int hh))
                break;
            if (!int.TryParse(arr[1], out int mm))
                break;
            if (!int.TryParse(arr[2], out int ss))
                break;

            textbox.BorderBrush = App.Current.Resources["TextControlBorderBrush"] as SolidColorBrush;
            return;
        } while (false);

        textbox.BorderBrush = App.Current.Resources["SystemFillColorCriticalBrush"] as SolidColorBrush;
    }
}
