using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace BKGalMgr.ViewModels;

[Serializable]
public partial class RepositoryInfo : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    [JsonIgnore]
    public partial string FolderPath { get; set; }

    [ObservableProperty]
    public partial DateTime CreateDate { get; set; } = DateTime.Now;

    [ObservableProperty]
    public partial string Description { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchSuggestedTags))]
    [JsonIgnore]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    [JsonIgnore]
    public partial ObservableCollection<string> SearchToken { get; set; } = new();

    [JsonIgnore]
    public List<string> SearchSuggestedTags => GetSuggestedTags();

    [ObservableProperty]
    public partial ObservableCollection<GroupInfo> Groups { get; set; } =
        new() { new() { Name = GlobalInfo.GroupItemCase_Add } };

    [ObservableProperty]
    public partial bool IsEnableGroup { get; set; } = false;

    partial void OnIsEnableGroupChanged(bool value) => GamesViewRefreshFilter();

    [ObservableProperty]
    public partial SortType SortType { get; set; } = SortType.CreateDate;

    [ObservableProperty]
    public partial SortDirection SortOrderType { get; set; } = SortDirection.Descending;

    partial void OnSortTypeChanged(SortType value)
    {
        GamesViewSort();
        SaveJsonFile();
    }

    partial void OnSortOrderTypeChanged(SortDirection value)
    {
        GamesViewSort();
        SaveJsonFile();
    }

    [ObservableProperty]
    public partial bool Ignore { get; set; } = false;

    partial void OnIgnoreChanged(bool value) => SaveJsonFile();

    [ObservableProperty]
    [JsonIgnore]
    public partial ObservableCollection<GameInfo> Games { get; set; } = new();

    [ObservableProperty]
    [JsonIgnore]
    public partial AdvancedCollectionView GamesView { get; set; }

    private GameInfo _selectedGame;

    [JsonIgnore]
    public GameInfo SelectedGame
    {
        get => _selectedGame;
        set
        {
            if (value == null && Games.Any())
                return;
            if (SetProperty(ref _selectedGame, value))
            {
                SeletedGameCreateDate = SelectedGame?.CreateDate ?? new();
                SaveJsonFile();
            }
        }
    }

    // 。。。yet, also, Selected is correct, but data is save
    public DateTime? SeletedGameCreateDate { get; set; }

    [JsonIgnore]
    private string JsonPath => Path.Combine(FolderPath, GlobalInfo.RepositoryJsonName);

    [ObservableProperty]
    [JsonIgnore]
    public partial long StorageUsage { get; set; } = 0;

    public RepositoryInfo()
    {
        GamesView = new(Games, true);
        GamesView.Filter = OnGamesViewFilter;

        SearchToken.CollectionChanged += (_, _) => GamesViewRefreshFilter();
    }

    public static bool IsExistedRepository(string folderPath)
    {
        return Path.Exists(Path.Combine(folderPath, GlobalInfo.RepositoryJsonName));
    }

    public static async Task<RepositoryInfo> Open(string folderPath, RepositoryInfo defaultValue)
    {
        if (!Path.Exists(folderPath))
            return null;

        var repositoryInfo = defaultValue;
        var jsonPath = Path.Combine(folderPath, GlobalInfo.RepositoryJsonName);
        if (IsExistedRepository(folderPath))
            repositoryInfo = JsonMisc.Deserialize<RepositoryInfo>(File.ReadAllText(jsonPath));
        if (repositoryInfo == null)
            repositoryInfo = defaultValue ?? new RepositoryInfo();

        repositoryInfo.FolderPath = folderPath;

        if (repositoryInfo.Ignore)
            return repositoryInfo;

        await repositoryInfo.Load();
        return repositoryInfo;
    }

    public async Task Load()
    {
        var dirs = Directory.GetDirectories(FolderPath).ToList();
        dirs.Sort();
        foreach (var dir in dirs)
        {
            await AddGame(dir);
        }

        RestoreAddGroupIndex();
        GamesViewSort();
    }

    public bool IsValid()
    {
        return !Name.IsNullOrEmpty() && !FolderPath.IsNullOrEmpty();
    }

    public List<string> GetSuggestedTags()
    {
        List<string> tags = new List<string>();
        foreach (var item in Games)
            tags = tags.Union(item.GetAllTags()).ToList();

        return tags.Where(t => t != null && t.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<long> RefreshStorageUsageAsync()
    {
        StorageUsage = 0;
        foreach (var item in Games)
        {
            StorageUsage += await item.RefreshStorageUsageAsync();
        }
        return StorageUsage;
    }

    public void RestoreAddGroupIndex()
    {
        // placeholder for add group
        var addGroupIndex = Groups.IndexOf(g => g.Name == GlobalInfo.GroupItemCase_Add);
        if (addGroupIndex == -1)
        {
            Groups.Add(new() { Name = GlobalInfo.GroupItemCase_Add });
        }
        else if (addGroupIndex != Groups.Count - 1)
        {
            Groups.Move(addGroupIndex, Groups.Count - 1);
        }
    }

    class StringContainsComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == null || y == null)
                return false;
            return y.Contains(x, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            // zero to comparer all
            return 0;
        }
    }

    public void GamesViewRefreshFilter()
    {
        GamesView.RefreshFilter();
        SaveJsonFile();
    }

    private bool OnGamesViewFilter(object game)
    {
        bool hit = true;
        if (game is GameInfo gameInfo)
        {
            if (SearchToken.Any())
                hit = gameInfo.GetAllTags().Intersect(SearchToken, new StringContainsComparer()).Any();

            if (hit && IsEnableGroup)
            {
                var validGroup = Groups.Where(g => g.IsChecked).Select(g => g.Name);
                if (validGroup.Any())
                {
                    hit = gameInfo.Group.Intersect(validGroup).Any();
                }
            }
        }
        return hit;
    }

    public void GamesViewSort()
    {
        GamesView.SortDescriptions.Clear();
        GamesView.SortDescriptions.Add(new(SortType.ToString(), SortOrderType));
        if (SortType == SortType.Company || SortType == SortType.PinValue)
        {
            GamesView.SortDescriptions.Add(new(SortType.PublishDate.ToString(), SortDirection.Descending));
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    void SelectSortType(string sortTypeName)
    {
        if (Enum.TryParse(sortTypeName, out SortType result))
        {
            SortType = result;
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    void SelectSortOrderType(string sortOrderTypeName)
    {
        if (Enum.TryParse(sortOrderTypeName, out SortDirection result))
        {
            SortOrderType = result;
        }
    }

    public void GroupChanged(GroupInfo oldGroup, GroupInfo newGroup, GroupChangedType type)
    {
        foreach (var game in Games)
        {
            game.GroupChanged(oldGroup, newGroup, type);
        }

        switch (type)
        {
            case GroupChangedType.Add:
                if (!Groups.Where(g => g.Name == newGroup.Name).Any())
                {
                    Groups.Insert(Groups.Count() - 1, newGroup);
                    SaveJsonFile();
                }
                break;
            case GroupChangedType.Remove:
                Groups.Remove(oldGroup);
                break;
            case GroupChangedType.Edit:
                var index = Groups.IndexOf(oldGroup);
                if (index == -1)
                    break;
                // 注意这里改了oldGroup的值，因为是同一个引用
                Groups[index].Name = newGroup.Name;
                break;
        }
        SaveJsonFile();
    }

    public GameInfo NewGame()
    {
        var game = new GameInfo();
        game.SetRepository(this);
        return game;
    }

    public void AddGame(GameInfo game)
    {
        game.SetRepository(this);
        Games.Add(game);
    }

    public async Task AddGame(string folderPath)
    {
        if (Games.Any(t => t.FolderPath == folderPath))
            return;

        var game = await Task.Run(() => GameInfo.Open(folderPath, this));
        if (game == null)
            return;

        Games.Add(game);
        if (game.CreateDate == SeletedGameCreateDate)
            SelectedGame = game;
        // merge game group, maybe copy from other repo
        foreach (var groupName in game.Group.Except(Groups.Select(g => g.Name)))
        {
            Groups.Add(new() { Name = groupName });
        }
    }

    public async Task<bool> DeleteGameAsync(GameInfo game)
    {
        if (Games.Contains(game))
        {
            var (success, message) = await FileSystemMisc.DeleteDirectoryAsync(game.FolderPath);
            if (!success)
            {
                App.ShowErrorMessage(message);
                return false;
            }
            Games.Remove(game);
        }
        if (SelectedGame == game)
        {
            _selectedGame = null;
            OnPropertyChanged(nameof(SelectedGame));
        }
        return true;
    }

    public void SaveJsonFile()
    {
        if (!IsValid())
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
}
