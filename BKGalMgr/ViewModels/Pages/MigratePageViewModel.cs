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
    public partial RepositoryInfo LeftRepository { get; set; }

    [ObservableProperty]
    public partial RepositoryInfo RightRepository { get; set; }
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

                // 可移动
                if (FileSystemMisc.IsSameRootPath(game.FolderPath, gameNewPath))
                {
                    (bool suc, string errmsg) = await Task.Run(
                        () => FileSystemMisc.IsMoveDirectoryValid(game.FolderPath, gameNewPath)
                    );
                    if (!suc)
                    {
                        App.ShowErrorMessage($"{game.Name} can not be move:\n{errmsg}");
                        return false;
                    }

                    var result = await Task.Run(() => FileSystemMisc.MoveDirectory(game.FolderPath, gameNewPath));
                    if (!result.success)
                    {
                        App.ShowErrorMessage($"{game.Name} move failed!\n{result.message}");
                        return false;
                    }
                }
                else
                {
                    // 复制
                    var ret = await Task.Run(() => FileSystemMisc.CopyDirectory(game.FolderPath, gameNewPath));
                    if (ret.success == false)
                    {
                        App.ShowErrorMessage($"{game.Name} copy failed!\n{ret.message}");
                        FileSystemMisc.DeleteDirectory(gameNewPath);
                        return false;
                    }
                }

                // 新库添加游戏
                await toRepository.AddGame(gameNewPath);
                toRepository.RestoreAddGroupIndex();
                // 旧库移除游戏
                if (await fromRepository.DeleteGameAsync(game) == false)
                {
                    App.ShowErrorMessage(
                        $"{game.Name} add to {toRepository.Name} successful,\nbut {fromRepository.Name} delete game failed!"
                    );
                    return false;
                }

                App.ShowSuccessMessage($"{game.Name} migrate successful！");
            }
        }
        return true;
    }
}
