using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Controls;
using Windows.Devices.Lights;
using Windows.Graphics.Imaging;

namespace BKGalMgr.ViewModels;

public record PlayedPeriod(DateTime benginTime, DateTime endTime);

[Serializable]
public partial class GameInfo : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private string _name = "";

    [ObservableProperty]
    [property: JsonIgnore]
    private string _jsonPath;

    [ObservableProperty]
    private DateTime _createDate = DateTime.Now;

    [ObservableProperty]
    private DateTime _lastPlayDate;

    [ObservableProperty]
    private TimeSpan _playedTime = TimeSpan.Zero;

    [ObservableProperty]
    private List<PlayedPeriod> _playedPeriods = new();

    [ObservableProperty]
    private string _cover;

    [ObservableProperty]
    private string _company;

    [ObservableProperty]
    private DateTime _publishDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ArtistItems))]
    private ObservableCollection<string> _artist = new();

    [ObservableProperty]
    private ObservableCollection<string> _cv = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScenarioItems))]
    private ObservableCollection<string> _scenario = new();

    [ObservableProperty]
    private ObservableCollection<string> _musician = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TagItems))]
    private ObservableCollection<string> _tag = new();

    [ObservableProperty]
    private string _website;

    [ObservableProperty]
    private string _story;

    [ObservableProperty]
    private string _blog;

    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isPropertyChanged;

    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isPlaying = false;

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<SourceInfo> _sources = new();

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<LocalizationInfo> _localizations = new();

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<TargetInfo> _targets = new();

    private TargetInfo _selectedTarget;

    [property: JsonIgnore]
    public TargetInfo SelectedTarget
    {
        get { return _selectedTarget; }
        set
        {
            if (value == null && Targets.Count() > 0)
                return;

            if (SetProperty(ref _selectedTarget, value))
            {
                SeletedTargetCreateDate = _selectedTarget?.CreateDate ?? new();
                SaveJsonFile();
            }
        }
    }
    public DateTime SeletedTargetCreateDate { get; set; }

    [property: JsonIgnore]
    public ObservableCollection<MetadataItem> ArtistItems =>
        new(Artist.Select(art => new MetadataItem() { Label = art }));

    [property: JsonIgnore]
    public ObservableCollection<MetadataItem> ScenarioItems =>
        new(Scenario.Select(scenario => new MetadataItem() { Label = scenario }));

    [property: JsonIgnore]
    public ObservableCollection<MetadataItem> TagItems => new(Tag.Select(tag => new MetadataItem() { Label = tag }));

    [property: JsonIgnore]
    public string FolderPath => Path.GetDirectoryName(JsonPath);

    [property: JsonIgnore]
    public string ScreenCaptureFolderPath => Path.Combine(FolderPath, GlobalInfo.GameScreenCaptureFolderName);

    public GameInfo() { }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName != nameof(IsPropertyChanged) && !IsPlaying)
            IsPropertyChanged = true;
    }

    public bool IsValid
    {
        get { return !Name.IsNullOrEmpty(); }
    }

    public void SetRepositoryPath(string dirPath)
    {
        JsonPath = Path.Combine(dirPath, CreateDate.ToString(GlobalInfo.FolderFormatStr), GlobalInfo.GameJsonName);
    }

    public List<string> GetAllTags()
    {
        List<string> tags = [Name, Company];
        tags = tags.Union(Artist).Union(Cv).Union(Scenario).Union(Musician).Union(tags).ToList();
        tags = tags.Union(Sources.Select(item => item.Name))
            .Union(Localizations.Select(item => item.Name))
            .Union(Targets.Select(item => item.Name))
            .ToList();
        return tags;
    }

    public static GameInfo Open(string dirPath)
    {
        var path = Path.Combine(dirPath, GlobalInfo.GameJsonName);
        if (!File.Exists(path))
            return null;

        var gameInfo = JsonMisc.Deserialize<GameInfo>(File.ReadAllText(path));
        gameInfo.JsonPath = path;

        path = Path.Combine(dirPath, GlobalInfo.SourcesFolderName);
        if (Directory.Exists(path))
        {
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                var source = SourceInfo.Open(dir);
                if (source != null)
                    gameInfo.Sources.Add(source);
            }
        }

        path = Path.Combine(dirPath, GlobalInfo.LocalizationsFolderName);
        if (Directory.Exists(path))
        {
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                var localization = LocalizationInfo.Open(dir);
                if (localization != null)
                    gameInfo.Localizations.Add(localization);
            }
        }

        path = Path.Combine(dirPath, GlobalInfo.TargetsFolderName);
        if (Directory.Exists(path))
        {
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                var target = TargetInfo.Open(dir);
                if (target != null)
                {
                    gameInfo.Targets.Add(target);
                    if (target.CreateDate == gameInfo.SeletedTargetCreateDate)
                        gameInfo._selectedTarget = target;
                }
            }
        }

        gameInfo.LoadCover();
        gameInfo.IsPropertyChanged = false;
        return gameInfo;
    }

    public SourceInfo NewSource()
    {
        var sourceInfo = new SourceInfo();
        sourceInfo.SetGamePath(FolderPath);

        return sourceInfo;
    }

    public async Task AddSource(string sourceFolderPath, SourceInfo sourceInfo)
    {
        if (sourceFolderPath.IsNullOrEmpty() || !sourceInfo.IsValid())
            return;
        await sourceInfo.CompressSourceFolder(sourceFolderPath);
        Sources.Add(sourceInfo);
    }

    public void UpdateSource(SourceInfo sourceInfo)
    {
        for (int i = 0; i < Sources.Count; i++)
        {
            if (Sources[i].CreateDate == sourceInfo.CreateDate)
            {
                Sources[i] = sourceInfo;
            }
        }
    }

    public async Task<bool> CopySource(string dirPath)
    {
        do
        {
            SourceInfo copySource = SourceInfo.Open(dirPath);
            if (copySource == null)
                break;
            if (!File.Exists(copySource.ZipPath))
                break;
            // copy to game
            var newSource = NewSource();

            await copySource.CopySourceToFolder(Path.GetDirectoryName(newSource.JsonPath));

            copySource.CreateDate = newSource.CreateDate;
            copySource.SetGamePath(FolderPath);
            copySource.SaveJsonFile();

            Sources.Add(copySource);

            return true;
        } while (false);
        return false;
    }

    public async Task DeleteSource(SourceInfo source)
    {
        if (Sources.Contains(source))
        {
            if (Directory.Exists(source.FolderPath))
                await Task.Run(() =>
                {
                    Directory.Delete(source.FolderPath, true);
                });
            Sources.Remove(source);
        }
    }

    public LocalizationInfo NewLocalization()
    {
        var localizationInfo = new LocalizationInfo();
        localizationInfo.SetGamePath(FolderPath);

        return localizationInfo;
    }

    public async Task AddLocalization(string sourceFolderPath, LocalizationInfo localizationInfo)
    {
        if (sourceFolderPath.IsNullOrEmpty() || !localizationInfo.IsValid())
            return;
        await localizationInfo.CompressLocalizationFolder(sourceFolderPath);
        Localizations.Add(localizationInfo);
    }

    public void UpdateLocalization(LocalizationInfo localizationInfo)
    {
        for (int i = 0; i < Localizations.Count; i++)
        {
            if (Localizations[i].CreateDate == localizationInfo.CreateDate)
            {
                Localizations[i] = localizationInfo;
            }
        }
    }

    public SourceInfo FindSource(SourceInfo sourceInfo)
    {
        if (sourceInfo == null)
            return null;

        for (int i = 0; i < Sources.Count; i++)
        {
            if (Sources[i].CreateDate == sourceInfo.CreateDate)
            {
                return Sources[i];
            }
        }
        return null;
    }

    public LocalizationInfo FindLocalization(LocalizationInfo localizationInfo)
    {
        if (localizationInfo == null)
            return null;

        for (int i = 0; i < Localizations.Count; i++)
        {
            if (Localizations[i].CreateDate == localizationInfo.CreateDate)
            {
                return Localizations[i];
            }
        }
        return null;
    }

    public async Task<bool> CopyLocalization(string dirPath)
    {
        do
        {
            LocalizationInfo copyLocalization = LocalizationInfo.Open(dirPath);
            if (copyLocalization == null)
                break;
            if (!File.Exists(copyLocalization.ZipPath))
                break;
            // copy to game
            var newLocalization = NewLocalization();

            await copyLocalization.CopyLocalizationToFolder(Path.GetDirectoryName(newLocalization.JsonPath));

            copyLocalization.CreateDate = newLocalization.CreateDate;
            copyLocalization.SetGamePath(FolderPath);
            copyLocalization.SaveJsonFile();

            Localizations.Add(copyLocalization);

            return true;
        } while (false);
        return false;
    }

    public async Task DeleteLocalization(LocalizationInfo localization)
    {
        if (Localizations.Contains(localization))
        {
            if (Directory.Exists(localization.FolderPath))
                await Task.Run(() =>
                {
                    Directory.Delete(localization.FolderPath, true);
                });
            Localizations.Remove(localization);
        }
    }

    public TargetInfo NewTarget()
    {
        var targetInfo = new TargetInfo() { Game = this };
        targetInfo.SetGamePath(FolderPath);

        return targetInfo;
    }

    public async Task AddTarget(TargetInfo targetInfo)
    {
        if (!targetInfo.IsValid())
            return;
        await targetInfo.DecompressSourceAndLocalization();
        Targets.Add(targetInfo);
    }

    public async Task<bool> CopyTarget(string shareFolderPath, TargetInfo targetInfo)
    {
        do
        {
            if (!targetInfo.IsValid())
                break;
            if (!Directory.Exists(shareFolderPath))
                break;
            // copy to game

            await targetInfo.CopyShareToTargetFolder(shareFolderPath);
            targetInfo.SaveJsonFile();

            Targets.Add(targetInfo);

            return true;
        } while (false);
        return false;
    }

    public void UpdateTarget(TargetInfo targetInfo)
    {
        for (int i = 0; i < Targets.Count; i++)
        {
            if (Targets[i].CreateDate == targetInfo.CreateDate)
            {
                Targets[i] = targetInfo;
            }
        }
    }

    public async Task DeleteTarget(TargetInfo targetInfo)
    {
        if (Targets.Contains(targetInfo))
        {
            if (Directory.Exists(targetInfo.FolderPath))
                await Task.Run(() =>
                {
                    Directory.Delete(targetInfo.FolderPath, true);
                });
            Targets.Remove(targetInfo);
        }
        if (targetInfo == SelectedTarget)
        {
            _selectedTarget = null;
            OnPropertyChanged(nameof(SelectedTarget));
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    public async Task SaveGameInfo()
    {
        SaveJsonFile();
        await SaveCover();
        IsPropertyChanged = false;
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void SaveJsonFile()
    {
        if (JsonPath.IsNullOrEmpty())
            return;

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

    public void LoadCover()
    {
        foreach (var format in GlobalInfo.GameCoverSupportFormats)
        {
            var coverPath = Path.Combine(FolderPath, GlobalInfo.GameCoverName + format);
            if (File.Exists(coverPath))
            {
                Cover = coverPath;
                break;
            }
        }
    }

    public string TransformCoverPath(string path)
    {
        var format = Path.GetExtension(path).ToLower();
        if (GlobalInfo.GameCoverSupportFormats.Contains(format))
            return Path.Combine(FolderPath, GlobalInfo.GameCoverName + format);
        return string.Empty;
    }

    public async Task SaveCover()
    {
        if (!Cover.IsNullOrEmpty())
        {
            var coverPath = TransformCoverPath(Cover);
            if (coverPath.IsNullOrEmpty())
                return;

            if (File.Exists(Cover) && Path.GetDirectoryName(Cover) != FolderPath)
            {
                File.Copy(Cover, coverPath, true);
                Cover = coverPath;
                return;
            }

            if (!Cover.StartsWith("http"))
                return;
            try
            {
                var fileData = await (new HttpClient()).GetByteArrayAsync(Cover);
                await File.WriteAllBytesAsync(coverPath, fileData);
                Cover = coverPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return;
            }
        }
    }

    public void SaveScreenCapture(TargetInfo targetInfo, Bitmap bitmap)
    {
        Directory.CreateDirectory(ScreenCaptureFolderPath);
        bitmap.Save(
            Path.Combine(
                ScreenCaptureFolderPath,
                $"{targetInfo.Name.ValidFileName("_")}_{DateTime.Now.ToString(GlobalInfo.GameScreenCaptureFileFormatStr)}.png"
            )
        );
    }
}
