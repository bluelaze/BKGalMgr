using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
public sealed partial class LibraryAndManagePage : Page
{
    public LibraryAndManagePageViewModel ViewModel { get; }

    public LibraryAndManagePage()
    {
        ViewModel = App.GetRequiredService<LibraryAndManagePageViewModel>();
        DataContext = this;
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Type pageType = e.Parameter as Type;
        if (pageType == typeof(LibraryPage))
            root_SelectorBar.SelectedItem = library_SelectorBarItem;
        else if (pageType == typeof(ManagePage))
            root_SelectorBar.SelectedItem = manage_SelectorBarItem;
        else if (root_SelectorBar.SelectedItem == null)
            root_SelectorBar.SelectedItem = library_SelectorBarItem;
    }

    public void NaviagteToManagePage()
    {
        root_SelectorBar.SelectedItem = manage_SelectorBarItem;
    }

    private void root_SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        var selectedItem = sender.SelectedItem;
        if (selectedItem == library_SelectorBarItem)
        {
            if (root_Frame.CurrentSourcePageType != typeof(LibraryPage))
                root_Frame.Navigate(typeof(LibraryPage));
        }
        else if (selectedItem == manage_SelectorBarItem)
        {
            if (root_Frame.CurrentSourcePageType != typeof(ManagePage))
                root_Frame.Navigate(typeof(ManagePage));
        }
    }
}
