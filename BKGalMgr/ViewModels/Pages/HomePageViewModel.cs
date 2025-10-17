using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels.Pages;

public partial class HomePageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<GameInfo> _banners = new();

    [ObservableProperty]
    private int _bannersCount = 12;

    [ObservableProperty]
    private ObservableCollection<GameInfo> _repositoryCovers = new();

    [ObservableProperty]
    private DateTime _currentDate;

    [ObservableProperty]
    private ObservableCollection<GameReviewGroupItem> _recentlyPlayedGames = new();

    [ObservableProperty]
    private ObservableCollection<GameReviewGroupItem> _recentlyCreatedGames = new();

    [ObservableProperty]
    private ObservableCollection<GameReviewGroupInfo> _groups = new();

    public LibraryAndManagePageViewModel LibraryAndManagePageViewModel { get; }

    public HomePageViewModel(LibraryAndManagePageViewModel libraryAndManagePageViewModel)
    {
        LibraryAndManagePageViewModel = libraryAndManagePageViewModel;
    }

    public void Refresh()
    {
        RepositoryCovers.Clear();
        List<GameInfo> allGames = new();
        foreach (var repo in LibraryAndManagePageViewModel.Repository)
        {
            if (!repo.Games.Any() || repo.Ignore)
                continue;

            allGames.AddRange(repo.Games);
            RepositoryCovers.Add(repo.Games.OrderByDescending(t => t.LastPlayDate).First());
        }
        if (!allGames.Any())
            return;

        int count = Math.Min(allGames.Count, 12);

        Banners.Clear();
        Banners.AddRange(allGames.Where(t => !t.Cover.IsNullOrEmpty()).ToList().TakeRandom(count));
        BannersCount = Banners.Count();

        allGames = allGames.OrderByDescending(t => t.LastPlayDate).ToList();
        RecentlyPlayedGames.Clear();
        RecentlyPlayedGames.AddRange(allGames.Take(count).Select(t => new GameReviewGroupItem() { Game = t }));

        allGames = allGames.OrderByDescending(t => t.CreateDate).ToList();
        RecentlyCreatedGames.Clear();
        RecentlyCreatedGames.AddRange(allGames.Take(count).Select(t => new GameReviewGroupItem() { Game = t }));

        Dictionary<string, GameReviewGroupInfo> groupMap = new();
        foreach (var game in allGames)
        {
            foreach (var group in game.Group)
            {
                groupMap.TryAdd(group, new() { Name = group, Games = new() });
                groupMap[group].Games.Add(new() { Game = game });
            }
        }
        Groups.Clear();
        Groups.AddRange(groupMap.Values);
    }
}
