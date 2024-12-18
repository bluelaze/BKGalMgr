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

    public static void NavigateTo(Type pageType, object parameter = null)
    {
        _mainpage.root_frame.Navigate(pageType, parameter);
        if (pageType == typeof(ManagePage))
            _mainpage.root_navigationview.SelectedItem = _mainpage.manage_navitem;
        else if (pageType == typeof(LibraryPage))
            _mainpage.root_navigationview.SelectedItem = _mainpage.library_navitem;
    }

    private void root_navigationview_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args
    )
    {
        love_and_peace_textblock.Visibility = Visibility.Collapsed;
        var selectedItem = args.SelectedItemContainer;
        if (selectedItem == library_navitem)
        {
            if (root_frame.CurrentSourcePageType != typeof(LibraryPage))
                root_frame.Navigate(typeof(LibraryPage));
        }
        else if (selectedItem == manage_navitem)
        {
            if (root_frame.CurrentSourcePageType != typeof(ManagePage))
                root_frame.Navigate(typeof(ManagePage));
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

    private void root_frame_Navigated(object sender, NavigationEventArgs e) { }

    private void root_frame_Navigating(object sender, NavigatingCancelEventArgs e) { }
}
