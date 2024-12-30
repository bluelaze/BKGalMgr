using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BKGalMgr.Helpers;
using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
using BKGalMgr.Views.Controls;
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
public sealed partial class MainPage : Page
{
    private static MainPage _mainpage;

    public MainPageViewModel ViewModel { get; }

    public MainPage()
    {
        ViewModel = App.GetRequiredService<MainPageViewModel>();
        DataContext = this;
        this.InitializeComponent();
        this.Loaded += MainPage_Loaded;
        _mainpage = this;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        App.MainWindow.SetTitleBar(titlebar_Grid);
    }

    public static void NavigateTo(Type pageType, object parameter = null)
    {
        _mainpage.love_and_peace_textblock.Visibility = Visibility.Collapsed;

        if (pageType == typeof(ManagePage))
        {
            if (_mainpage.root_frame.CurrentSourcePageType != typeof(LibraryAndManagePage))
            {
                _mainpage.root_frame.Navigate(typeof(LibraryAndManagePage), pageType);
            }
            else
            {
                (_mainpage.root_frame.Content as LibraryAndManagePage).NaviagteToManagePage();
            }
        }
        else
        {
            _mainpage.root_frame.Navigate(pageType, parameter);
        }
    }

    private void root_navigationview_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args
    )
    {
        love_and_peace_textblock.Visibility = Visibility.Collapsed;
        var selectedItem = args.SelectedItemContainer;
        if (selectedItem == home_navitem)
        {
            if (root_frame.CurrentSourcePageType != typeof(HomePage))
                root_frame.Navigate(typeof(HomePage));
        }
        else if (selectedItem == review_navitem)
        {
            if (root_frame.CurrentSourcePageType != typeof(ReviewPage))
                root_frame.Navigate(typeof(ReviewPage));
        }
        else if (selectedItem == migration_navitem)
        {
            if (root_frame.CurrentSourcePageType != typeof(MigrationPage))
                root_frame.Navigate(typeof(MigrationPage));
        }
        else if (selectedItem == settings_navitem)
        {
            if (root_frame.CurrentSourcePageType != typeof(SettingsPage))
                root_frame.Navigate(typeof(SettingsPage));
        }
    }

    private async void root_frame_Navigated(object sender, NavigationEventArgs e)
    {
        if (e.SourcePageType == typeof(LibraryAndManagePage))
        {
            await Task.Delay(67);
            reporitory_navitem.IsSelected = true;
            root_navigationview.SelectedItem = null;
        }
        else
        {
            reporitory_navitem.IsSelected = false;
        }
    }

    private void root_frame_Navigating(object sender, NavigatingCancelEventArgs e) { }

    private void reporitory_navitem_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (root_frame.CurrentSourcePageType == typeof(LibraryAndManagePage))
            return;
        NavigateTo(typeof(LibraryAndManagePage));
    }

    private async void add_repository_Button_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFolder folder = await FileSystemMisc.PickFolder(new() { "*" });
        if (folder != null)
        {
            App.ShowLoading();
            RepositoryInfo newRepository = new() { FolderPath = folder.Path };

            var result = await EditRepositoryInfo(newRepository);
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.LibraryAndManagePageViewModel.AddRepository(newRepository);
            }

            App.HideLoading();
        }
    }

    private async Task<ContentDialogResult> EditRepositoryInfo(RepositoryInfo repositoryInfo)
    {
        ContentDialog dialog = DialogHelper.GetConfirmDialog();
        dialog.Title = LanguageHelper.GetString("Dlg_Repository_Edit");
        dialog.Content = new RepositoryInfoControl() { Width = 720, DataContext = repositoryInfo };
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !repositoryInfo.IsValid();
        };
        return await dialog.ShowAsync();
    }

    private async void edit_repository_Menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        RepositoryInfo editRepository = (sender as MenuFlyoutItem).DataContext as RepositoryInfo;

        var result = await EditRepositoryInfo(editRepository);
        if (result == ContentDialogResult.Primary)
        {
            editRepository.SaveJsonFile();
        }
    }

    private async void remove_repository_Menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        RepositoryInfo editRepository = (sender as MenuFlyoutItem).DataContext as RepositoryInfo;
        if (await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Remove_Confirm")))
        {
            ViewModel.LibraryAndManagePageViewModel.RemoveRepository(editRepository);
        }
    }
}
