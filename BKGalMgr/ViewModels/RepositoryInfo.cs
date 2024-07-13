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
    private string _name;

    [ObservableProperty]
    [property: JsonIgnore]
    private string _folderPath;

    [ObservableProperty]
    private DateTime _createDate = DateTime.Now;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchSuggestedTags))]
    [property: JsonIgnore]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<string> _searchToken = new();

    [property: JsonIgnore]
    public List<string> SearchSuggestedTags => GetSuggestedTags();

    [ObservableProperty]
    private ObservableCollection<GroupInfo> _groups = new() { new() { Name = GlobalInfo.GroupItemCase_Add } };

    [ObservableProperty]
    private bool _isEnableGroup = false;

    partial void OnIsEnableGroupChanged(bool value) => GamesViewRefreshFilter();

    [ObservableProperty]
    private SortType _sortType = SortType.CreateDate;

    [ObservableProperty]
    private SortDirection _sortOrderType = SortDirection.Descending;

    partial void OnSortTypeChanged(SortType value) => GamesViewSort();

    partial void OnSortOrderTypeChanged(SortDirection value) => GamesViewSort();

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<GameInfo> _games = new();

    [ObservableProperty]
    [property: JsonIgnore]
    private AdvancedCollectionView _gamesView;

    private GameInfo _selectedGame;

    [property: JsonIgnore]
    public GameInfo SelectedGame
    {
        get => _selectedGame;
        set
        {
            if (value == null && Games.Any())
                return;
            if (SetProperty(ref _selectedGame, value))
            {
                OnPropertyChanged(nameof(SelectedGameIsValid));

                SeletedGameCreateDate = SelectedGame?.CreateDate ?? new();
                SaveJsonFile();
            }
        }
    }

    [property: JsonIgnore]
    public bool SelectedGameIsValid
    {
        get { return SelectedGame != null; }
    }
    public DateTime? SeletedGameCreateDate { get; set; }

    [property: JsonIgnore]
    private string JsonPath => Path.Combine(FolderPath, GlobalInfo.RepositoryJsonName);

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

    public static RepositoryInfo Open(string folderPath, RepositoryInfo defaultValue)
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

        var dirs = Directory.GetDirectories(folderPath);
        foreach (var dir in dirs)
        {
            var game = GameInfo.Open(dir, repositoryInfo);
            if (game != null)
            {
                repositoryInfo.Games.Add(game);
                if (game.CreateDate == repositoryInfo.SeletedGameCreateDate)
                    repositoryInfo.SelectedGame = game;
                // merge game group, maybe copy from other repo
                foreach (var groupName in game.Group.Except(repositoryInfo.Groups.Select(g => g.Name)))
                {
                    repositoryInfo.Groups.Add(new() { Name = groupName });
                }
            }
        }
        // placeholder for add group
        if (!repositoryInfo.Groups.Where(g => g.Name == GlobalInfo.GroupItemCase_Add).Any())
            repositoryInfo.Groups.Add(new() { Name = GlobalInfo.GroupItemCase_Add });

        repositoryInfo.GamesViewSort();
        return repositoryInfo;
    }

    class StringContainsComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == null || y == null)
                return false;
            return y.Contains(x);
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
        SaveJsonFile();
    }

    public List<string> GetSuggestedTags()
    {
        List<string> tags = new List<string>();
        foreach (var item in Games)
            tags = tags.Union(item.GetAllTags()).ToList();

        return tags.Where(t => t != null && t.Contains(SearchText)).ToList();
    }

    public bool IsValid()
    {
        return !Name.IsNullOrEmpty() && !FolderPath.IsNullOrEmpty();
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

    public async Task DeleteGame(GameInfo game)
    {
        if (Games.Contains(game))
        {
            if (Directory.Exists(game.FolderPath))
                await Task.Run(() =>
                {
                    Directory.Delete(game.FolderPath, true);
                });
            Games.Remove(game);
        }
        if (SelectedGame == game)
            SelectedGame = null;
    }

    public void GroupChanged(GroupInfo oldGroup, GroupInfo newGroup, GroupChangedType type)
    {
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
                Groups[index].Name = newGroup.Name;
                break;
        }
        foreach (var game in Games)
            game.GroupChanged(oldGroup, newGroup, type);
        SaveJsonFile();
    }

    public void SaveJsonFile()
    {
        if (!IsValid())
            return;
        string jsonStr = JsonMisc.Serialize(this);
        Directory.CreateDirectory(Path.GetDirectoryName(JsonPath));
        File.WriteAllText(JsonPath, jsonStr);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenJsonFolder()
    {
        Process.Start("explorer", Path.GetDirectoryName(JsonPath));
    }
}
