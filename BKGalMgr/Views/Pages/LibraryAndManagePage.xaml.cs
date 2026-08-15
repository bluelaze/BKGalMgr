using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using BKGalMgr.Interfaces.Page;
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
    private SelectorBarItem _targetSelectedItem;

    public LibraryAndManagePageViewModel ViewModel { get; }

    public LibraryAndManagePage()
    {
        ViewModel = App.GetRequiredService<LibraryAndManagePageViewModel>();
        DataContext = this;
        this.InitializeComponent();
        Loaded += async (s, e) =>
        {
            ShowExtendedContent();
            _ = DispatcherQueue.EnqueueAsync(() =>
            {
                root_SelectorBar.SelectedItem = _targetSelectedItem;
            });
        };
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (root_SelectorBar.SelectedItem == null)
            _targetSelectedItem = library_SelectorBarItem;
    }

    public async void NavigateTo(Type pageType, object parameter = null)
    {
        if (pageType == typeof(LibraryPage))
        {
            root_Frame.Navigate(
                typeof(LibraryPage),
                new IExtendsContentIntoTitleBarPage.PageParameter(this, parameter),
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft }
            );
            // 如果直接赋值，会导致按钮选择样式不生效
            _targetSelectedItem = library_SelectorBarItem;
        }
        else if (pageType == typeof(ManagePage))
        {
            root_Frame.Navigate(
                typeof(ManagePage),
                new IExtendsContentIntoTitleBarPage.PageParameter(this, parameter),
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight }
            );
            _targetSelectedItem = manage_SelectorBarItem;
            if (string.Equals(parameter as string, "fromLibrary"))
            {
                root_SelectorBar.SelectedItem = manage_SelectorBarItem;
            }
        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);

        HideExtendedContent();
        if (root_Frame.Content is IExtendsContentIntoTitleBarPage page)
        {
            page.HideExtendedContent();
        }
    }

    public async void ShowExtendedContent()
    {
        if (IsLoaded)
        {
            _ = DispatcherQueue.EnqueueAsync(() =>
            {
                root_Popup.IsOpen = true;
            });
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
