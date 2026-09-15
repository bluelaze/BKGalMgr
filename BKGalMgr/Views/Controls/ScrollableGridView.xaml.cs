using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Dispatching;
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

public partial class ScrollableGridViewItem : ObservableObject
{
    [ObservableProperty]
    public partial List<object> Items { get; set; }

    [ObservableProperty]
    public partial DataTemplate ItemTemplate { get; set; }

    [ObservableProperty]
    public partial double PageWidth { get; set; }

    [ObservableProperty]
    public partial int Columns { get; set; }

    [ObservableProperty]
    public partial int Rows { get; set; }

    [ObservableProperty]
    public partial double ColumnSpacing { get; set; }

    [ObservableProperty]
    public partial double RowSpacing { get; set; }
}

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
        new PropertyMetadata(default(DataTemplate), OnPropertyChanged)
    );

    public IEnumerable ItemsSource
    {
        get { return (IEnumerable)GetValue(ItemsSourceProperty); }
        set { SetValue(ItemsSourceProperty, value); }
    }
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(ScrollableGridView),
        new PropertyMetadata(default(IEnumerable), OnItemsSourceChanged)
    );

    public double MinItemWidth
    {
        get => (double)GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    public static readonly DependencyProperty MinItemWidthProperty = DependencyProperty.Register(
        nameof(MinItemWidth),
        typeof(double),
        typeof(ScrollableGridView),
        new PropertyMetadata(100, OnPropertyChanged)
    );

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
        nameof(ItemHeight),
        typeof(double),
        typeof(ScrollableGridView),
        new PropertyMetadata(100, OnPropertyChanged)
    );

    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }
    public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
        nameof(Rows),
        typeof(int),
        typeof(ScrollableGridView),
        new PropertyMetadata(0, OnPropertyChanged)
    );

    public double ColumnSpacing
    {
        get => (double)GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }
    public static readonly DependencyProperty ColumnSpacingProperty = DependencyProperty.Register(
        nameof(ColumnSpacing),
        typeof(double),
        typeof(ScrollableGridView),
        new PropertyMetadata(16, OnPropertyChanged)
    );

    public double RowSpacing
    {
        get => (double)GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }
    public static readonly DependencyProperty RowSpacingProperty = DependencyProperty.Register(
        nameof(RowSpacing),
        typeof(double),
        typeof(ScrollableGridView),
        new PropertyMetadata(16, OnPropertyChanged)
    );

    public delegate void ItemsControlClickEventHandler(object sender, object clickedItem);
    public event ItemsControlClickEventHandler ItemClick;

    private int _columns = 1;
    private double _pageWidth = 0;
    private ObservableCollection<ScrollableGridViewItem> _itemSource { get; set; } = new();

    public ScrollableGridView()
    {
        this.InitializeComponent();
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ScrollableGridView)d;
        control.UpdateLayoutCalculations(control.scrollable_FlipView.ActualWidth);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ScrollableGridView)d;
        if (e.OldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= control.OnExternalCollectionChanged;

        if (e.NewValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += control.OnExternalCollectionChanged;

        control.ScheduleUpdate();
    }

    private void OnExternalCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        ScheduleUpdate();
    }

    private void ItemsControl_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (ItemClick == null)
            return;

        // 1. 获取真正被点击的最底层 UI 元素 (比如 Image 或者 Border)
        if (e.OriginalSource is FrameworkElement clickedElement)
        {
            // 2. 它的 DataContext 就是对应的 Item 数据模型
            Type dataType = null;
            foreach (var item in ItemsSource)
            {
                dataType = item.GetType();
                break;
            }
            if (clickedElement.DataContext.GetType() == dataType)
            {
                ItemClick.Invoke(sender, clickedElement.DataContext);
                e.Handled = true;
            }
        }
    }

    private void scrollable_FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        left_scroll_Button.IsEnabled = scrollable_FlipView.SelectedIndex > 0;
        right_scroll_Button.IsEnabled = scrollable_FlipView.SelectedIndex < scrollable_FlipView.Items.Count - 1;
    }

    private void left_scroll_Button_Click(object sender, RoutedEventArgs e)
    {
        scrollable_FlipView.SelectedIndex--;
    }

    private void right_scroll_Button_Click(object sender, RoutedEventArgs e)
    {
        scrollable_FlipView.SelectedIndex++;
    }

    private DispatcherQueueTimer _resizeTimer;

    private void root_Grid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width <= 0 || e.NewSize.Width == e.PreviousSize.Width)
            return;

        if (_resizeTimer == null)
        {
            _resizeTimer = DispatcherQueue.CreateTimer();
            _resizeTimer.Interval = TimeSpan.FromMilliseconds(150); // 防抖延迟
            _resizeTimer.Tick += async (s, args) =>
            {
                _resizeTimer.Stop();
                UpdateLayoutCalculations(root_Grid.ActualWidth);
            };
        }
        _resizeTimer.Stop();
        _resizeTimer.Start();
    }

    private void UpdateLayoutCalculations(double availableWidth)
    {
        if (availableWidth <= 0)
            availableWidth = root_Grid.ActualWidth;

        if (availableWidth <= 0)
            return;

        int columns = (int)((availableWidth - Padding.Right + ColumnSpacing) / (MinItemWidth + ColumnSpacing));
        columns = Math.Max(1, columns);

        if (columns == _columns && availableWidth == _pageWidth)
            return;

        _columns = columns;
        _pageWidth = availableWidth;

        ScheduleUpdate();
    }

    private bool _isUpdatePending = false;

    private void ScheduleUpdate()
    {
        if (_isUpdatePending)
            return;

        _isUpdatePending = true;

        // 🌟 提交到事件循环末尾，无论这一帧改了多少个属性，最终只执行一次 RecalculateLayout
        DispatcherQueue.TryEnqueue(
            DispatcherQueuePriority.Low,
            () =>
            {
                RecalculatePages();
                _isUpdatePending = false;
            }
        );
    }

    private void RecalculatePages()
    {
        if (ItemsSource == null)
        {
            _itemSource.Clear();
            scrollable_FlipView.Visibility = Visibility.Collapsed;
            return;
        }

        var allItems = ItemsSource.Cast<object>().ToList();
        if (allItems.Count == 0)
        {
            _itemSource.Clear();
            scrollable_FlipView.Visibility = Visibility.Collapsed;
            return;
        }

        int rows = 0;
        int pageSize = allItems.Count;
        if (Rows > 0)
        {
            rows = Rows;
            pageSize = _columns * rows;
        }
        if (allItems.Count <= pageSize)
        {
            rows = 0;
        }

        // 高度需要手动计算
        int ActualRows = rows;
        if (ActualRows == 0)
        {
            ActualRows = allItems.Count / _columns;
            if (allItems.Count % _columns > 0)
            {
                ActualRows++;
            }
        }
        if (ActualRows > 0)
        {
            scrollable_FlipView.Height = ActualRows * (ItemHeight + RowSpacing) - RowSpacing;
            scrollable_FlipView.Visibility = Visibility.Visible;
        }

        ObservableCollection<ScrollableGridViewItem> newItemSource = new();
        for (int i = 0; i < allItems.Count; i += pageSize)
        {
            var pageItems = allItems.Skip(i).Take(pageSize).ToList();

            newItemSource.Add(
                new ScrollableGridViewItem
                {
                    Items = new(pageItems),
                    ItemTemplate = ItemTemplate,
                    PageWidth = _pageWidth,
                    Columns = _columns,
                    Rows = rows,
                    ColumnSpacing = ColumnSpacing,
                    RowSpacing = RowSpacing,
                }
            );
        }

        int addCount = newItemSource.Count - _itemSource.Count;
        int sameCount = Math.Min(newItemSource.Count, _itemSource.Count);
        for (int i = 0; i < sameCount; i++)
        {
            _itemSource[i].ItemTemplate = newItemSource[i].ItemTemplate;
            _itemSource[i].Rows = newItemSource[i].Rows;
            _itemSource[i].ColumnSpacing = newItemSource[i].ColumnSpacing;
            _itemSource[i].RowSpacing = newItemSource[i].RowSpacing;

            _itemSource[i].PageWidth = newItemSource[i].PageWidth;
            _itemSource[i].Columns = newItemSource[i].Columns;
            _itemSource[i].Items = newItemSource[i].Items;
        }
        if (addCount > 0)
        {
            for (int i = sameCount; i < newItemSource.Count; i++)
            {
                _itemSource.Add(newItemSource[i]);
            }
        }
        else if (addCount < 0)
        {
            for (int i = _itemSource.Count - 1; i >= newItemSource.Count; i--)
            {
                _itemSource.RemoveAt(i);
            }
        }

        if (_itemSource.Count <= 1)
        {
            left_scroll_Button.Visibility = Visibility.Collapsed;
            right_scroll_Button.Visibility = Visibility.Collapsed;
        }
        else
        {
            left_scroll_Button.Visibility = Visibility.Visible;
            right_scroll_Button.Visibility = Visibility.Visible;

            left_scroll_Button.IsEnabled = scrollable_FlipView.SelectedIndex > 0;
            right_scroll_Button.IsEnabled = scrollable_FlipView.SelectedIndex < _itemSource.Count - 1;
        }
    }
}
