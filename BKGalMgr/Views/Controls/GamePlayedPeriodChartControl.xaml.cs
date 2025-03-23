using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BKGalMgr.ViewModels;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SkiaSharp;
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
        nameof(PlayedPeriods),
        typeof(IEnumerable<PlayedPeriodInfo>),
        typeof(GamePlayedPeriodChartControl),
        new PropertyMetadata(
            null,
            (DependencyObject o, DependencyPropertyChangedEventArgs args) =>
            {
                var chart = (GamePlayedPeriodChartControl)o;
                var playedPeriods = (IEnumerable<PlayedPeriodInfo>)args.NewValue;
                if (playedPeriods == null)
                {
                    chart.XAxes = null;
                    chart.YAxes = null;
                }
                else
                {
                    SKColor labelColor = new SKColor(128, 128, 128);
                    if (chart.Foreground is SolidColorBrush colorBrush)
                        labelColor = new SKColor(
                            colorBrush.Color.R,
                            colorBrush.Color.G,
                            colorBrush.Color.B,
                            colorBrush.Color.A
                        );

                    chart.XAxes = new List<ICartesianAxis>()
                    {
                        new Axis()
                        {
                            LabelsPaint = new SolidColorPaint(labelColor),
                            TextSize = 14,
                            Labels = playedPeriods
                                .Select(t => t.Coordinate.SecondaryValue.AsDate().ToString())
                                .Reverse()
                                .ToList(),
                        },
                    };

                    chart.YAxes = new List<ICartesianAxis>()
                    {
                        new Axis
                        {
                            LabelsPaint = new SolidColorPaint(labelColor),
                            TextSize = 14,
                            SeparatorsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(100)),
                            Labeler = value => value.AsTimeSpan().Format("hhmmss"),
                            //MaxLimit = TimeSpan.FromHours(1).Ticks,
                            MinLimit = 0,
                        },
                    };
                }
            }
        )
    );

    public Visibility ShowClose
    {
        get { return (Visibility)GetValue(ShowCloseProperty); }
        set { SetValue(ShowCloseProperty, value); }
    }
    public static readonly DependencyProperty ShowCloseProperty = DependencyProperty.Register(
        nameof(ShowClose),
        typeof(Visibility),
        typeof(GamePlayedPeriodChartControl),
        new PropertyMetadata(Visibility.Visible)
    );

    public IEnumerable<ICartesianAxis> XAxes
    {
        get => (IEnumerable<ICartesianAxis>)GetValue(XAxesProperty);
        set => SetValue(XAxesProperty, value);
    }

    public IEnumerable<ICartesianAxis> YAxes
    {
        get => (IEnumerable<ICartesianAxis>)GetValue(YAxesProperty);
        set => SetValue(YAxesProperty, value);
    }

    public static readonly DependencyProperty XAxesProperty = DependencyProperty.Register(
        nameof(XAxes),
        typeof(IEnumerable<ICartesianAxis>),
        typeof(GamePlayedPeriodChartControl),
        new PropertyMetadata(null)
    );

    public static readonly DependencyProperty YAxesProperty = DependencyProperty.Register(
        nameof(YAxes),
        typeof(IEnumerable<ICartesianAxis>),
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
