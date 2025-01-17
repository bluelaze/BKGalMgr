using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

public sealed partial class ScrollableGridView : UserControl
{
    public object Header
    {
        get { return (object)GetValue(HeaderProperty); }
        set { SetValue(HeaderProperty, value); }
    }
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header),
        typeof(object),
        typeof(ScrollableGridView),
        new PropertyMetadata(default(object))
    );

    public DataTemplate ItemTemplate
    {
        get { return (DataTemplate)GetValue(ItemTemplateProperty); }
        set { SetValue(ItemTemplateProperty, value); }
    }
    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
        nameof(ItemTemplate),
        typeof(DataTemplate),
        typeof(ScrollableGridView),
        new PropertyMetadata(default(DataTemplate))
    );

    public object ItemsSource
    {
        get { return (object)GetValue(ItemsSourceProperty); }
        set { SetValue(ItemsSourceProperty, value); }
    }
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(object),
        typeof(ScrollableGridView),
        new PropertyMetadata(default(object))
    );

    public event ItemClickEventHandler ItemClick;

    private ScrollViewer _scrollViewer;

    public ScrollableGridView()
    {
        this.InitializeComponent();
    }

    private void scrollable_GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        ItemClick?.Invoke(sender, e);
    }

    private void scrollable_GridView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_scrollViewer == null)
            _scrollViewer = scrollable_GridView.FindDescendant<ScrollViewer>();

        if (_scrollViewer != null)
        {
            _scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
            _scrollViewer.SizeChanged += ScrollViewer_SizeChanged;
            ScrollViewer_SizeChanged(_scrollViewer, null);
        }
    }

    private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
    {
        var scroller = (ScrollViewer)sender;
        left_scroll_Button.IsEnabled = e.FinalView.HorizontalOffset > 0;
        right_scroll_Button.IsEnabled = e.FinalView.HorizontalOffset < scroller.ScrollableWidth;
    }

    private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs _)
    {
        var scroller = (ScrollViewer)sender;
        if (scroller.ScrollableWidth > 0)
        {
            left_scroll_Button.Visibility = Visibility.Visible;
            right_scroll_Button.Visibility = Visibility.Visible;
        }
        else
        {
            left_scroll_Button.Visibility = Visibility.Collapsed;
            right_scroll_Button.Visibility = Visibility.Collapsed;
        }
        left_scroll_Button.IsEnabled = scroller.HorizontalOffset > 0;
        right_scroll_Button.IsEnabled = scroller.HorizontalOffset < scroller.ScrollableWidth;
    }

    private double GetScrollIncrement()
    {
        double increment = 0;
        if (scrollable_GridView.ContainerFromIndex(0) is FrameworkElement item)
        {
            double unit = item.ActualWidth + 20;
            increment = ((int)(_scrollViewer.ViewportWidth / unit)) * unit;
        }
        if (increment == 0)
            increment = _scrollViewer.ViewportWidth;

        return increment;
    }

    private void left_scroll_Button_Click(object sender, RoutedEventArgs e)
    {
        if (_scrollViewer == null)
            return;
        _scrollViewer.ChangeView(_scrollViewer.HorizontalOffset - GetScrollIncrement(), null, null);
    }

    private void right_scroll_Button_Click(object sender, RoutedEventArgs e)
    {
        if (_scrollViewer == null)
            return;
        _scrollViewer.ChangeView(_scrollViewer.HorizontalOffset + GetScrollIncrement(), null, null);
    }
}
