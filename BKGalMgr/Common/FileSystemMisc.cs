using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace BKGalMgr.Common;

class FileSystemMisc
{
    public static async Task<StorageFile> PickFile(List<string> fileTypeFilter)
    {
        var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
        filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

        foreach (var filter in fileTypeFilter)
            filePicker.FileTypeFilter.Add(filter);

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hWnd);

        return await filePicker.PickSingleFileAsync();
    }

    public static async Task<StorageFolder> PickFolder(List<string> fileTypeFilter)
    {
        var folderPicker = new Windows.Storage.Pickers.FolderPicker();
        folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

        foreach (var filter in fileTypeFilter)
            folderPicker.FileTypeFilter.Add(filter);

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

        return await folderPicker.PickSingleFolderAsync();
    }
}
