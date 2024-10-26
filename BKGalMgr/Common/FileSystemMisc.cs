using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace BKGalMgr.Common;

internal class FileSystemMisc
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

    public static bool ArePathsOnSameDrive(string path1, string path2)
    {
        return string.Equals(Path.GetPathRoot(path1), Path.GetPathRoot(path2), StringComparison.OrdinalIgnoreCase);
    }

    // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }

    public static void DirectoryMoveOrCopy(string sourceDirName, string destDirName)
    {
        if (ArePathsOnSameDrive(sourceDirName, destDirName))
            Directory.Move(sourceDirName, destDirName);
        else
            CopyDirectory(sourceDirName, destDirName);
    }
}
