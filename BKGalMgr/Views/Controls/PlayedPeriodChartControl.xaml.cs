using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView.WinUI;
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

public sealed partial class PlayedPeriodChartControl : UserControl
{
    public IEnumerable<ISeries> Series
    {
        get { return (IEnumerable<ISeries>)GetValue(SeriesProperty); }
        set { SetValue(SeriesProperty, value); }
    }
    public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register(
        "Series",
        typeof(IEnumerable<ISeries>),
        typeof(PlayedPeriodChartControl),
        new PropertyMetadata(null)
    );

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public PlayedPeriodChartControl()
    {
        this.InitializeComponent();
    }
}
