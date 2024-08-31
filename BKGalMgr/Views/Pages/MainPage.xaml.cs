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
            _mainpage.root_navigationview.SelectedItem = _mainpage.manage_navitem;
    }

    private void root_navigationview_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args
    )
    {
        love_and_peace_textblock.Visibility = Visibility.Collapsed;
        var selectedItem = args.SelectedItemContainer;
        if (selectedItem == manage_navitem)
        {
            root_frame.Navigate(typeof(ManagePage));
        }
        else if (selectedItem == library_navitem)
        {
            root_frame.Navigate(typeof(LibraryPage));
        }
        else if (selectedItem == settings_navitem)
        {
            root_frame.Navigate(typeof(SettingsPage));
        }
    }

    private void root_frame_Navigated(object sender, NavigationEventArgs e) { }

    private void root_frame_Navigating(object sender, NavigatingCancelEventArgs e) { }
}
