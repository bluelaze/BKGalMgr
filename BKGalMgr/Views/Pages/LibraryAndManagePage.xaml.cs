using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
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
public sealed partial class LibraryAndManagePage : Page, IExtendsContentIntoTitleBarPage
{
    public LibraryAndManagePageViewModel ViewModel { get; }

    public LibraryAndManagePage()
    {
        ViewModel = App.GetRequiredService<LibraryAndManagePageViewModel>();
        DataContext = this;
        this.InitializeComponent();
        Loaded += (s, e) =>
        {
            ShowExtendedContent();
        };
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (root_SelectorBar.SelectedItem == null)
            root_SelectorBar.SelectedItem = library_SelectorBarItem;
    }

    public async void NavigateTo(Type pageType)
    {
        if (pageType == typeof(LibraryPage))
        {
            root_Frame.Navigate(
                typeof(LibraryPage),
                this,
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft }
            );
            // 需要延迟，如果直接赋值，会导致按钮选择样式不生效
            await Task.Delay(67);
            root_SelectorBar.SelectedItem = library_SelectorBarItem;
        }
        else if (pageType == typeof(ManagePage))
        {
            root_Frame.Navigate(
                typeof(ManagePage),
                this,
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight }
            );
            await Task.Delay(67);
            root_SelectorBar.SelectedItem = manage_SelectorBarItem;
        }
    }

    public async void ShowExtendedContent()
    {
        if (IsLoaded)
        {
            await Task.Delay(33);
            root_Popup.IsOpen = true;
        }
    }

    public void HideExtendedContent()
    {
        root_Popup.IsOpen = false;
    }

    private void root_SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        var selectedItem = sender.SelectedItem;
        if (selectedItem == library_SelectorBarItem)
        {
            if (root_Frame.CurrentSourcePageType != typeof(LibraryPage))
                NavigateTo(typeof(LibraryPage));
        }
        else if (selectedItem == manage_SelectorBarItem)
        {
            if (root_Frame.CurrentSourcePageType != typeof(ManagePage))
                NavigateTo(typeof(ManagePage));
        }
    }
}
