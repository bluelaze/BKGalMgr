using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using BKGalMgr.Helpers;
using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
using BKGalMgr.Views.Controls;
using H.NotifyIcon;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
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

namespace BKGalMgr.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LibraryPage : Page
{
    public LibraryAndManagePageViewModel ViewModel { get; }

    public LibraryPage()
    {
        ViewModel = App.GetRequiredService<LibraryAndManagePageViewModel>();
        DataContext = this;
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (!ViewModel.IsLoadedRepository)
        {
            App.ShowLoading();
            await ViewModel.LoadRepository();
            App.HideLoading();
        }
        if (e.Parameter is GameInfo game)
        {
            game.Repository.SelectedGame = game;
            ViewModel.SelectedRepository = game.Repository;

            games_ListView.SelectedItem = game;
            games_ListView.ScrollIntoView(game);
            games_ListView.MakeVisible(new SemanticZoomLocation() { Item = game });
            games_ListView.UpdateLayout();
        }
    }

    private void gamename_linkbutton_Click(object sender, RoutedEventArgs e)
    {
        var gameInfo = (sender as FrameworkElement).DataContext as GameInfo;
        if (gameInfo != null)
        {
            ViewModel.SelectedRepository.SelectedGame = gameInfo;
            MainPage.NavigateTo(typeof(ManagePage));
        }
    }

    private void capture_hotkey_button_Click(object sender, RoutedEventArgs e)
    {
        var targetInfo = (sender as FrameworkElement).DataContext as TargetInfo;
        App.MainWindow.Hide();
        Task.Delay(225)
            .ContinueWith(
                t =>
                {
                    targetInfo.DoScreenCapture();
                    App.MainWindow.ShowWindow();
                },
                TaskScheduler.FromCurrentSynchronizationContext()
            );
    }

    private void play_Button_Click(object sender, RoutedEventArgs e)
    {
        var gameInfo = (sender as FrameworkElement).DataContext as GameInfo;
        if (gameInfo != null)
        {
            App.MainWindow.NavigateToGamePlayPage(gameInfo);
        }
    }

    private void group_togglebutton_IsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedRepository.IsEnableGroup)
        {
            // delay to IsChecked TwoWay binding valid
            Task.Delay(33)
                .ContinueWith(
                    _ => ViewModel.SelectedRepository.GamesViewRefreshFilter(),
                    TaskScheduler.FromCurrentSynchronizationContext()
                );
        }
    }

    private async Task<ContentDialogResult> EditGroupInfo(GroupInfo groupInfo)
    {
        GroupInfoControl groupInfoControl = new() { Width = 720, DataContext = groupInfo };

        ContentDialog dialog = DialogHelper.GetConfirmDialog();
        dialog.Title = LanguageHelper.GetString("Dlg_Group_Edit");
        dialog.Content = groupInfoControl;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = groupInfo.Name.IsNullOrEmpty();
        };

        return await dialog.ShowAsync();
    }

    private async void add_group_button_Click(object sender, RoutedEventArgs e)
    {
        App.ShowLoading();
        GroupInfo groupInfo = new();
        var result = await EditGroupInfo(groupInfo);
        if (result == ContentDialogResult.Primary && !groupInfo.Name.IsNullOrEmpty())
        {
            ViewModel.SelectedRepository.GroupChanged(null, groupInfo, GroupChangedType.Add);
        }
        App.HideLoading();
    }

    private async void edit_group_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        GroupInfo groupInfo = (sender as MenuFlyoutItem).DataContext as GroupInfo;
        GroupInfo newGroupInfo = JsonMisc.Deserialize<GroupInfo>(JsonMisc.Serialize(groupInfo));
        var result = await EditGroupInfo(newGroupInfo);
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.SelectedRepository.GroupChanged(groupInfo, newGroupInfo, GroupChangedType.Edit);
        }
    }

    private async void delete_group_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        GroupInfo groupInfo = (sender as MenuFlyoutItem).DataContext as GroupInfo;
        if (await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Delete_Confirm")))
        {
            App.ShowLoading();
            ViewModel.SelectedRepository.GroupChanged(groupInfo, null, GroupChangedType.Remove);
            App.HideLoading();
        }
    }

    private void MetadataControl_ItemClick(object sender, MetadataControl.MetadataItemClickEventArgs e)
    {
        ViewModel.SelectedRepository.SearchText = string.Empty;
        ViewModel.SelectedRepository.SearchToken.Clear();
        ViewModel.SelectedRepository.SearchToken.Add(e.ClickedItem.Label);
    }

    private void games_view_semanticzoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
    {
        if (e.IsSourceZoomedInView == false)
        {
            e.DestinationItem.Item = e.SourceItem.Item;
            games_ListView.SelectedItem = e.SourceItem.Item;
        }
        else
        {
            e.DestinationItem.Item = games_ListView.SelectedItem;
        }
    }

    private void switch_gameview_button_Click(object sender, RoutedEventArgs e)
    {
        games_view_semanticzoom.ToggleActiveView();
    }

    private void game_group_Flyout_Opened(object sender, object e)
    {
        if (VisualTreeHelper.GetOpenPopupsForXamlRoot(this.XamlRoot).FirstOrDefault() is not { } popup)
        {
            return;
        }
        var gameGroupItemsView = popup.Child.FindDescendant("game_group_ItemsView") as ItemsView;
        var gameInfo = gameGroupItemsView.DataContext as GameInfo;
        var gameGroups = new BindingList<GroupInfo>();
        foreach (var g in ViewModel.SelectedRepository.Groups)
        {
            if (g.Name == GlobalInfo.GroupItemCase_Add)
                continue;
            gameGroups.Add(new() { Name = g.Name, IsChecked = gameInfo.Group.Any(t => t == g.Name) });
        }
        gameGroups.ListChanged += (object sender, ListChangedEventArgs e) =>
        {
            if (sender is BindingList<GroupInfo> gl)
            {
                foreach (var item in gl)
                {
                    if (item.IsChecked)
                    {
                        if (!gameInfo.Group.Contains(item.Name))
                            gameInfo.Group.Add(item.Name);
                    }
                    else
                    {
                        if (gameInfo.Group.Contains(item.Name))
                            gameInfo.Group.Remove(item.Name);
                    }
                }
            }
        };
        gameGroupItemsView.ItemsSource = gameGroups;
    }

    private void cover_Button_Click(object sender, RoutedEventArgs e)
    {
        var gameInfo = (sender as Button).DataContext as GameInfo;
        App.ShowImages(gameInfo.Covers, 0);
    }

    private void gallery_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var gameInfo = (sender as MenuFlyoutItem).DataContext as GameInfo;
        gameInfo.LoadGallery();
        if (gameInfo.Gallery.Count > 0)
            App.ShowImages(gameInfo.Gallery, 0);
    }

    private void special_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var gameInfo = (sender as MenuFlyoutItem).DataContext as GameInfo;
        gameInfo.LoadSpecial();
        if (gameInfo.Special.Count > 0)
            App.ShowImages(gameInfo.Special, 0);
    }

    private void screenshot_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var gameInfo = (sender as MenuFlyoutItem).DataContext as GameInfo;
        gameInfo.LoadScreenCapture();
        if (gameInfo.ScreenCaptures.Count > 0)
            App.ShowImages(gameInfo.ScreenCaptures, 0);
    }

    private void gameinfo_SplitView_PaneOpening(SplitView sender, object args)
    {
        var gameInfo = sender.DataContext as GameInfo;
        var playedChart = new GamePlayedPeriodChartControl() { PlayedPeriods = gameInfo.PlayedPeriods };
        playedChart.CloseClick += (_, _) =>
        {
            sender.IsPaneOpen = false;
        };
        sender.Pane = playedChart;
    }

    private void gameinfo_SplitView_PaneClosed(SplitView sender, object args)
    {
        sender.Pane = null;
    }

    private void goto_top_Button_Click(object sender, RoutedEventArgs e)
    {
        games_ListView.FindDescendant<ScrollViewer>()?.ChangeView(0, 0, null);
        games_GridView.FindDescendant<ScrollViewer>()?.ChangeView(0, 0, null);
    }
}
