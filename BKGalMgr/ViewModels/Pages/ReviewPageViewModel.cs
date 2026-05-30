using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels.Pages;

public partial class ReviewPageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial long StorageUsage { get; set; } = 0;

    [ObservableProperty]
    public partial int RepositoriesNumber { get; set; } = 0;

    [ObservableProperty]
    public partial TimeSpan PlayedTime { get; set; } = TimeSpan.Zero;

    [ObservableProperty]
    public partial int GamesNumber { get; set; } = 0;

    [ObservableProperty]
    public partial int SessionsPlayed { get; set; } = 0;

    [ObservableProperty]
    public partial ObservableCollection<GameReviewGroupInfo> Groups { get; set; }
    public LibraryAndManagePageViewModel LibraryAndManagePageViewModel { get; }

    public ReviewPageViewModel(LibraryAndManagePageViewModel libraryAndManagePageViewModel)
    {
        LibraryAndManagePageViewModel = libraryAndManagePageViewModel;
    }

    public async Task RefreshAsync()
    {
        StorageUsage = 0;
        RepositoriesNumber = LibraryAndManagePageViewModel.Repository.Count();
        PlayedTime = TimeSpan.Zero;
        GamesNumber = 0;
        SessionsPlayed = 0;
        var groups = new List<GameReviewGroupInfo>();
        foreach (var repo in LibraryAndManagePageViewModel.Repository)
        {
            if (repo.Ignore)
                continue;

            await repo.RefreshStorageUsageAsync();
            StorageUsage += repo.StorageUsage;
            GamesNumber += repo.Games.Count;
            foreach (var game in repo.Games.OrderByDescending(g => g.LastPlayDate))
            {
                PlayedTime += game.PlayedTime;
                SessionsPlayed += game
                    .PlayedPeriods.Where(t =>
                        t.Period >= TimeSpan.FromSeconds(60) || t.PauseTime >= TimeSpan.FromSeconds(60)
                    )
                    .Count();
                foreach (var playedPriods in game.PlayedPeriods)
                {
                    var label = playedPriods.BenginTime.ToString("d", CultureInfo.CurrentUICulture);
                    // same day
                    var group = groups.Find(g =>
                        (int)(new TimeSpan(g.Label.Ticks)).TotalDays
                        == (int)(new TimeSpan(playedPriods.BenginTime.Ticks)).TotalDays
                    );
                    if (group == null)
                    {
                        group = new() { Label = playedPriods.BenginTime, Games = new() };
                        groups.Add(group);
                    }
                    group.PlayedTime += playedPriods.Period;
                    var item = group.Games.Where(g => g.Game.Name == game.Name).FirstOrDefault();
                    if (item == null)
                        group.Games.Add(new() { PlayedTime = playedPriods.Period, Game = game });
                    else
                        item.PlayedTime += playedPriods.Period;
                }
            }
        }
        Groups = new(groups.OrderByDescending(g => g.Label));
    }

    public async Task RefreshAsync(List<GameInfo> games)
    {
        StorageUsage = 0;
        RepositoriesNumber = games.Select(t => t.Repository).Distinct().Count();
        PlayedTime = TimeSpan.Zero;
        GamesNumber = games.Count;
        SessionsPlayed = 0;
        var groups = new List<GameReviewGroupInfo>();

        foreach (var game in games.OrderByDescending(g => g.LastPlayDate))
        {
            StorageUsage += game.StorageUsage == 0 ? await game.RefreshStorageUsageAsync() : game.StorageUsage;
            PlayedTime += game.PlayedTime;
            SessionsPlayed += game
                .PlayedPeriods.Where(t =>
                    t.Period >= TimeSpan.FromSeconds(60) || t.PauseTime >= TimeSpan.FromSeconds(60)
                )
                .Count();

            foreach (var playedPriods in game.PlayedPeriods)
            {
                var label = playedPriods.BenginTime.ToString("d", CultureInfo.CurrentUICulture);
                // same day
                var group = groups.Find(g =>
                    (int)(new TimeSpan(g.Label.Ticks)).TotalDays
                    == (int)(new TimeSpan(playedPriods.BenginTime.Ticks)).TotalDays
                );
                if (group == null)
                {
                    group = new() { Label = playedPriods.BenginTime, Games = new() };
                    groups.Add(group);
                }
                group.PlayedTime += playedPriods.Period;
                var item = group.Games.Where(g => g.Game.Name == game.Name).FirstOrDefault();
                if (item == null)
                    group.Games.Add(new() { PlayedTime = playedPriods.Period, Game = game });
                else
                    item.PlayedTime += playedPriods.Period;
            }
        }
        Groups = new(groups.OrderByDescending(g => g.Label));
    }
}
