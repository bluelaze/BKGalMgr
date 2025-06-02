using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

    public static (bool success, string message) CopyFile(
        string sourceFileName,
        string destFileName,
        bool overwrite = true
    )
    {
        try
        {
            // Directory.CreateDirectory 是一个相当灵活的方法。如果目标文件夹不存在，它会自动创建；
            // 如果目标文件夹已经存在，它会忽略这个操作。同时，它还会沿途创建所有不存在的文件夹（类似 mkdir 的 -p 参数）
            var destParentFolderPath = Path.GetDirectoryName(destFileName);
            if (destParentFolderPath != null)
                Directory.CreateDirectory(destParentFolderPath);
            File.Copy(sourceFileName, destFileName, overwrite);
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }
        return (true, "");
    }

    public static async Task<(bool success, string message)> CopyFileAsync(
        string sourceFileName,
        string destFileName,
        bool overwrite = true
    )
    {
        return await Task.Run(() => CopyFile(sourceFileName, destFileName, overwrite));
    }

    // https://blog.coldwind.top/posts/how-to-copy-folder/
    public static (bool success, string message) CopyDirectory(string sourceFolderPath, string targetFolderPath)
    {
        if (!Directory.Exists(sourceFolderPath))
            return (false, $"{sourceFolderPath} is invalid");
        try
        {
            Directory.CreateDirectory(targetFolderPath);

            foreach (string filePath in Directory.GetFiles(sourceFolderPath, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceFolderPath, filePath);
                var targetFilePath = Path.Combine(targetFolderPath, relativePath);
                // Path.GetDirectoryName 方法有可能返回空。这一情况通常发生在文件位于根目录的情况（例如 Windows 的 C:\，或 Unix 的 /）。
                var destParentFolderPath = Path.GetDirectoryName(targetFilePath);
                if (destParentFolderPath != null)
                    Directory.CreateDirectory(destParentFolderPath);
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
        if (!Directory.Exists(sourceDirName))
            return (false, $"{sourceDirName} is invalid");
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
                {
                    Directory.Delete(destDirName, true);
                }
                else
                {
                    var destParentFolderPath = Path.GetDirectoryName(destDirName);
                    if (destParentFolderPath != null)
                        Directory.CreateDirectory(destParentFolderPath);
                }
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

    public static async Task<long> GetDirectorySizeAsync(string path, bool recurse = true)
    {
        if (!Directory.Exists(path))
            return 0;
        return await Task.Run((() => GetDirectorySize(new DirectoryInfo(path), recurse)));
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
            DirectoryInfo[] diEntries = di.GetDirectories("*", SearchOption.TopDirectoryOnly);
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

    public static async Task<(bool success, string message)> CreateZipFromDirectoryAsync(
        string sourceDirectoryName,
        string destinationArchiveFileName,
        CompressionLevel compressionLevel,
        bool includeBaseDirectory = false
    )
    {
        return await Task.Run(() =>
        {
            try
            {
                if (File.Exists(destinationArchiveFileName))
                {
                    File.Delete(destinationArchiveFileName);
                }
                else
                {
                    var destParentFolderPath = Path.GetDirectoryName(destinationArchiveFileName);
                    if (destParentFolderPath != null)
                        Directory.CreateDirectory(destParentFolderPath);
                }

                ZipFile.CreateFromDirectory(
                    sourceDirectoryName,
                    destinationArchiveFileName,
                    compressionLevel,
                    includeBaseDirectory
                );
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
            return (true, "");
        });
    }

    public static async Task<(bool success, string message)> ExtractZipFromDirectoryAsync(
        string sourceArchiveFileName,
        string destinationDirectoryName,
        bool overwriteFiles = true
    )
    {
        return await Task.Run(() =>
        {
            try
            {
                Directory.CreateDirectory(destinationDirectoryName);
                ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, overwriteFiles);
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
            return (true, "");
        });
    }
}
