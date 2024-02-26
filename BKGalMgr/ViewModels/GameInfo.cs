using CommunityToolkit.WinUI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Devices.Lights;

namespace BKGalMgr.ViewModels;

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
            if (SetProperty(ref _selectedTarget, value))
            {
                SeletedTargetCreateDate = _selectedTarget.CreateDate;
                SaveJsonFile();
            }
        }
    }
    public DateTime SeletedTargetCreateDate { get; set; }

    [property: JsonIgnore]
    public ObservableCollection<MetadataItem> ArtistItems => new(Artist.Select(art => new MetadataItem() { Label = art }));
    [property: JsonIgnore]
    public ObservableCollection<MetadataItem> ScenarioItems => new(Scenario.Select(scenario => new MetadataItem() { Label = scenario }));
    [property: JsonIgnore]
    public ObservableCollection<MetadataItem> TagItems => new(Tag.Select(tag => new MetadataItem() { Label = tag }));

    public GameInfo() { }

    public bool IsValid { get { return !Name.IsNullOrEmpty(); } }

    public static GameInfo Open(string dirPath)
    {
        var path = Path.Combine(dirPath, GlobalInfo.GameJsonName);
        if (!File.Exists(path))
            return null;

        var gameInfo = JsonSerializer.Deserialize<GameInfo>(File.ReadAllBytes(path));
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

        return gameInfo;
    }

    public SourceInfo NewSource()
    {
        var sourceInfo = new SourceInfo();
        sourceInfo.SetGamePath(Path.GetDirectoryName(JsonPath));

        return sourceInfo;
    }
    public async Task AddSource(string sourceFolderPath, SourceInfo sourceInfo)
    {
        if (sourceFolderPath.IsNullOrEmpty() || sourceInfo.Name.IsNullOrEmpty() || sourceInfo.StartupName.IsNullOrEmpty())
            return;
        await sourceInfo.CompressSourceFolder(sourceFolderPath);
        Sources.Add(sourceInfo);
    }

    public LocalizationInfo NewLocalization()
    {
        var localizationInfo = new LocalizationInfo();
        localizationInfo.SetGamePath(Path.GetDirectoryName(JsonPath));

        return localizationInfo;
    }
    public async Task AddLocalization(string sourceFolderPath, LocalizationInfo localizationInfo)
    {
        if (sourceFolderPath.IsNullOrEmpty() || localizationInfo.Name.IsNullOrEmpty() || localizationInfo.StartupName.IsNullOrEmpty())
            return;
        await localizationInfo.CompressLocalizationFolder(sourceFolderPath);
        Localizations.Add(localizationInfo);
    }

    public TargetInfo NewTarget()
    {
        var targetInfo = new TargetInfo() { Game = this };
        targetInfo.SetGamePath(Path.GetDirectoryName(JsonPath));

        return targetInfo;
    }
    public async Task AddTarget(TargetInfo targetInfo)
    {
        if (targetInfo.Name.IsNullOrEmpty() || targetInfo.StartupName.IsNullOrEmpty())
            return;
        await targetInfo.DecompressSourceAndLocalization();
        Targets.Add(targetInfo);
    }

    public void SetRepositoryPath(string dirPath)
    {
        JsonPath = Path.Combine(dirPath, CreateDate.ToString(GlobalInfo.FolderFormatStr), GlobalInfo.GameJsonName);
    }

    [RelayCommand]
    public void SaveJsonFile()
    {
        if (JsonPath.IsNullOrEmpty()) return;

        string jsonStr = JsonMisc.Serialize(this);
        Directory.CreateDirectory(Path.GetDirectoryName(JsonPath));
        File.WriteAllText(JsonPath, jsonStr);
    }
}
