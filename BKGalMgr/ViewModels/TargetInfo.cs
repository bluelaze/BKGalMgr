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
    private bool _enableScreenCapture;

    partial void OnEnableScreenCaptureChanged(bool value)
    {
        if (IsPlaying)
        {
            ScreenCaptureActive(value);
        }
    }

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
    private bool _isPlaying = false;

    partial void OnIsPlayingChanged(bool value)
    {
        if (EnableScreenCapture)
        {
            ScreenCaptureActive(value);
        }
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
    }

    public async Task DecompressSource()
    {
        if (Source == null || !File.Exists(Source.ZipPath))
            return;

        await Task.Run(() =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(TargetPath));
            ZipFile.ExtractToDirectory(Source.ZipPath, TargetPath, true);
        });
    }

    public async Task DecompressLocalization()
    {
        if (Localization == null || !File.Exists(Localization.ZipPath))
            return;

        await Task.Run(() =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(TargetPath));
            ZipFile.ExtractToDirectory(Localization.ZipPath, TargetPath, true);
        });
    }

    public async Task DecompressSourceAndLocalization()
    {
        await Task.Run(async () =>
        {
            // dezip source
            await DecompressSource();

            // dezip target
            await DecompressLocalization();

            SaveJsonFile();
        });
    }

    public async Task CopyShareToTargetFolder(string shareFolderPath)
    {
        await Task.Run(() =>
        {
            Directory.CreateDirectory(TargetPath);

            // dezip archive target
            var targetZipPath = Path.Combine(shareFolderPath, GlobalInfo.TargetZipName);
            if (Directory.Exists(targetZipPath))
            {
                ZipFile.ExtractToDirectory(targetZipPath, TargetPath, true);
                return;
            }

            // move need delete target folder
            if (Directory.Exists(TargetPath))
                Directory.Delete(TargetPath, true);

            // copy target folder
            var targetPath = Path.Combine(shareFolderPath, GlobalInfo.TargetName);
            if (Directory.Exists(targetPath))
            {
                Directory.Move(targetPath, TargetPath);
                return;
            }

            // copy current folder as a new target
            Directory.Move(shareFolderPath, TargetPath);
        });
    }

    public async Task CopyTargetAsSourceToFolder(string targetFolderPath)
    {
        await Task.Run(() =>
        {
            Directory.CreateDirectory(targetFolderPath);

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
                File.Copy(TargetZipPath, Path.Combine(targetFolderPath, GlobalInfo.SourceZipName));
            else
                ZipFile.CreateFromDirectory(
                    TargetPath,
                    Path.Combine(targetFolderPath, GlobalInfo.SourceZipName),
                    App.ZipLevel(),
                    false
                );
        });
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
        await Task.Run(() =>
        {
            if (File.Exists(TargetZipPath))
                File.Delete(TargetZipPath);
            ZipFile.CreateFromDirectory(TargetPath, TargetZipPath, App.ZipLevel(), false);
        });
    }

    public async Task DeArchive()
    {
        await Task.Run(() =>
        {
            if (!Directory.Exists(TargetPath))
                Directory.CreateDirectory(TargetPath);
            ZipFile.ExtractToDirectory(TargetZipPath, TargetPath);
        });
    }

    public async Task DeleteFolderOnly()
    {
        if (!IsArchive)
            return;
        await Task.Run(() =>
        {
            Directory.Delete(TargetPath, true);
        });
    }

    public void ScreenCaptureActive(bool active)
    {
        if (active)
        {
            // copy lastest capture to clipboard, and save to game capture folder,
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

    public void DoScreenCapture()
    {
        var capture = ScreenCapture.CaptureRegion();
        if (capture.captureBmp != null)
        {
            Clipboard.SetImage(capture.captureBmp.ToBitmapImage());
            Game.SaveScreenCapture(this, capture.captureBmp);
            capture.captureBmp.Dispose();
        }
    }
}
