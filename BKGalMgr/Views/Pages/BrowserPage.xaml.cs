using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Devices.Lights;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BKGalMgr.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BrowserPage : Page
{
    public BrowserPageViewModel ViewModel { get; }

    public BrowserPage()
    {
        ViewModel = App.GetRequiredService<BrowserPageViewModel>();
        DataContext = this;
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (!ViewModel.LibraryAndManagePageViewModel.IsLoadedRepository)
        {
            App.ShowLoading();

            await ViewModel.LibraryAndManagePageViewModel.LoadRepository();

            App.HideLoading();
        }

        if (!ViewModel.Games.Any())
        {
            ViewModel.Refresh();
        }
    }

    private void FilterGames(List<string> searchToken)
    {
        ViewModel.SearchText = string.Empty;
        ViewModel.SearchToken.Clear();
        ViewModel.SearchToken.AddRange(searchToken);
    }

    public List<GameInfo> GetSelectedGames()
    {
        if (select_ToggleButton.IsChecked == true)
        {
            return games_GridView.SelectedItems.Select(t => t as GameInfo).ToList();
        }
        return games_GridView.Items.Select(t => t as GameInfo).ToList();
    }

    private void group_Togglebutton_IsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsEnableGroup)
        {
            // delay to IsChecked TwoWay binding valid
            Task.Delay(33)
                .ContinueWith(
                    _ => ViewModel.GamesViewRefreshFilter(),
                    TaskScheduler.FromCurrentSynchronizationContext()
                );
        }
    }

    private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        var control = sender as UserControl;
        VisualStateManager.GoToState(control, "PointerOver", false);
    }

    private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        var control = sender as UserControl;
        VisualStateManager.GoToState(control, "Normal", false);
    }

    private void games_GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        var gameInfo = e.ClickedItem as GameInfo;
        App.MainWindow.NavigateToGamePlayPage(gameInfo);
    }

    private void blog_Button_Click(object sender, RoutedEventArgs e)
    {
        var gameInfo = (sender as FrameworkElement).DataContext as GameInfo;
        App.MainWindow.ShowBlog(gameInfo);
    }

    private void refresh_Button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Refresh();
    }

    private void goto_top_Button_Click(object sender, RoutedEventArgs e)
    {
        games_GridView.FindDescendant<ScrollViewer>()?.ChangeView(0, 0, null);
    }

    private void repository_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectedRepositories = repository_ListView.SelectedItems.Select(t => t as RepositoryInfo).ToList();
        if (repository_ToggleSplitButton.IsChecked)
            ViewModel.GamesViewRefreshFilter();
    }

    private void company_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectedCompanines = company_ListView.SelectedItems.Select(t => t as string).ToList();
        if (company_ToggleSplitButton.IsChecked)
            ViewModel.GamesViewRefreshFilter();
    }

    private void invert_select_Button_Click(object sender, RoutedEventArgs e)
    {
        var selectedIndices = games_GridView.SelectedItems.Select(item => games_GridView.Items.IndexOf(item)).ToList();
        selectedIndices.Sort(); // 前面几个有时候是乱的，要排序

        if (selectedIndices.Count == 0)
        {
            games_GridView.SelectAll();
            return;
        }

        List<(int Start, int End)> selectedRanges = new();
        List<(int Start, int End)> unselectedRanges = new();
        int selectedStart = selectedIndices[0];
        // 1. 检查开头是否有未选中的区间 (例如列表从 3 开始，那么 0-2 就是未选中)
        if (selectedStart > 0)
        {
            unselectedRanges.Add((0, selectedStart - 1));
        }

        // 2. 遍历已排序的列表，同步计算两种区间
        for (int i = 1; i < selectedIndices.Count; i++)
        {
            // 如果数字不连续，说明中间有断层（即存在未选中区间）
            if (selectedIndices[i] > selectedIndices[i - 1] + 1)
            {
                // 结算【在列表内】的区间
                selectedRanges.Add((selectedStart, selectedIndices[i - 1]));
                // 结算【不在列表内】的区间
                unselectedRanges.Add((selectedIndices[i - 1] + 1, selectedIndices[i] - 1));
                // 开启新的已选中区间起点
                selectedStart = selectedIndices[i];
            }
        }

        // 3. 结算最后一个【在列表内】的区间
        int lastSelected = selectedIndices[selectedIndices.Count - 1];
        selectedRanges.Add((selectedStart, lastSelected));

        // 4. 检查结尾是否有未选中的区间 (例如列表在 95 结束，那么 96-100 就是未选中)
        if (lastSelected < games_GridView.Items.Count - 1)
        {
            unselectedRanges.Add((lastSelected + 1, games_GridView.Items.Count - 1));
        }

        foreach (var item in selectedRanges)
        {
            games_GridView.DeselectRange(new(item.Start, (uint)(item.End - item.Start + 1)));
        }
        foreach (var item in unselectedRanges)
        {
            games_GridView.SelectRange(new(item.Start, (uint)(item.End - item.Start + 1)));
        }
    }

    private void website_HyperlinkButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as HyperlinkButton).Content is TextBlock tb)
            FilterGames([tb.Text]);
        else
            FilterGames([(sender as HyperlinkButton).Content as string]);
    }

    private void MetadataControl_ItemClick(object sender, Controls.MetadataControl.MetadataItemClickEventArgs e)
    {
        FilterGames([e.ClickedItem.Label]);
    }

    private void tag_ItemsView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        FilterGames([args.InvokedItem as string]);
    }
}
