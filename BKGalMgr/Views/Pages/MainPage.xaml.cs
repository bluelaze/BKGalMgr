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

    private object _navigateParameter = null;

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
        _mainpage._navigateParameter = parameter;
        if (pageType == typeof(HomePage))
        {
            _mainpage.root_navigationview.SelectedItem = _mainpage.home_navitem;
        }
        else if (pageType == typeof(BrowserPage))
        {
            _mainpage.root_navigationview.SelectedItem = _mainpage.browser_navitem;
        }
        else if (pageType == typeof(ReviewPage))
        {
            _mainpage.root_navigationview.SelectedItem = _mainpage.review_navitem;
        }
        else if (pageType == typeof(MigrationPage))
        {
            _mainpage.root_navigationview.SelectedItem = _mainpage.migration_navitem;
        }
        else if (pageType == typeof(LibraryAndManagePage))
        {
            _mainpage.root_navigationview.SelectedItem = _mainpage.repository_navitem;
            (_mainpage.root_frame.Content as LibraryAndManagePage).NavigateTo(typeof(LibraryPage), parameter);
        }
        else if (pageType == typeof(ManagePage))
        {
            _mainpage.root_navigationview.SelectedItem = _mainpage.repository_navitem;
            (_mainpage.root_frame.Content as LibraryAndManagePage).NavigateTo(typeof(ManagePage), parameter);
        }
        else if (pageType == typeof(SettingsPage))
        {
            _mainpage.root_navigationview.SelectedItem = _mainpage.settings_navitem;
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
                root_frame.Navigate(typeof(HomePage), _navigateParameter);
        }
        else if (selectedItem == browser_navitem)
        {
            if (root_frame.CurrentSourcePageType != typeof(BrowserPage))
                root_frame.Navigate(typeof(BrowserPage), _navigateParameter);
        }
        else if (selectedItem == review_navitem)
        {
            if (root_frame.CurrentSourcePageType != typeof(ReviewPage))
                root_frame.Navigate(typeof(ReviewPage), _navigateParameter);
        }
        else if (selectedItem == migration_navitem)
        {
            if (root_frame.CurrentSourcePageType != typeof(MigrationPage))
                root_frame.Navigate(typeof(MigrationPage), _navigateParameter);
        }
        else if (selectedItem == repository_navitem)
        {
            if (root_frame.CurrentSourcePageType != typeof(LibraryAndManagePage))
                root_frame.Navigate(typeof(LibraryAndManagePage), _navigateParameter);
        }
        else if (selectedItem == repository_listview_navitem)
        {
            root_navigationview.SelectedItem = repository_navitem;
        }
        else if (selectedItem == settings_navitem)
        {
            if (root_frame.CurrentSourcePageType != typeof(SettingsPage))
                root_frame.Navigate(typeof(SettingsPage), _navigateParameter);
        }
        _navigateParameter = null;
    }

    private void root_frame_Navigated(object sender, NavigationEventArgs e) { }

    private void root_frame_Navigating(object sender, NavigatingCancelEventArgs e) { }

    private void root_frame_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (repository_gridview_Popup.IsOpen)
            RefreshRepositoryGridView();
    }

    private void repository_listview_navitem_Loaded(object sender, RoutedEventArgs e)
    {
        if (repository_listview_navitem.FindDescendant("ContentGrid") is Grid ContentGrid)
        {
            ContentGrid.Margin = new(0);
            if (ContentGrid.Children[0] is Border IconColumn)
                IconColumn.Visibility = Visibility.Collapsed;
        }
    }

    private void repository_ListView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (repository_ListView.ActualHeight > 50)
        {
            if (root_navigationview.FindDescendant("FooterItemsScrollViewer") is ScrollViewer scrollViewer)
            {
                double maxHeight = 0;
                foreach (var item in root_navigationview.FooterMenuItems)
                {
                    if (item is UIElement ele)
                    {
                        maxHeight += ele.ActualSize.Y;
                    }
                }
                scrollViewer.MaxHeight = maxHeight;
            }
            repository_listview_navitem.IsEnabled = true;
        }
        else
        {
            repository_listview_navitem.IsEnabled = false;
        }
        if (repository_gridview_Popup.IsOpen)
            RefreshRepositoryGridView();
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

    private void repository_ListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
    {
        ViewModel.LibraryAndManagePageViewModel.SaveRepositoryOrder();
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

    private void RefreshRepositoryGridView()
    {
        repository_Grid.Width = root_frame.ActualWidth;
        repository_Grid.Height =
            +repository_listview_navitem.ActualHeight
            + repository_NavigationViewItemSeparator.ActualHeight * 2
            - 1
            + settings_navitem.ActualHeight;
    }

    private void repository_gridview_Button_Click(object sender, RoutedEventArgs e)
    {
        RefreshRepositoryGridView();
        repository_gridview_Popup.IsOpen = true;
    }

    private void repository_gridview_ToggleButton_IsChecked(object sender, RoutedEventArgs e)
    {
        if (repository_gridview_ToggleButton.IsChecked == true)
        {
            // 需要先关闭再打开，否则属性不生效
            repository_gridview_Popup.IsLightDismissEnabled = false;
            repository_gridview_Popup.IsOpen = false;
            repository_gridview_Popup.IsOpen = true;
        }
        else
        {
            repository_gridview_Popup.IsLightDismissEnabled = true;
            repository_gridview_Popup.IsOpen = false;
        }
    }

    private void repository_gridview_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        repository_ListView.ScrollIntoView(ViewModel.LibraryAndManagePageViewModel.SelectedRepository);
    }
}
