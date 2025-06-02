using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace BKGalMgr.Common;

public class FileSystemMisc
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

    // https://blog.coldwind.top/posts/how-to-copy-folder/
    public static (bool success, string message) CopyDirectory(string sourceFolderPath, string targetFolderPath)
    {
        try
        {
            Directory.CreateDirectory(targetFolderPath);

            foreach (string filePath in Directory.GetFiles(sourceFolderPath, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceFolderPath, filePath);
                var targetFilePath = Path.Combine(targetFolderPath, relativePath);
                var subTargetFolderPath = Path.GetDirectoryName(targetFilePath);
                // Path.GetDirectoryName 方法有可能返回空。这一情况通常发生在文件位于根目录的情况（例如 Windows 的 C:\，或 Unix 的 /）。
                if (subTargetFolderPath != null)
                    Directory.CreateDirectory(subTargetFolderPath);
                File.Copy(filePath, targetFilePath, true);
            }
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }
        return (true, "");
    }

    public static (bool success, string message) MoveOrCopyDirectory(string sourceDirName, string destDirName)
    {
        // move
        try
        {
            if (
                string.Equals(
                    Path.GetPathRoot(sourceDirName),
                    Path.GetPathRoot(destDirName),
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                // move need delete target folder
                if (Directory.Exists(destDirName))
                    Directory.Delete(destDirName, true);
                Directory.Move(sourceDirName, destDirName);
                return (true, "");
            }
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }

        // copy
        return CopyDirectory(sourceDirName, destDirName);
    }

    public static async Task<(bool success, string message)> MoveOrCopyDirectoryAsync(
        string sourceDirName,
        string destDirName
    )
    {
        return await Task.Run(() => MoveOrCopyDirectory(sourceDirName, destDirName));
    }

    // https://stackoverflow.com/questions/2979432/directory-file-size-calculation-how-to-make-it-faster
    public static long GetDirectorySize(string sourceDir, bool recurse = true)
    {
        return GetDirectorySize(new DirectoryInfo(sourceDir), recurse);
    }

    private static long GetDirectorySize(DirectoryInfo di, bool recurse = true)
    {
        long size = 0;
        foreach (var fiEntry in di.GetFiles())
        {
            Interlocked.Add(ref size, fiEntry.Length);
        }

        if (recurse)
        {
            DirectoryInfo[] diEntries = di.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
            Parallel.For<long>(
                0,
                diEntries.Length,
                () => 0,
                (i, loop, subtotal) =>
                {
                    if (
                        (diEntries[i].Attributes & System.IO.FileAttributes.ReparsePoint)
                        == System.IO.FileAttributes.ReparsePoint
                    )
                        return 0;
                    subtotal += GetDirectorySize(diEntries[i], true);
                    return subtotal;
                },
                (x) => Interlocked.Add(ref size, x)
            );
        }
        return size;
    }

    public static List<string> GetDirectoryFiles(string path)
    {
        if (path.IsNullOrEmpty() || !Directory.Exists(path))
            return null;
        return Directory.GetFiles(path).ToList().SortByName();
    }

    public static (bool success, string message) DeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
            return (true, "");
        try
        {
            Directory.Delete(path, true);
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }
        return (true, "");
    }

    public static async Task<(bool success, string message)> DeleteDirectoryAsync(string path)
    {
        if (!Directory.Exists(path))
            return (true, "");

        return await Task.Run(() => DeleteDirectory(path));
    }
}
