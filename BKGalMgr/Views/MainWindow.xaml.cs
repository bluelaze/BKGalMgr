using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ShareX.HelpersLib;
using SkiaSharp;
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
    [ObservableProperty]
    private ObservableCollection<string> _images;

    [ObservableProperty]
    private int _imageSelectedIndex = -1;

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

    public void ShowImages(IEnumerable<string> images, int selectedIndex)
    {
        Images = new(images);
        ImageSelectedIndex = selectedIndex;
        image_viewer_Grid.Visibility = Visibility.Visible;
    }

    [RelayCommand]
    public void HideImages()
    {
        image_viewer_Grid.Visibility = Visibility.Collapsed;
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

    private void ImageFitToScreen(ScrollViewer scrollerViwer, Image imagePost)
    {
        if (scrollerViwer.ActualWidth == 0 || scrollerViwer.ActualHeight == 0)
            return;
        if (imagePost.ActualWidth == 0 || imagePost.ActualHeight == 0)
            return;

        double widthCompare = scrollerViwer.ActualWidth / imagePost.ActualWidth;
        double heightCompare = scrollerViwer.ActualHeight / imagePost.ActualHeight;

        double zoomFactor = 1.0f;
        double v = 0.001f;
        if (widthCompare > 1 && heightCompare > 1)
        {
            zoomFactor = 1.0f;
        }
        else if (widthCompare > heightCompare)
        {
            while (imagePost.ActualHeight * heightCompare >= scrollerViwer.ActualHeight)
                heightCompare -= v;

            zoomFactor = heightCompare;
        }
        else
        {
            while (imagePost.ActualWidth * widthCompare >= scrollerViwer.ActualWidth)
                widthCompare -= v;

            zoomFactor = widthCompare;
        }

        scrollerViwer.ChangeView(0, 0, (float)zoomFactor);
    }

    private void image_ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var scrollerViwer = sender as ScrollViewer;
        ImageFitToScreen(sender as ScrollViewer, scrollerViwer.FindName("post_Image") as Image);
    }

    private void post_Image_ImageOpened(object sender, RoutedEventArgs e)
    {
        var image = sender as Image;
        ImageFitToScreen(image.FindAscendant("image_ScrollViewer") as ScrollViewer, image);
    }

    private void image_viewer_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listView = sender as ListView;
        listView.ScrollIntoView(listView.SelectedItem);
    }
}
