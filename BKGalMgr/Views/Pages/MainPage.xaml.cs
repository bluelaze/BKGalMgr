using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public MainPage()
    {
        this.InitializeComponent();
        _mainpage = this;
    }

    public static void NavigateTo(Type pageType)
    {
        if (pageType == typeof(ManagePage))
            _mainpage.navigationview_root.SelectedItem = _mainpage.navitem_manage;
    }

    private void navigationview_root_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        textblock_love_and_peace.Visibility = Visibility.Collapsed;
        var selectedItem = args.SelectedItemContainer;
        if (selectedItem == navitem_manage)
        {
            frame_root.Navigate(typeof(ManagePage));
        }
        if (selectedItem == navitem_games)
        {
            frame_root.Navigate(typeof(GamesPage));
        }
    }

    private void frame_root_Navigated(object sender, NavigationEventArgs e)
    {

    }

    private void frame_root_Navigating(object sender, NavigatingCancelEventArgs e)
    {

    }
}
