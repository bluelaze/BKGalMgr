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
    public partial string SearchText { get; set; } = string.Empty;

    partial void OnSearchTextChanged(string value)
    {
        RefreshSuggestedTags();
    }

    [ObservableProperty]
    public partial ObservableCollection<string> SearchToken { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<string> SearchSuggestedTags { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<GroupInfo> Groups { get; set; } = new();

    [ObservableProperty]
    public partial bool IsEnableGroup { get; set; } = false;

    partial void OnIsEnableGroupChanged(bool value) => GamesViewRefreshFilter();

    [ObservableProperty]
    public partial SortType SortType { get; set; } = SortType.CreateDate;

    [ObservableProperty]
    public partial SortDirection SortOrderType { get; set; } = SortDirection.Descending;

    partial void OnSortTypeChanged(SortType value) => GamesViewSort();

    partial void OnSortOrderTypeChanged(SortDirection value) => GamesViewSort();

    [ObservableProperty]
    public partial bool IsEnableRepository { get; set; } = false;

    partial void OnIsEnableRepositoryChanged(bool value) => GamesViewRefreshFilter();

    public List<RepositoryInfo> SelectedRepositories = new();

    [ObservableProperty]
    public partial bool IsEnableCompany { get; set; } = false;

    partial void OnIsEnableCompanyChanged(bool value) => GamesViewRefreshFilter();

    [ObservableProperty]
    public partial List<string> AllCompanines { get; set; } = new();

    public List<string> SelectedCompanines = new();

    [ObservableProperty]
    public partial bool IsEnablePublishDate { get; set; } = false;

    partial void OnIsEnablePublishDateChanged(bool value) => GamesViewRefreshFilter();

    [ObservableProperty]
    public partial DateTime PublishDateBegin { get; set; }

    partial void OnPublishDateBeginChanged(DateTime value)
    {
        if (IsEnablePublishDate)
            GamesViewRefreshFilter();
    }

    [ObservableProperty]
    public partial DateTime PublishDateEnd { get; set; }

    partial void OnPublishDateEndChanged(DateTime value)
    {
        if (IsEnablePublishDate)
            GamesViewRefreshFilter();
    }

    [ObservableProperty]
    public partial bool IsEnablePlayedDate { get; set; } = false;

    partial void OnIsEnablePlayedDateChanged(bool value) => GamesViewRefreshFilter();

    [ObservableProperty]
    public partial DateTime PlayedDateBegin { get; set; }

    partial void OnPlayedDateBeginChanged(DateTime value)
    {
        if (IsEnablePlayedDate)
            GamesViewRefreshFilter();
    }

    [ObservableProperty]
    public partial DateTime PlayedDateEnd { get; set; }

    partial void OnPlayedDateEndChanged(DateTime value)
    {
        if (IsEnablePlayedDate)
            GamesViewRefreshFilter();
    }

    [ObservableProperty]
    public partial ObservableCollection<GameInfo> Games { get; set; } = new();

    [ObservableProperty]
    public partial AdvancedCollectionView GamesView { get; set; }

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
            if (repo.Ignore)
                continue;

            allGames.AddRange(repo.Games);
            allGroups = allGroups
                .UnionBy(repo.Groups.Where(t => t.Name != GlobalInfo.GroupItemCase_Add), t => t.Name)
                .Select(t => JsonMisc.CloneObject(t))
                .ToList();
        }
        // 移除没有的，合入有的
        var exceptGame = Games.Except(allGames).ToList();
        foreach (var g in exceptGame)
        {
            Games.Remove(g);
        }
        Games.MergeRange(allGames);

        Groups.Clear();
        Groups.AddRange(allGroups);

        List<string> allTags = new();
        List<string> allCompanines = new();
        foreach (var item in Games)
        {
            allTags.AddRange(item.GetAllTags());
            allCompanines.Add(item.Company);
        }
        _allTags = allTags.Order().Distinct().ToList();
        AllCompanines = allCompanines.Where(i => !i.IsNullOrEmpty()).Order().Distinct().ToList();

        GamesViewSort();
    }

    void RefreshSuggestedTags()
    {
        SearchSuggestedTags.Clear();
        SearchSuggestedTags.AddRange(
            _allTags.Where(t => t != null && t.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
        );
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
            if (hit && IsEnableRepository)
            {
                hit = SelectedRepositories.Contains(gameInfo.Repository);
            }
            if (hit && IsEnableCompany)
            {
                hit = SelectedCompanines.Contains(gameInfo.Company);
            }
            if (hit && IsEnablePublishDate)
            {
                // 要考虑只设置开始或者结束的情况
                hit = gameInfo.PublishDate >= PublishDateBegin;
                if (hit && PublishDateEnd.Ticks > 0)
                    hit = gameInfo.PublishDate <= PublishDateEnd;
            }
            if (hit && IsEnablePlayedDate)
            {
                hit = false;
                if (gameInfo.PlayedPeriods?.Any() == true)
                {
                    foreach (var item in gameInfo.PlayedPeriods)
                    {
                        hit = item.BenginTime >= PlayedDateBegin || item.EndTime >= PlayedDateBegin;
                        if (hit && PlayedDateEnd.Ticks > 0)
                            hit =
                                item.BenginTime >= PlayedDateBegin && item.BenginTime <= PlayedDateEnd
                                || item.EndTime >= PlayedDateBegin && item.EndTime <= PlayedDateEnd;

                        if (hit)
                            break;
                    }
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
