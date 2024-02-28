using BKGalMgr.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels.Pages;

public partial class GamesManagePageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<RepositoryInfo> _repository = new();

    private RepositoryInfo _selectedRepository;
    [property: JsonIgnore]
    public RepositoryInfo SelectedRepository
    {
        get { return _selectedRepository; }
        set
        {
            if (SetProperty(ref _selectedRepository, value))
            {
                _settings.LoadedSettings.SelectedRepositoryPath = _selectedRepository.FolderPath;
                OnPropertyChanged(nameof(SelectedRepositoryIsValid));
            }
        }
    }
    public bool SelectedRepositoryIsValid { get { return _selectedRepository != null; } }

    [ObservableProperty]
    private GameInfo _game = new();

    [ObservableProperty]
    private SourceInfo _source = new();

    private SettingsModel _settings;
    public GamesManagePageViewModel()
    {
        _settings = App.GetRequiredService<SettingsModel>();
        foreach (var path in _settings.LoadedSettings.RepositoryPath)
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
        if (repository.FolderPath == _settings.LoadedSettings.SelectedRepositoryPath)
            _selectedRepository = repository;
        if (!_settings.LoadedSettings.RepositoryPath.Contains(repository.FolderPath))
        {
            _settings.LoadedSettings.RepositoryPath.Add(repository.FolderPath);
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
}
