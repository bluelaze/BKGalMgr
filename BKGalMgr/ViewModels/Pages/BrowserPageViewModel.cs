using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels.Pages;

public partial class BrowserPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value)
    {
        RefreshSuggestedTags();
    }

    [ObservableProperty]
    private ObservableCollection<string> _searchToken = new();

    [ObservableProperty]
    private ObservableCollection<string> _searchSuggestedTags = new();

    [ObservableProperty]
    private ObservableCollection<GroupInfo> _groups = new();

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
    private ObservableCollection<GameInfo> _games = new();

    [ObservableProperty]
    private AdvancedCollectionView _gamesView;

    private List<string> _allTags = new();

    public LibraryAndManagePageViewModel LibraryAndManagePageViewModel { get; }

    public BrowserPageViewModel(LibraryAndManagePageViewModel libraryAndManagePageViewModel)
    {
        LibraryAndManagePageViewModel = libraryAndManagePageViewModel;

        GamesView = new(Games, true);
        GamesView.Filter = OnGamesViewFilter;

        SearchToken.CollectionChanged += (_, _) => GamesViewRefreshFilter();
    }

    public void Refresh()
    {
        List<GameInfo> allGames = new();
        List<GroupInfo> allGroups = new();
        foreach (var repo in LibraryAndManagePageViewModel.Repository)
        {
            allGames.AddRange(repo.Games);
            allGroups = allGroups
                .UnionBy(repo.Groups.Where(t => t.Name != GlobalInfo.GroupItemCase_Add), t => t.Name)
                .Select(t => JsonMisc.Deserialize<GroupInfo>(JsonMisc.Serialize(t)))
                .ToList();
        }
        // 移除没有的，合入有的
        var exceptGame = Games.Except(allGames);
        foreach (var g in exceptGame)
        {
            Games.Remove(g);
        }
        Games.MergeRange(allGames);

        Groups.Clear();
        Groups.AddRange(allGroups);

        _allTags.Clear();
        foreach (var item in Games)
        {
            _allTags = _allTags.Union(item.GetAllTags()).ToList();
        }
        GamesViewSort();
    }

    void RefreshSuggestedTags()
    {
        SearchSuggestedTags.Clear();
        SearchSuggestedTags.AddRange(_allTags.Where(t => t != null && t.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
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
    void SelectSortType(string sortTypeName)
    {
        if (Enum.TryParse(sortTypeName, out SortType result))
        {
            SortType = result;
        }
    }

    [RelayCommand]
    void SelectSortOrderType(string sortOrderTypeName)
    {
        if (Enum.TryParse(sortOrderTypeName, out SortDirection result))
        {
            SortOrderType = result;
        }
    }
}
