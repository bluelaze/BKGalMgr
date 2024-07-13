using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BKGalMgr.Models;

namespace BKGalMgr.ViewModels.Pages;

public partial class GamesManagePageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<RepositoryInfo> _repository = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedRepositoryIsValid))]
    [property: JsonIgnore]
    private RepositoryInfo _selectedRepository;

    partial void OnSelectedRepositoryChanged(RepositoryInfo value)
    {
        _settings.SelectedRepositoryPath = SelectedRepository?.FolderPath ?? "";
    }

    public bool SelectedRepositoryIsValid
    {
        get { return SelectedRepository != null; }
    }

    private readonly SettingsDto _settings;

    public GamesManagePageViewModel()
    {
        _settings = App.GetRequiredService<SettingsDto>();
        foreach (var path in _settings.RepositoryPath)
        {
            AddRepository(new RepositoryInfo() { FolderPath = path });
        }
    }

    public bool AddRepository(RepositoryInfo repository)
    {
        if (repository.FolderPath.IsNullOrEmpty())
            return false;

        repository = RepositoryInfo.Open(repository.FolderPath, repository);
        if (repository == null)
            return false;

        Repository.Add(repository);
        if (repository.FolderPath == _settings.SelectedRepositoryPath)
            SelectedRepository = repository;
        if (!_settings.RepositoryPath.Contains(repository.FolderPath))
        {
            _settings.RepositoryPath.Add(repository.FolderPath);
            _settings.SaveSettings();
        }
        return true;
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
}
