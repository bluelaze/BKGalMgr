using System;
using System.Collections.Generic;
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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BKGalMgr.Views.Controls;

public sealed partial class GamePlayedPeriodChartControl : UserControl
{
    public IEnumerable<PlayedPeriodInfo> PlayedPeriods
    {
        get { return (IEnumerable<PlayedPeriodInfo>)GetValue(PlayedPeriodsProperty); }
        set { SetValue(PlayedPeriodsProperty, value); }
    }
    public static readonly DependencyProperty PlayedPeriodsProperty = DependencyProperty.Register(
        "PlayedPeriods",
        typeof(IEnumerable<PlayedPeriodInfo>),
        typeof(GamePlayedPeriodChartControl),
        new PropertyMetadata(null)
    );

    public delegate void CloseClickEventHandler(object sender, RoutedEventArgs e);

    public event CloseClickEventHandler CloseClick;

    public GamePlayedPeriodChartControl()
    {
        this.InitializeComponent();
    }

    private void Close_Button_Click(object sender, RoutedEventArgs e)
    {
        CloseClick?.Invoke(this, e);
    }
}
