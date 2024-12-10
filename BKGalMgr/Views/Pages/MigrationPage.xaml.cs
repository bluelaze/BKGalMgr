using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BKGalMgr.Helpers;
using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
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
public sealed partial class MigrationPage : Page
{
    public MigratePageViewModel ViewModel { get; }

    public MigrationPage()
    {
        ViewModel = App.GetRequiredService<MigratePageViewModel>();
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
    }

    private void game_name_HyperlinkButton_Click(object sender, RoutedEventArgs e)
    {
        var gameItem = (sender as HyperlinkButton).DataContext as GameInfo;
        gameItem.Repository.SelectedGame = gameItem;
        ViewModel.LibraryAndManagePageViewModel.SelectedRepository = gameItem.Repository;
        MainPage.NavigateTo(typeof(LibraryPage), gameItem);
    }

    private async void move_game_left_to_right_Button_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.LeftRepository == null || ViewModel.RightRepository == null)
            return;
        if (
            await DialogHelper.ShowConfirm(
                LanguageHelper
                    .GetString("Msg_Migrate_Confirm")
                    .Format(
                        left_repository_ListView.SelectedItems.Count,
                        ViewModel.LeftRepository.Name,
                        ViewModel.RightRepository.Name
                    )
            )
        )
        {
            App.ShowLoading();
            var games = left_repository_ListView.SelectedItems.Select(i => i as GameInfo).ToList();
            if (await ViewModel.MigrateGames(games, true) == false)
                await DialogHelper.ShowError(LanguageHelper.GetString("Msg_Migrate_Exception"));
            App.HideLoading();
        }
    }

    private async void move_game_right_to_left_Button_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.LeftRepository == null || ViewModel.RightRepository == null)
            return;
        if (
            await DialogHelper.ShowConfirm(
                LanguageHelper
                    .GetString("Msg_Migrate_Confirm")
                    .Format(
                        right_repository_ListView.SelectedItems.Count,
                        ViewModel.RightRepository.Name,
                        ViewModel.LeftRepository.Name
                    )
            )
        )
        {
            App.ShowLoading();
            var games = right_repository_ListView.SelectedItems.Select(i => i as GameInfo).ToList();
            if (await ViewModel.MigrateGames(games, false) == false)
                await DialogHelper.ShowError(LanguageHelper.GetString("Msg_Migrate_Exception"));
            App.HideLoading();
        }
    }
}
