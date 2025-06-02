using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using BKGalMgr.Helpers;
using BKGalMgr.ThirdParty;

namespace BKGalMgr.ViewModels;

[Serializable]
public partial class TargetInfo : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    [property: JsonIgnore]
    private string _jsonPath;

    [ObservableProperty]
    private string _startupName;

    [ObservableProperty]
    private DateTime _createDate = DateTime.Now;

    [ObservableProperty]
    private DateTime _lastPlayDate;

    [ObservableProperty]
    private TimeSpan _playedTime = TimeSpan.Zero;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private bool _enableLocalEmulator;

    [ObservableProperty]
    [property: JsonIgnore]
    private string _screenCaptureHotkey = string.Empty;

    [ObservableProperty]
    private GameInfo _game;

    [ObservableProperty]
    private SourceInfo _source;

    [ObservableProperty]
    private LocalizationInfo _localization;

    [ObservableProperty]
    [property: JsonIgnore]
    private PlayStatus _playStatus = PlayStatus.Stop;

    partial void OnPlayStatusChanged(PlayStatus value)
    {
        ScreenCaptureActive(value == PlayStatus.Playing || value == PlayStatus.Pause);
    }

    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isArchive = false;

    [property: JsonIgnore]
    public string FolderPath => Path.GetDirectoryName(JsonPath);

    [property: JsonIgnore]
    public string TargetPath => Path.Combine(FolderPath, GlobalInfo.TargetName);

    [property: JsonIgnore]
    public string TargetZipPath => Path.Combine(FolderPath, GlobalInfo.TargetZipName);

    [property: JsonIgnore]
    public string TargetExePath => Path.Combine(TargetPath, StartupName);

    public TargetInfo() { }

    public static TargetInfo Open(string dirPath)
    {
        var path = Path.Combine(dirPath, GlobalInfo.TargetJsonName);
        if (!File.Exists(path))
            return null;

        var targetInfo = JsonMisc.Deserialize<TargetInfo>(File.ReadAllText(path));
        if (targetInfo == null)
            return null;

        targetInfo.JsonPath = path;
        if (!targetInfo.IsValid())
            return null;

        if (!Directory.Exists(targetInfo.TargetPath) && !File.Exists(targetInfo.TargetZipPath))
            return null;

        targetInfo.CheckArchiveStatus();
        return targetInfo;
    }

    public bool IsValid()
    {
        return !Name.IsNullOrEmpty()
            && !JsonPath.IsNullOrEmpty()
            && !StartupName.IsNullOrEmpty()
            && (Source != null || Localization != null);
    }

    public void SetGamePath(string dirPath)
    {
        JsonPath = Path.Combine(
            dirPath,
            GlobalInfo.TargetsFolderName,
            CreateDate.ToString(GlobalInfo.FolderFormatStr),
            GlobalInfo.TargetJsonName
        );
    }

    public void SaveJsonFile()
    {
        string jsonStr = JsonMisc.Serialize(this);
        Directory.CreateDirectory(FolderPath);
        File.WriteAllText(JsonPath, jsonStr);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenJsonFolder()
    {
        Process.Start("explorer", FolderPath);
    }

    public void SeletedSource()
    {
        if (Source is null)
            return;
        Name = Source.Name;
        StartupName = Source.StartupName;
    }

    public void SeletedLocalization()
    {
        if (Localization is null)
            return;
        Name = Localization.Name;
        if (!Localization.StartupName.IsNullOrEmpty())
            StartupName = Localization.StartupName;

        if (Source != null && !Source.Name.IsNullOrEmpty())
            Name = Source.Name + "-" + Name;
    }

    public async Task DecompressSource()
    {
        if (Source == null || !File.Exists(Source.ZipPath))
            return;

        var ret = await FileSystemMisc.ExtractZipFromDirectoryAsync(Source.ZipPath, TargetPath);
        if (!ret.success)
        {
            App.ShowErrorMessage(ret.message);
        }
    }

    public async Task DecompressLocalization()
    {
        if (Localization == null || !File.Exists(Localization.ZipPath))
            return;

        var ret = await FileSystemMisc.ExtractZipFromDirectoryAsync(Localization.ZipPath, TargetPath);
        if (!ret.success)
        {
            App.ShowErrorMessage(ret.message);
        }
    }

    public async Task DecompressSourceAndLocalization()
    {
        // dezip source
        await DecompressSource();

        // dezip target
        await DecompressLocalization();

        SaveJsonFile();
    }

    public async Task CopyShareToTargetFolder(string shareFolderPath)
    {
        // dezip archive target
        var targetZipPath = Path.Combine(shareFolderPath, GlobalInfo.TargetZipName);
        if (Directory.Exists(targetZipPath))
        {
            await FileSystemMisc.ExtractZipFromDirectoryAsync(targetZipPath, TargetPath);
            return;
        }

        // copy target folder
        var targetPath = Path.Combine(shareFolderPath, GlobalInfo.TargetName);
        if (Directory.Exists(targetPath))
        {
            await FileSystemMisc.MoveOrCopyDirectoryAsync(targetPath, TargetPath);
            return;
        }

        // copy current folder as a new target
        await FileSystemMisc.MoveOrCopyDirectoryAsync(shareFolderPath, TargetPath);
    }

    public async Task CopyTargetAsSourceToFolder(string targetFolderPath)
    {
        SourceInfo newSource = new();
        newSource.Name = Name;
        newSource.StartupName = StartupName;
        newSource.Description = Description;
        newSource.JsonPath = Path.Combine(targetFolderPath, GlobalInfo.SourceJsonName);
        if (Source != null && Source.Contributors != null)
            newSource.Contributors = new(newSource.Contributors.Concat(Source.Contributors));
        if (Localization != null && Localization.Contributors != null)
            newSource.Contributors = new(newSource.Contributors.Concat(Localization.Contributors));
        newSource.SaveJsonFile();

        if (IsArchive)
            await FileSystemMisc.CopyFileAsync(TargetZipPath, Path.Combine(targetFolderPath, GlobalInfo.SourceZipName));
        else
            await FileSystemMisc.CreateZipFromDirectoryAsync(
                TargetPath,
                Path.Combine(targetFolderPath, GlobalInfo.SourceZipName),
                App.ZipLevel()
            );
    }

    public bool CheckArchiveStatus()
    {
        if (File.Exists(TargetZipPath))
            IsArchive = true;
        else
            IsArchive = false;
        return IsArchive;
    }

    public async Task Archive()
    {
        await FileSystemMisc.CreateZipFromDirectoryAsync(TargetPath, TargetZipPath, App.ZipLevel());
    }

    public async Task DeArchive()
    {
        await FileSystemMisc.ExtractZipFromDirectoryAsync(TargetZipPath, TargetPath);
    }

    public async Task DeleteFolderOnly()
    {
        CheckArchiveStatus();
        if (!IsArchive)
            return;
        var (success, message) = await FileSystemMisc.DeleteDirectoryAsync(TargetPath);
        if (!success)
        {
            App.ShowErrorMessage(message);
            return;
        }
    }

    public void ScreenCaptureActive(bool active)
    {
        if (active)
        {
            if (!ScreenCaptureHotkey.IsNullOrEmpty())
                return;
            // copy latest capture to clipboard, and save to game capture folder,
            // if launch mutil target, hotkey maybe not same each start.
            ScreenCaptureHotkey = HotkeyHelper.AddOrReplace(
                (e) =>
                {
                    DoScreenCapture();
                    e.Handled = true;
                }
            );
        }
        else
        {
            HotkeyHelper.Remove(ScreenCaptureHotkey);
            ScreenCaptureHotkey = string.Empty;
        }
    }

    public async void DoScreenCapture()
    {
        var capture = ScreenCapture.CaptureRegion();
        if (capture.CaptureBitmap != null)
        {
            Clipboard.SetImage(capture.CaptureBitmap.ToBitmapImage());
            await Game.SaveScreenCapture(this, capture.CaptureBitmap);
            capture.CaptureBitmap.Dispose();
        }
    }
}
