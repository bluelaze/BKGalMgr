using BKGalMgr.ViewModels;
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

namespace BKGalMgr.Views.Controls;

public sealed partial class SourceInfoControl : UserControl
{
    public string FolderPath
    {
        get { return (string)GetValue(FolderPathProperty); }
        set { SetValue(FolderPathProperty, value); }
    }
    public static readonly DependencyProperty FolderPathProperty = DependencyProperty.Register("FolderPath", typeof(string), typeof(SourceInfoControl), new PropertyMetadata(default(string)));

    public bool FolderPathVisible
    {
        get { return (bool)GetValue(FolderPathVisibleProperty); }
        set { SetValue(FolderPathVisibleProperty, value); }
    }
    public static readonly DependencyProperty FolderPathVisibleProperty = DependencyProperty.Register("FolderPathVisible", typeof(bool), typeof(SourceInfoControl), new PropertyMetadata(true));

    public SourceInfoControl()
    {
        this.InitializeComponent();
    }

    public bool IsValidSource()
    {
        var source = this.DataContext as SourceInfo;
        return (!FolderPathVisible || !FolderPath.IsNullOrEmpty()) && source != null && source.IsValid();
    }

    private async void button_pick_folder_Click(object sender, RoutedEventArgs e)
    {
        var folderPicker = new Windows.Storage.Pickers.FolderPicker();
        folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
        folderPicker.FileTypeFilter.Add("*");

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

        Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            headeredtextbox_pick_folder.Text = folder.Path;
        }
    }

    private async void button_pick_startup_name_Click(object sender, RoutedEventArgs e)
    {
        var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
        filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
        filePicker.FileTypeFilter.Add(".exe");

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hWnd);

        Windows.Storage.StorageFile file = await filePicker.PickSingleFileAsync();
        if (file != null)
        {
            headeredtextbox_pick_startup_name.Text = file.Name;
        }
    }

    private void button_add_contributor_Click(object sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as SourceInfo;
        if (vm != null)
        {
            vm.Contributors.Add(new());
        }
    }

    private void ContributorInfoControl_Delete(object sender, RoutedEventArgs e)
    {
        var ele = sender as ContributorInfoControl;
        var ctbtor = ele.DataContext as ContributorInfo;
        if (ctbtor != null)
        {
            (this.DataContext as SourceInfo).Contributors.Remove(ctbtor);
        }
    }
}
