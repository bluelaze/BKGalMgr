using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BKGalMgr.Views.Pages;
using H.NotifyIcon;
using Microsoft.UI.Windowing;
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

namespace BKGalMgr.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
[ObservableObject]
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();

        //https://learn.microsoft.com/en-us/windows/apps/develop/title-bar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(app_titlebar_grid);
        AppWindow.Changed += AppWindow_Changed;
        AppWindow.Closing += AppWindow_Closing;

        main_root_frame.Navigate(typeof(MainPage));
    }

    public void ShowLoading()
    {
        loading_contentpresenter.Visibility = Visibility.Visible;
    }

    public void HideLoading()
    {
        loading_contentpresenter.Visibility = Visibility.Collapsed;
    }

    [RelayCommand]
    public void ShowWindow()
    {
        (AppWindow.Presenter as OverlappedPresenter).IsAlwaysOnTop = true;
        this.Show();
        Activate();
        (AppWindow.Presenter as OverlappedPresenter).IsAlwaysOnTop = false;
    }

    private bool _exit = false;

    [RelayCommand]
    public void Exit()
    {
        _exit = true;
        taskbaricon.Dispose();
        Close();
    }

    private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (args.DidPresenterChange)
        {
            switch (sender.Presenter.Kind)
            {
                case AppWindowPresenterKind.CompactOverlay:
                    // Compact overlay - hide custom title bar
                    // and use the default system title bar instead.
                    app_titlebar_grid.Visibility = Visibility.Collapsed;
                    sender.TitleBar.ResetToDefault();
                    break;

                case AppWindowPresenterKind.FullScreen:
                    // Full screen - hide the custom title bar
                    // and the default system title bar.
                    app_titlebar_grid.Visibility = Visibility.Collapsed;
                    sender.TitleBar.ExtendsContentIntoTitleBar = true;
                    break;

                case AppWindowPresenterKind.Overlapped:
                    // Normal - hide the system title bar
                    // and use the custom title bar instead.
                    app_titlebar_grid.Visibility = Visibility.Visible;
                    sender.TitleBar.ExtendsContentIntoTitleBar = true;
                    break;

                default:
                    // Use the default system title bar.
                    sender.TitleBar.ResetToDefault();
                    break;
            }
        }
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (!_exit)
        {
            args.Cancel = true;
            this.Hide();
        }
    }

    private void main_root_frame_Navigated(object sender, NavigationEventArgs e) { }

    private void main_root_frame_Navigating(object sender, NavigatingCancelEventArgs e) { }
}
