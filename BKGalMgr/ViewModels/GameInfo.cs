using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BKGalMgr.Helpers;
using BKGalMgr.Models.Bangumi;
using BKGalMgr.ThirdParty;
using Windows.Storage;

namespace BKGalMgr.ViewModels;

public enum PlayStatus
{
    Stop,
    Playing,
    Pause,
}

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
    private ObservableCollection<PlayedPeriodInfo> _playedPeriods = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoverImageSource))]
    private string _cover;

    private Microsoft.UI.Xaml.Media.Imaging.BitmapImage _coverImageSource;

    [JsonIgnore]
    public Microsoft.UI.Xaml.Media.Imaging.BitmapImage CoverImageSource
    {
        get
        {
            if (Cover.IsNullOrEmpty())
                return null;
            // 看着只不支持svg当封面，目前用bitmap应该都可以
            // https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.media.imagesource
            //CoverImageSource = (Microsoft.UI.Xaml.Media.ImageSource)
            //    Microsoft.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(
            //        typeof(Microsoft.UI.Xaml.Media.ImageSource),
            //        value
            //    );
            if (_coverImageSource == null)
                _coverImageSource = new();
            _coverImageSource.UriSource = new(Cover);
            return _coverImageSource;
        }
    }

    [ObservableProperty]
    private string _company;

    [ObservableProperty]
    private DateTime _publishDate;

    [ObservableProperty]
    private int _pinValue = 1000;

    [ObservableProperty]
    private ObservableCollection<string> _artist = new();

    [ObservableProperty]
    private ObservableCollection<string> _cv = new();

    [ObservableProperty]
    private ObservableCollection<string> _scenario = new();

    [ObservableProperty]
    private ObservableCollection<string> _musician = new();

    [ObservableProperty]
    private ObservableCollection<string> _singer = new();

    [ObservableProperty]
    private ObservableCollection<CharacterInfo> _characters = new();

    [ObservableProperty]
    private ObservableCollection<string> _tag = new();

    [ObservableProperty]
    private ObservableCollection<string> _group = new();

    [ObservableProperty]
    private string _website;

    [ObservableProperty]
    private string _story;

    [ObservableProperty]
    private string _blog;

    [ObservableProperty]
    private string _bangumiSubjectId;

    [ObservableProperty]
    private string _t2DFanSubjectId;

    [ObservableProperty]
    private bool _stopTimingWhenNotActive = true;

    [ObservableProperty]
    private ShoppingSiteInfo _shoppingInfo;

    [ObservableProperty]
    public ThemeInfo _customTheme = new();

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<string> _covers = new();

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<string> _gallery = new();

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<string> _special = new();

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<string> _screenshot = new();

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<string> _websiteShot = new();

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<string> _bugBugNews = new();

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<string> _campaign = new();

    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isPropertyChanged;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlaying))]
    [property: JsonIgnore]
    private PlayStatus _playStatus = PlayStatus.Stop;

    [JsonIgnore]
    public CancellationTokenSource PlayCancelTokenSource { get; set; }

    [JsonIgnore]
    public bool IsPlaying => PlayStatus != PlayStatus.Stop;

    [ObservableProperty]
    [property: JsonIgnore]
    private RepositoryInfo _repository;

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
            if (value == null && Targets.Any())
                return;

            if (SetProperty(ref _selectedTarget, value))
            {
                SeletedTargetCreateDate = _selectedTarget?.CreateDate ?? new();
                SaveJsonFile();
            }
        }
    }
    public DateTime SeletedTargetCreateDate { get; set; }

    [ObservableProperty]
    [property: JsonIgnore]
    private SaveDataSettingsInfo _saveDataSettings = new();

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<SaveDataInfo> _saveDatas = new();

    [property: JsonIgnore]
    public string FolderPath => Path.GetDirectoryName(JsonPath);

    [property: JsonIgnore]
    public string ScreenshotFolderPath => Path.Combine(FolderPath, GlobalInfo.GameScreenshotFolderName);

    [property: JsonIgnore]
    public string ShortcutFolderPath => Path.Combine(FolderPath, GlobalInfo.GameShortcutFolderName);

    public GameInfo()
    {
        Group.CollectionChanged += Group_CollectionChanged;
    }

    public static GameInfo Open(string dirPath, RepositoryInfo repo)
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
                    target.Game = gameInfo;
                    gameInfo.Targets.Add(target);
                    if (target.CreateDate == gameInfo.SeletedTargetCreateDate)
                        gameInfo._selectedTarget = target;
                }
            }
        }

        path = Path.Combine(dirPath, GlobalInfo.SaveDatasFolderName);
        if (Directory.Exists(path))
        {
            gameInfo.SaveDataSettings = SaveDataSettingsInfo.Open(path) ?? new();
            gameInfo.SaveDataSettings.JsonPath = Path.Combine(path, GlobalInfo.SaveDataSettingsJsonName);

            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                var savedata = SaveDataInfo.Open(dir);
                if (savedata != null)
                    gameInfo.SaveDatas.Insert(0, savedata);
            }
        }
        gameInfo.SaveDataSettings.SetGamePath(dirPath);

        gameInfo.Refresh();
        gameInfo.LoadTheme();
        gameInfo.Repository = repo;
        gameInfo.Group.CollectionChanged += gameInfo.Group_CollectionChanged;
        gameInfo.IsPropertyChanged = false;
        return gameInfo;
    }

    public bool IsValid
    {
        get { return !Name.IsNullOrEmpty(); }
    }

    public void SetRepository(RepositoryInfo repository)
    {
        Repository = repository;
        if (JsonPath.IsNullOrEmpty())
        {
            JsonPath = Path.Combine(
                repository.FolderPath,
                CreateDate.ToString(GlobalInfo.FolderFormatStr),
                GlobalInfo.GameJsonName
            );
        }

        SaveDataSettings.SetGamePath(FolderPath);
    }

    public List<string> GetAllTags()
    {
        List<string> tags = [Name, Company];
        tags = tags.Union(Artist)
            .Union(Cv)
            .Union(Scenario)
            .Union(Musician)
            .Union(Singer)
            .Union(Tag)
            .Union(Group)
            .ToList();
        tags = tags.Union(Sources.Select(item => item.Name))
            .Union(Localizations.Select(item => item.Name))
            .Union(Targets.Select(item => item.Name))
            .ToList();
        return tags;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (
            e.PropertyName != nameof(IsPropertyChanged)
            && e.PropertyName != nameof(WebsiteShot)
            && e.PropertyName != nameof(BugBugNews)
            && e.PropertyName != nameof(Campaign)
            && e.PropertyName != nameof(CoverImageSource)
            && PlayStatus == PlayStatus.Stop
        )
            IsPropertyChanged = true;
        if (e.PropertyName == nameof(PinValue))
            SaveJsonFile();
    }

    private void Group_CollectionChanged(
        object sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e
    )
    {
        SaveJsonFile();
    }

    public void GroupChanged(GroupInfo oldGroup, GroupInfo newGroup, GroupChangedType type)
    {
        switch (type)
        {
            case GroupChangedType.Add:
                OnPropertyChanged(nameof(Group));
                break;
            case GroupChangedType.Remove:
                Group.Remove(oldGroup.Name);
                OnPropertyChanged(nameof(Group));
                break;
            case GroupChangedType.Edit:
                var index = Group.IndexOf(oldGroup?.Name);
                if (index == -1)
                {
                    OnPropertyChanged(nameof(Group));
                    break;
                }
                Group.Insert(index, newGroup.Name);
                Group.Remove(oldGroup.Name);
                break;
        }
        SaveJsonFile();
    }

    public void UpdateGame(GameInfo newGame)
    {
        if (Name.IsNullOrEmpty())
            Name = newGame.Name;
        if (Cover.IsNullOrEmpty())
            Cover = newGame.Cover;
        if (Company.IsNullOrEmpty())
            Company = newGame.Company;
        if (Website.IsNullOrEmpty())
            Website = newGame.Website;
        if (Story.IsNullOrEmpty())
            Story = newGame.Story;
        if (PublishDate.Ticks == 0)
            PublishDate = newGame.PublishDate;

        Musician.MergeRange(newGame.Musician);
        Artist.MergeRange(newGame.Artist);
        Singer.MergeRange(newGame.Singer);
        Scenario.MergeRange(newGame.Scenario);
        Cv.MergeRange(newGame.Cv);
        Tag.MergeRange(newGame.Tag);

        Characters.AddRange(newGame.Characters.ExceptBy(Characters.Select(c => c.Name), c => c.Name));
        // from bangumi
        foreach (var oldCharacter in Characters)
        {
            foreach (var newCharacter in newGame.Characters)
            {
                if (oldCharacter.Name == newCharacter.Name)
                {
                    oldCharacter.BangumiCharacterId = newCharacter.BangumiCharacterId;
                    if (oldCharacter.Illustration.IsNullOrEmpty())
                        oldCharacter.Illustration = newCharacter.Illustration;
                    if (oldCharacter.CV.IsNullOrEmpty())
                        oldCharacter.CV = newCharacter.CV;
                    if (oldCharacter.BloodType.IsNullOrEmpty())
                        oldCharacter.BloodType = newCharacter.BloodType;
                }
            }
            oldCharacter.GameFolderPath = FolderPath;
        }
        if (BangumiSubjectId != newGame.BangumiSubjectId && !newGame.BangumiSubjectId.IsNullOrEmpty())
            BangumiSubjectId = newGame.BangumiSubjectId;
    }

    public void AddPlayedPeriodToFirst(PlayedPeriodInfo playedPeriodInfo)
    {
        PlayedPeriods.Insert(0, playedPeriodInfo);
        // notify for chart to update
        OnPropertyChanged(nameof(PlayedPeriods));
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

            await copySource.CopySourceToFolder(newSource.FolderPath);

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
            var (success, message) = await FileSystemMisc.DeleteDirectoryAsync(source.FolderPath);
            if (!success)
            {
                App.ShowErrorMessage(message);
                return;
            }
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

            await copyLocalization.CopyLocalizationToFolder(newLocalization.FolderPath);

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
            var (success, message) = await FileSystemMisc.DeleteDirectoryAsync(localization.FolderPath);
            if (!success)
            {
                App.ShowErrorMessage(message);
                return;
            }
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
            var (success, message) = await FileSystemMisc.DeleteDirectoryAsync(targetInfo.FolderPath);
            if (!success)
            {
                App.ShowErrorMessage(message);
                return;
            }
            Targets.Remove(targetInfo);
        }
        if (targetInfo == SelectedTarget)
        {
            _selectedTarget = null;
            OnPropertyChanged(nameof(SelectedTarget));
        }
    }

    public SaveDataInfo NewSaveData()
    {
        var savedataInfo = new SaveDataInfo();
        savedataInfo.SetGamePath(FolderPath);

        return savedataInfo;
    }

    public async Task<bool> AddSaveData(SaveDataInfo savedataInfo)
    {
        if (!SaveDataSettings.IsValid() || !Directory.Exists(SaveDataSettings.SaveDataFolderPath))
            return false;

        var ret = await savedataInfo.BackupSaveDataFolder(SaveDataSettings.SaveDataFolderPath);
        if (ret)
            SaveDatas.Insert(0, savedataInfo);

        return ret;
    }

    public void UpdateSaveData(SaveDataInfo savedataInfo)
    {
        for (int i = 0; i < SaveDatas.Count; i++)
        {
            if (SaveDatas[i].CreateDate == savedataInfo.CreateDate)
            {
                SaveDatas[i] = savedataInfo;
            }
        }
    }

    public async Task<bool> RestoreSaveData(SaveDataInfo savedataInfo)
    {
        if (!SaveDataSettings.IsValid())
            return false;

        return await savedataInfo.RestoreToSaveDataFolder(SaveDataSettings.SaveDataFolderPath);
    }

    public async Task DeleteSaveData(SaveDataInfo savedata)
    {
        if (SaveDatas.Contains(savedata))
        {
            var (success, message) = await FileSystemMisc.DeleteDirectoryAsync(savedata.FolderPath);
            if (!success)
            {
                App.ShowErrorMessage(message);
                return;
            }
            SaveDatas.Remove(savedata);
        }
    }

    public bool CreateShortcut(string path)
    {
        try
        {
            FileSystemMisc.DeleteDirectory(ShortcutFolderPath);
            Directory.CreateDirectory(ShortcutFolderPath);
            if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(path, Path.Combine(ShortcutFolderPath, Path.GetFileName(path)), true);
            }
            else
            {
                return ShortcutHelpers.CreateShortcut(Path.Combine(ShortcutFolderPath, Path.GetFileName(path)), path);
            }
        }
        catch
        {
            return false;
        }
        return true;
    }

    public void MoveUpCharacter(CharacterInfo characterInfo)
    {
        int index = Characters.IndexOf(characterInfo);
        if (index > 0)
        {
            Characters.Move(index, index - 1);
            OnPropertyChanged(nameof(Characters));
        }
    }

    public void MoveDwonCharacter(CharacterInfo characterInfo)
    {
        int index = Characters.IndexOf(characterInfo);
        if (index >= 0 && index + 1 < Characters.Count)
        {
            Characters.Move(index, index + 1);
            OnPropertyChanged(nameof(Characters));
        }
    }

    public void DeleteCharacter(CharacterInfo characterInfo)
    {
        if (Characters.Contains(characterInfo))
        {
            Characters.Remove(characterInfo);
            OnPropertyChanged(nameof(Characters));
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

        IsPropertyChanged = false;
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenJsonFolder()
    {
        Process.Start("explorer", FolderPath);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenCoversFolder()
    {
        var coversPath = Path.Combine(FolderPath, GlobalInfo.GameCoversFolderName);
        Directory.CreateDirectory(coversPath);
        Process.Start("explorer", coversPath);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenGalleryFolder()
    {
        var galleryPath = Path.Combine(FolderPath, GlobalInfo.GameGalleryFolderName);
        Directory.CreateDirectory(galleryPath);
        Process.Start("explorer", galleryPath);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenSpecialFolder()
    {
        var specialPath = Path.Combine(FolderPath, GlobalInfo.GameSpecialFolderName);
        Directory.CreateDirectory(specialPath);
        Process.Start("explorer", specialPath);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenScreenshotFolder()
    {
        var capturePath = Path.Combine(FolderPath, GlobalInfo.GameScreenshotFolderName);
        Directory.CreateDirectory(capturePath);
        Process.Start("explorer", capturePath);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenCharacterFolder()
    {
        var characterPath = Path.Combine(FolderPath, GlobalInfo.GameCharacterFolderName);
        Directory.CreateDirectory(characterPath);
        Process.Start("explorer", characterPath);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenWebsiteShotFolder()
    {
        var websiteShotPath = Path.Combine(FolderPath, GlobalInfo.GameWebsiteShotFolderName);
        Directory.CreateDirectory(websiteShotPath);
        Process.Start("explorer", websiteShotPath);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenBugBugNewsFolder()
    {
        var bugBugNewsPath = Path.Combine(FolderPath, GlobalInfo.GameBugBugNewsFolderName);
        Directory.CreateDirectory(bugBugNewsPath);
        Process.Start("explorer", bugBugNewsPath);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenCampaignFolder()
    {
        var campaignShotPath = Path.Combine(FolderPath, GlobalInfo.GameCampaignFolderName);
        Directory.CreateDirectory(campaignShotPath);
        Process.Start("explorer", campaignShotPath);
    }

    public void LoadCover()
    {
        // 需要支持自定义封面
        // 封面是需要存储在文件夹内的，但是仓库可能会移动或者重命名
        // 所以需要提取出封面的相对路径
        string customCover = string.Empty;
        if (!Cover.IsNullOrEmpty())
        {
            int folderIndex = Cover.IndexOf(Path.GetFileName(FolderPath));
            if (folderIndex != -1)
            {
                customCover = Path.Combine(Path.GetDirectoryName(FolderPath), Cover.Substring(folderIndex));
            }
        }

        string defaultCover = string.Empty;
        foreach (var format in GlobalInfo.GameCoverSupportFormats)
        {
            var coverPath = Path.Combine(FolderPath, GlobalInfo.GameCoverName + format);
            if (File.Exists(coverPath))
            {
                defaultCover = coverPath;
                break;
            }
        }

        if (File.Exists(customCover))
        {
            Cover = customCover;
        }
        else if (File.Exists(defaultCover))
        {
            Cover = defaultCover;
        }

        var covers = new List<string>();
        var coversPath = Path.Combine(FolderPath, GlobalInfo.GameCoversFolderName);
        if (!defaultCover.IsNullOrEmpty())
            covers.Add(defaultCover);
        // 优先加载贩卖网站的几个文件夹
        covers.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(coversPath, GlobalInfo.MelonbooksFolderName)));
        covers.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(coversPath, GlobalInfo.DLsiteFolderName)));
        covers.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(coversPath, GlobalInfo.DMMFolderName)));
        covers.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(coversPath, GlobalInfo.GetchuFolderName)));
        covers.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(coversPath, GlobalInfo.MasterUpFolderName)));
        covers.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(coversPath, GlobalInfo.CharacterFolderName)));
        covers.MergeRange(FileSystemMisc.GetDirectoryFiles(coversPath));

        var coverIndex = covers.IndexOf(Cover);
        if (!Cover.IsNullOrEmpty() && coverIndex != 0)
        {
            if (coverIndex != -1)
                covers.RemoveAt(coverIndex);
            covers.Insert(0, Cover);
        }

        if (!Covers.Any())
        {
            Covers = new(covers);
            return;
        }
        // Move动效不行，不如这种删除插入的有点动效，性能可能差些
        Covers.RemoveIf(t => !covers.Contains(t));
        for (int i = 0; i < covers.Count; i++)
        {
            var existIndex = Covers.IndexOf(covers[i]);
            if (i == existIndex)
                continue;
            if (existIndex != -1)
                Covers.RemoveAt(existIndex);
            Covers.Insert(i, covers[i]);
        }
    }

    public void LoadGallery()
    {
        var galleryPath = Path.Combine(FolderPath, GlobalInfo.GameGalleryFolderName);
        if (Directory.Exists(galleryPath))
        {
            var images = FileSystemMisc.GetDirectoryFiles(galleryPath);
            Gallery.RemoveIf(t => !images.Contains(t));
            Gallery.MergeRange(images);
        }
    }

    public void LoadSpecial()
    {
        var specialPath = Path.Combine(FolderPath, GlobalInfo.GameSpecialFolderName);
        if (Directory.Exists(specialPath))
        {
            var images = new List<string>();
            // 优先加载几个文件夹
            // csharpier-ignore-start
            images.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(specialPath, GlobalInfo.TopFolderName)));
            images.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(specialPath, GlobalInfo.MovieFolderName)));
            images.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(specialPath, GlobalInfo.WorldFolderName)));
            images.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(specialPath, GlobalInfo.CharacterFolderName)));
            images.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(specialPath, GlobalInfo.TrialFolderName)));
            images.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(specialPath, GlobalInfo.BonusFolderName)));
            images.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(specialPath, GlobalInfo.ComicFolderName)));
            images.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(specialPath, GlobalInfo.NovelFolderName)));
            images.MergeRange(FileSystemMisc.GetDirectoryFiles(Path.Combine(specialPath, GlobalInfo.ASMRFolderName)));
            images.MergeRange(FileSystemMisc.GetDirectoryFiles(specialPath));
            // csharpier-ignore-end

            if (!Special.Any())
            {
                Special = new(images);
                return;
            }
            Special.RemoveIf(t => !images.Contains(t));
            for (int i = 0; i < images.Count; i++)
            {
                var existIndex = Special.IndexOf(images[i]);
                if (i == existIndex)
                    continue;
                if (existIndex != -1)
                    Special.RemoveAt(existIndex);
                Special.Insert(i, images[i]);
            }
        }
    }

    public void LoadScreenshot()
    {
        var capturePath = Path.Combine(FolderPath, GlobalInfo.GameScreenshotFolderOldName);
        var shotPath = Path.Combine(FolderPath, GlobalInfo.GameScreenshotFolderName);

        // 重命名旧的截图文件夹screencaptures为screenshot
        if (Directory.Exists(capturePath) && !Directory.Exists(shotPath))
        {
            try
            {
                Directory.Move(capturePath, shotPath);
            }
            catch (Exception e)
            {
                App.ShowErrorMessage(e.Message);
            }
        }

        if (Directory.Exists(shotPath))
        {
            var images = FileSystemMisc.GetDirectoryFiles(shotPath);
            Screenshot.RemoveIf(t => !images.Contains(t));
            Screenshot.MergeRange(images);
        }
    }

    public void LoadCharacter()
    {
        foreach (var c in Characters)
        {
            c.GameFolderPath = FolderPath;
            c.LoadIllustration();
        }
    }

    public void LoadWebsiteShot()
    {
        var websiteShotPath = Path.Combine(FolderPath, GlobalInfo.GameWebsiteShotFolderName);
        if (Directory.Exists(websiteShotPath))
        {
            var images = FileSystemMisc.GetDirectoryFiles(websiteShotPath);
            WebsiteShot.RemoveIf(t => !images.Contains(t));
            WebsiteShot.MergeRange(images);
        }
        else
        {
            WebsiteShot.Clear();
        }
        OnPropertyChanged(nameof(WebsiteShot));
    }

    public void LoadBugBugNews()
    {
        var bugBugNewsPath = Path.Combine(FolderPath, GlobalInfo.GameBugBugNewsFolderName);
        if (Directory.Exists(bugBugNewsPath))
        {
            var images = FileSystemMisc.GetDirectoryFiles(bugBugNewsPath);
            BugBugNews.RemoveIf(t => !images.Contains(t));
            BugBugNews.MergeRange(images);
        }
        else
        {
            BugBugNews.Clear();
        }
        OnPropertyChanged(nameof(BugBugNews));
    }

    public void LoadCampaign()
    {
        var campaignPath = Path.Combine(FolderPath, GlobalInfo.GameCampaignFolderName);
        if (Directory.Exists(campaignPath))
        {
            var images = FileSystemMisc.GetDirectoryFiles(campaignPath);
            Campaign.RemoveIf(t => !images.Contains(t));
            Campaign.MergeRange(images);
        }
        else
        {
            Campaign.Clear();
        }
        OnPropertyChanged(nameof(Campaign));
    }

    public void LoadTheme()
    {
        // 如果指定的图片是在游戏文件夹内的，则可能是会发生游戏迁移导致背景实现
        // 所以根据路径拼接判断下重新设置
        if (!CustomTheme.BackgroundImage.IsNullOrEmpty())
        {
            int folderIndex = CustomTheme.BackgroundImage.IndexOf(Path.GetFileName(FolderPath));
            if (folderIndex != -1)
            {
                string imagePath = Path.Combine(
                    Path.GetDirectoryName(FolderPath),
                    CustomTheme.BackgroundImage.Substring(folderIndex)
                );
                if (File.Exists(imagePath))
                    CustomTheme.BackgroundImage = imagePath;
            }
        }
    }

    public void Refresh()
    {
        LoadCover();
        LoadGallery();
        LoadSpecial();
        LoadScreenshot();
        LoadCharacter();
        LoadWebsiteShot();
        LoadBugBugNews();
        LoadCampaign();
    }

    private string TransformCoverPath(string path)
    {
        string absolutePath = path.StartsWith("http") ? (new Uri(path)).AbsolutePath : path;
        var format = Path.GetExtension(absolutePath).ToLower();
        if (GlobalInfo.GameCoverSupportFormats.Contains(format))
            return Path.Combine(FolderPath, GlobalInfo.GameCoverName + format);
        return string.Empty;
    }

    public async Task SaveCover()
    {
        if (Cover.IsNullOrEmpty())
        {
            LoadCover();
            return;
        }

        // check format, build path
        var coverPath = TransformCoverPath(Cover);
        if (coverPath.IsNullOrEmpty())
            return;

        // copy local file
        if (File.Exists(Cover) && !Path.GetFullPath(Cover).StartsWith(Path.GetFullPath(FolderPath)))
        {
            File.Copy(Cover, coverPath, true);
            Cover = coverPath;
            return;
        }

        // network image
        if (!Cover.StartsWith("http"))
        {
            LoadCover();
            return;
        }
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

    [RelayCommand]
    [property: JsonIgnore]
    public void CustomCover()
    {
        string coversFolder = Path.Combine(FolderPath, GlobalInfo.GameCoversFolderName);
        var cover = FileSystemMisc.PickFile(coversFolder, new() { "Image Files|*.*" })?.FirstOrDefault();
        if (cover == null)
            return;
        Cover = cover;
        _ = SaveCover();
        SaveJsonFile();
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void MoveImageToCoverFolder(string folderName)
    {
        if (folderName.IsNullOrEmpty())
            return;
        string coversFolder = Path.Combine(FolderPath, GlobalInfo.GameCoversFolderName);
        string siteFolder = "";
        if (
            string.Equals(folderName, GlobalInfo.GetchuFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.DMMFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.DLsiteFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.MelonbooksFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.MasterUpFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.CharacterFolderName, StringComparison.OrdinalIgnoreCase)
        )
        {
            siteFolder = Path.Combine(coversFolder, folderName.ToLower());
        }
        else if (string.Equals(folderName, GlobalInfo.GameCoversFolderName, StringComparison.OrdinalIgnoreCase))
        {
            siteFolder = Path.Combine(FolderPath, folderName.ToLower());
        }
        else
        {
            return;
        }
        var images = FileSystemMisc.PickFile(coversFolder, new() { "Image Files|*.*" });
        if (images == null)
            return;
        foreach (var image in images)
        {
            FileSystemMisc.MoveOrCopyFile(image, Path.Combine(siteFolder, Path.GetFileName(image)));
        }
        LoadCover();
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void MoveImageToSpecialFolder(string folderName)
    {
        if (folderName.IsNullOrEmpty())
            return;
        string specialFolder = Path.Combine(FolderPath, GlobalInfo.GameSpecialFolderName);
        string groupsiteFolder = "";
        if (
            string.Equals(folderName, GlobalInfo.TopFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.MovieFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.WorldFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.CharacterFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.TrialFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.BonusFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.ComicFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.NovelFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.ASMRFolderName, StringComparison.OrdinalIgnoreCase)
        )
        {
            groupsiteFolder = Path.Combine(specialFolder, folderName.ToLower());
        }
        else if (
            string.Equals(folderName, GlobalInfo.GameSpecialFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.GameCampaignFolderName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folderName, GlobalInfo.GameWebsiteShotFolderName, StringComparison.OrdinalIgnoreCase)
        )
        {
            groupsiteFolder = Path.Combine(FolderPath, folderName.ToLower());
        }
        else
        {
            return;
        }
        var images = FileSystemMisc.PickFile(specialFolder, new() { "Image Files|*.*" });
        if (images == null)
            return;
        foreach (var image in images)
        {
            FileSystemMisc.MoveOrCopyFile(image, Path.Combine(groupsiteFolder, Path.GetFileName(image)));
        }
        LoadSpecial();
    }

    public async Task SaveScreenshot(TargetInfo targetInfo, Bitmap bitmap)
    {
        Directory.CreateDirectory(ScreenshotFolderPath);

        string screenshotPath = Path.Combine(
            ScreenshotFolderPath,
            $"{targetInfo.Name.ValidFileName("_")}_{DateTime.Now.ToString(GlobalInfo.GameScreenshotFileFormatStr)}.png"
        );
        var picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);

        await Task.Run(() =>
        {
            bitmap.Save(screenshotPath, ImageFormat.Png);
            // also save to Pictures
            bitmap.Save(
                Path.Combine(
                    picturesLibrary.SaveFolder.Path,
                    $"BKGalMgr_{DateTime.Now.ToString(GlobalInfo.GameScreenshotFileFormatStr)}.png"
                ),
                ImageFormat.Png
            );
        });

        if (CustomTheme.LastScreenshotAsBackground)
            CustomTheme.BackgroundImage = screenshotPath;
    }
}
