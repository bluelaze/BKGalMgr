using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels;

[Serializable]
public partial class RepositoryInfo : ObservableObject
{
    [ObservableProperty]
    private string _name;
    [ObservableProperty]
    [property: JsonIgnore]
    private string _folderPath;
    [ObservableProperty]
    private DateTime _createDate = DateTime.Now;
    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<GameInfo> _games = new();

    private GameInfo _selectedGame;
    [property: JsonIgnore]
    public GameInfo SelectedGame
    {
        get { return _selectedGame; }
        set
        {
            if (SetProperty(ref _selectedGame, value))
            {
                SeletedGameCreateDate = _selectedGame?.CreateDate;
                OnPropertyChanged(nameof(SelectedGameIsValid));
                SaveJsonFile();
            }
        }
    }
    [property: JsonIgnore]
    public bool SelectedGameIsValid { get { return _selectedGame != null; } }
    public DateTime? SeletedGameCreateDate { get; set; }

    [property: JsonIgnore]
    private string JsonPath => Path.Combine(FolderPath, GlobalInfo.RepositoryJsonName);

    public RepositoryInfo() { }

    public static bool IsExistedRepository(string folderPath)
    {
        return Path.Exists(Path.Combine(folderPath, GlobalInfo.RepositoryJsonName));
    }

    public static RepositoryInfo Open(string folderPath, RepositoryInfo defaultValue)
    {
        if (!Path.Exists(folderPath))
            return null;

        var repositoryInfo = defaultValue;
        var jsonPath = Path.Combine(folderPath, GlobalInfo.RepositoryJsonName);
        if (IsExistedRepository(folderPath))
            repositoryInfo = JsonSerializer.Deserialize<RepositoryInfo>(File.ReadAllBytes(jsonPath));
        if (repositoryInfo == null)
            repositoryInfo = defaultValue == null ? new RepositoryInfo() : defaultValue;

        repositoryInfo.FolderPath = folderPath;

        var dirs = Directory.GetDirectories(folderPath);
        foreach (var dir in dirs)
        {
            var game = GameInfo.Open(dir);
            if (game != null)
            {
                repositoryInfo.Games.Add(game);
                if (game.CreateDate == repositoryInfo.SeletedGameCreateDate)
                    repositoryInfo._selectedGame = game;
            }
        }
        repositoryInfo.SaveJsonFile();
        return repositoryInfo;
    }

    public bool IsValid()
    {
        return !Name.IsNullOrEmpty() && !FolderPath.IsNullOrEmpty();
    }

    public GameInfo NewGame()
    {
        var gameInfo = new GameInfo();
        gameInfo.SetRepositoryPath(FolderPath);

        return gameInfo;
    }

    public void AddGame(GameInfo game)
    {
        Games.Add(game);
    }

    public async Task DeleteGame(GameInfo game)
    {
        if (Games.Contains(game))
        {
            if(Directory.Exists(game.FolderPath))
                await Task.Run(() => { Directory.Delete(game.FolderPath, true); });
            Games.Remove(game);
        }
    }

    public void SaveJsonFile()
    {
        string jsonStr = JsonMisc.Serialize(this);
        Directory.CreateDirectory(Path.GetDirectoryName(JsonPath));
        File.WriteAllText(JsonPath, jsonStr);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenJsonFolder()
    {
        Process.Start("explorer", Path.GetDirectoryName(JsonPath));
    }
}
