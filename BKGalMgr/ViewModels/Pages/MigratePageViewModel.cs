using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels.Pages;

public partial class MigratePageViewModel : ObservableObject
{
    [ObservableProperty]
    private RepositoryInfo _leftRepository;

    [ObservableProperty]
    private RepositoryInfo _rightRepository;

    public LibraryAndManagePageViewModel LibraryAndManagePageViewModel { get; }

    public MigratePageViewModel(LibraryAndManagePageViewModel libraryAndManagePageViewModel)
    {
        LibraryAndManagePageViewModel = libraryAndManagePageViewModel;
    }

    public async Task<bool> MigrateGames(IList<GameInfo> games, bool fromLeftToRight)
    {
        if (games == null || LeftRepository == null || RightRepository == null)
            return false;
        RepositoryInfo fromRepository = LeftRepository;
        RepositoryInfo toRepository = RightRepository;
        if (!fromLeftToRight)
        {
            fromRepository = RightRepository;
            toRepository = LeftRepository;
        }
        foreach (var game in games)
        {
            if (fromRepository.Games.Contains(game))
            {
                var gameNewPath = Path.Combine(toRepository.FolderPath, Path.GetFileName(game.FolderPath));
                var ret = await Task.Run(() =>
                {
                    try
                    {
                        FileSystemMisc.DirectoryMoveOrCopy(game.FolderPath, gameNewPath);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                });
                if (ret == false)
                {
                    if (Directory.Exists(gameNewPath))
                        Directory.Delete(gameNewPath, true);
                    return false;
                }

                await fromRepository.DeleteGame(game);
                toRepository.AddGame(gameNewPath);
                toRepository.RestoreAddGroupIndex();
            }
        }
        return true;
    }
}
