using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
using LiveChartsCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BKGalMgr.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ReviewPage : Page
{
    public ReviewPageViewModel ViewModel { get; }

    public ReviewPage()
    {
        ViewModel = App.GetRequiredService<ReviewPageViewModel>();
        DataContext = this;
        this.InitializeComponent();
        Loaded += ReviewPage_Loaded;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (!ViewModel.LibraryAndManagePageViewModel.IsLoadedRepository)
        {
            App.ShowLoading();
            await ViewModel.LibraryAndManagePageViewModel.LoadRepository();
            await ViewModel.RefreshAsync();
            App.HideLoading();
        }
    }

    private void ReviewPage_Loaded(object sender, RoutedEventArgs e)
    {
        time_axis_ScrollView.ScrollPresenter.VerticalScrollController = time_axis_AnnotatedScrollBar.ScrollController;
    }

    private void time_axis_AnnotatedScrollBar_DetailLabelRequested(
        AnnotatedScrollBar sender,
        AnnotatedScrollBarDetailLabelRequestedEventArgs args
    )
    {
        var group = ViewModel.Groups?.ElementAtOrDefault((int)args.ScrollOffset / 290 + 1);
        if (group != null)
            args.Content = group.Label.ToString("d", CultureInfo.CurrentUICulture);
    }

    private void group_header_HyperlinkButton_Click(object sender, RoutedEventArgs e)
    {
        var group = (sender as HyperlinkButton).DataContext as GameReviewGroupInfo;
        var index = ViewModel.Groups.Count - ViewModel.Groups.IndexOf(group);
        time_CartesianChart.XAxes.ElementAt(0).MinLimit = index - 15 < 0 ? 0 : index - 15;
        time_CartesianChart.XAxes.ElementAt(0).MaxLimit =
            index + 15 > ViewModel.Groups.Count ? ViewModel.Groups.Count : index + 15;
    }

    private void time_CartesianChart_DataPointerDown(
        LiveChartsCore.Kernel.Sketches.IChartView chart,
        IEnumerable<LiveChartsCore.Kernel.ChartPoint> points
    )
    {
        if (points is null)
            return;

        // notice in the chart command we are not able to use strongly typed points
        // but we can cast the point.Context.DataSource to the actual type.
        foreach (var point in points)
        {
            if (point.Context.DataSource is long ticks)
            {
                for (int i = 0; i < ViewModel.Groups.Count; i++)
                {
                    if (ViewModel.Groups[i].PlayedTime.Ticks == ticks)
                    {
                        time_axis_ScrollView.ScrollTo(0, 290 * i);
                        break;
                    }
                }
            }
        }
    }

    private void group_game_name_HyperlinkButton_Click(object sender, RoutedEventArgs e)
    {
        var gameItem = (sender as HyperlinkButton).DataContext as GameReviewGroupItem;
        MainPage.NavigateTo(typeof(LibraryAndManagePage), gameItem.Game);
    }

    private async void review_refresh_Button_Click(object sender, RoutedEventArgs e)
    {
        App.ShowLoading();
        await ViewModel.RefreshAsync();
        App.HideLoading();
    }
}
