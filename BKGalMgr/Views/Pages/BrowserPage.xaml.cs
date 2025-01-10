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
}
