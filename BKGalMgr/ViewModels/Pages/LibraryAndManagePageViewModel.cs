using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BKGalMgr.Models;
using BKGalMgr.Services;

namespace BKGalMgr.ViewModels.Pages;

public partial class LibraryAndManagePageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<RepositoryInfo> _repository = new();

    [ObservableProperty]
    private RepositoryInfo _selectedRepository;

    partial void OnSelectedRepositoryChanged(RepositoryInfo value)
    {
        Settings.SelectedRepositoryPath = SelectedRepository?.FolderPath ?? "";
    }

    public bool IsLoadedRepository { get; private set; } = false;

    public string BangumiAccessToken => Settings.Bangumi.AccessToken;

    public readonly SettingsDto Settings;

    public LibraryAndManagePageViewModel(SettingsDto settings)
    {
        Settings = settings;
    }

    public async Task LoadRepository()
    {
        if (IsLoadedRepository)
            return;
        IsLoadedRepository = true;

        foreach (var path in Settings.RepositoryPath)
        {
            await AddRepository(new RepositoryInfo() { FolderPath = path });
        }
    }

    public async Task<bool> AddRepository(RepositoryInfo repository)
    {
        if (repository.FolderPath.IsNullOrEmpty())
            return false;

        repository = await Task.Run(() => RepositoryInfo.Open(repository.FolderPath, repository));
        if (repository == null)
            return false;

        Repository.Add(repository);
        if (repository.FolderPath == Settings.SelectedRepositoryPath)
            SelectedRepository = repository;
        if (!Settings.RepositoryPath.Contains(repository.FolderPath))
        {
            Settings.RepositoryPath.Add(repository.FolderPath);
            Settings.SaveSettings();
        }
        return true;
    }

    public bool RemoveRepository(RepositoryInfo repository)
    {
        if (repository.FolderPath.IsNullOrEmpty())
            return false;

        Repository.Remove(repository);

        Settings.RepositoryPath.Remove(repository.FolderPath);
        Settings.SaveSettings();
        return true;
    }

    public void AddNewGame()
    {
        GameInfo newGame = SelectedRepository.NewGame();
        AddNewGame(newGame);
    }

    public void AddNewGame(GameInfo newGame)
    {
        if (newGame == null)
            return;
        SelectedRepository.AddGame(newGame);
        SelectedRepository.SelectedGame = newGame;
    }

    public void UpdateGame(GameInfo newGame)
    {
        if (newGame == null)
            return;
        SelectedRepository.SelectedGame.UpdateGame(newGame);
    }

    public void RefreshGame()
    {
        SelectedRepository.SelectedGame.Refresh();
    }

    public record PullGameResponse(GameInfo Game, string ErrorMessage);

    public async Task<PullGameResponse> PullGameFromBangumi(string accessToken, string subjectUrl)
    {
        Settings.Bangumi.AccessToken = accessToken;

        GameInfo newGame = SelectedRepository.NewGame();
        newGame.BangumiSubjectId = subjectUrl.Split('/').LastOrDefault();

        var errorMessage = await App.GetRequiredService<BangumiService>()
            .PullGameInfoAsync(newGame, newGame.BangumiSubjectId);

        if (!errorMessage.IsNullOrEmpty())
            return new(null, errorMessage);

        return new(newGame, errorMessage);
    }

    public async Task<string> PullGameCharacterFromBangumi(GameInfo gameInfo)
    {
        return await App.GetRequiredService<BangumiService>().PullGameCharacterInfoAsync(gameInfo);
    }

    public void UpdateSource(SourceInfo source)
    {
        source.SaveJsonFile();
        SelectedRepository.SelectedGame.UpdateSource(source);
    }

    public void UpdateLocalization(LocalizationInfo localizationInfo)
    {
        localizationInfo.SaveJsonFile();
        SelectedRepository.SelectedGame.UpdateLocalization(localizationInfo);
    }

    public void UpdateTarget(TargetInfo target)
    {
        target.SaveJsonFile();
        SelectedRepository.SelectedGame.UpdateTarget(target);
    }

    public void UpdateSaveData(SaveDataInfo savedata)
    {
        savedata.SaveJsonFile();
        SelectedRepository.SelectedGame.UpdateSaveData(savedata);
    }

    public async Task<bool> CopySource(string dirPath)
    {
        return await SelectedRepository.SelectedGame.CopySource(dirPath);
    }

    public async Task<bool> CopyLocalization(string dirPath)
    {
        return await SelectedRepository.SelectedGame.CopyLocalization(dirPath);
    }

    public async Task DeleteGame(GameInfo game)
    {
        await SelectedRepository.DeleteGame(game);
    }

    public async Task DeleteSource(SourceInfo source)
    {
        await SelectedRepository.SelectedGame.DeleteSource(source);
    }

    public async Task DeleteLocalization(LocalizationInfo localizationInfo)
    {
        await SelectedRepository.SelectedGame.DeleteLocalization(localizationInfo);
    }

    public async Task DeleteTarget(TargetInfo target)
    {
        await SelectedRepository.SelectedGame.DeleteTarget(target);
    }

    public async Task DeleteSaveData(SaveDataInfo saveData)
    {
        await SelectedRepository.SelectedGame.DeleteSaveData(saveData);
    }
}
