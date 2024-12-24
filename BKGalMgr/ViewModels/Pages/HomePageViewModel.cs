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
        List<GameInfo> allCoverGames = new();
        foreach (var repo in LibraryAndManagePageViewModel.Repository)
        {
            if (!repo.Games.Any())
                continue;
            var r = repo.Games.Where(t => !t.Cover.IsNullOrEmpty());
            RepositoryCovers.Add(r.OrderByDescending(t => t.LastPlayDate).FirstOrDefault());
            allCoverGames.AddRange(r);
        }
        if (!allCoverGames.Any())
            return;

        int count = Math.Min(allCoverGames.Count, 12);

        Banners.Clear();
        Banners.AddRange(allCoverGames.TakeRandom(count));
        BannersCount = Banners.Count();

        allCoverGames = allCoverGames.OrderByDescending(t => t.LastPlayDate).ToList();
        RecentlyPlayedGames.Clear();
        RecentlyPlayedGames.AddRange(allCoverGames.Take(count).Select(t => new GameReviewGroupItem() { Game = t }));

        allCoverGames = allCoverGames.OrderByDescending(t => t.CreateDate).ToList();
        RecentlyCreatedGames.Clear();
        RecentlyCreatedGames.AddRange(allCoverGames.Take(count).Select(t => new GameReviewGroupItem() { Game = t }));

        Dictionary<string, GameReviewGroupInfo> groupMap = new();
        foreach (var game in allCoverGames)
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
