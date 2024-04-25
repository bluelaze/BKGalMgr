using Microsoft.VisualBasic;
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedGameIsValid))]
    [property: JsonIgnore]
    private GameInfo _selectedGame;
    partial void OnSelectedGameChanged(GameInfo value)
    {
        SeletedGameCreateDate = SelectedGame?.CreateDate ?? new();
        SaveJsonFile();
    }

    [property: JsonIgnore]
    public bool SelectedGameIsValid { get { return SelectedGame != null; } }
    public DateTime? SeletedGameCreateDate { get; set; }

    [property: JsonIgnore]
    private string JsonPath => Path.Combine(FolderPath, GlobalInfo.RepositoryJsonName);

    public RepositoryInfo()
    {
        GamesView = new(Games, true);
        GamesView.Filter = GamesViewFilter;

        SearchToken.CollectionChanged += (_, _) =>
        {
            GamesView.RefreshFilter();
        };
    }
    class StringContainsComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == null || y == null) return false;
            return y.Contains(x);
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            // zero to comparer all
            return 0;
        }
    }
    public bool GamesViewFilter(object game)
    {
        GameInfo gameInfo = game as GameInfo;
        if (gameInfo != null && SearchToken.Count() > 0)
        {
            return gameInfo.GetAllTags().Intersect(SearchToken, new StringContainsComparer()).Count() > 0;
        }
        return true;
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
            repositoryInfo = JsonSerializer.Deserialize<RepositoryInfo>(File.ReadAllBytes(jsonPath));
        if (repositoryInfo == null)
            repositoryInfo = defaultValue ?? new RepositoryInfo();

        repositoryInfo.FolderPath = folderPath;

        var dirs = Directory.GetDirectories(folderPath);
        foreach (var dir in dirs)
        {
            var game = GameInfo.Open(dir);
            if (game != null)
            {
                repositoryInfo.Games.Add(game);
                if (game.CreateDate == repositoryInfo.SeletedGameCreateDate)
                    repositoryInfo._selectedGame = game;
            }
        }
        repositoryInfo.GamesViewSort();
        return repositoryInfo;
    }

    public bool IsValid()
    {
        return !Name.IsNullOrEmpty() && !FolderPath.IsNullOrEmpty();
    }

    public GameInfo NewGame()
    {
        var gameInfo = new GameInfo();
        gameInfo.SetRepositoryPath(FolderPath);

        return gameInfo;
    }

    public void AddGame(GameInfo game)
    {
        Games.Add(game);
    }

    public async Task DeleteGame(GameInfo game)
    {
        if (Games.Contains(game))
        {
            if (Directory.Exists(game.FolderPath))
                await Task.Run(() => { Directory.Delete(game.FolderPath, true); });
            Games.Remove(game);
        }
        if(SelectedGame == game)
            SelectedGame = null;
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
