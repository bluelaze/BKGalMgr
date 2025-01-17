using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BKGalMgr.ViewModels;

public partial class SaveDataSettingsInfo : ObservableObject
{
    [ObservableProperty]
    [property: JsonIgnore]
    private string _jsonPath;

    [ObservableProperty]
    private DateTime _createDate = DateTime.Now;

    [ObservableProperty]
    private string _saveDataFolderPath;

    [ObservableProperty]
    private bool _autoBackup = false;

    [ObservableProperty]
    private string _description;

    [property: JsonIgnore]
    public string FolderPath => Path.GetDirectoryName(JsonPath);

    public SaveDataSettingsInfo() { }

    public static SaveDataSettingsInfo Open(string dirPath)
    {
        var path = Path.Combine(dirPath, GlobalInfo.SaveDataSettingsJsonName);
        if (!File.Exists(path))
            return null;

        var savedataSettingsInfo = JsonMisc.Deserialize<SaveDataSettingsInfo>(File.ReadAllText(path));
        if (savedataSettingsInfo == null)
            return null;

        savedataSettingsInfo.JsonPath = path;
        if (!savedataSettingsInfo.IsValid())
            return null;

        return savedataSettingsInfo;
    }

    public bool IsValid()
    {
        return !SaveDataFolderPath.IsNullOrEmpty() && !JsonPath.IsNullOrEmpty();
    }

    public void SetGamePath(string dirPath)
    {
        JsonPath = Path.Combine(dirPath, GlobalInfo.SaveDatasFolderName, GlobalInfo.SaveDataSettingsJsonName);
    }

    public void SaveJsonFile()
    {
        string jsonStr = JsonMisc.Serialize(this);
        Directory.CreateDirectory(FolderPath);
        File.WriteAllText(JsonPath, jsonStr);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenSaveDataFolder()
    {
        Process.Start("explorer", SaveDataFolderPath);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenJsonFolder()
    {
        Process.Start("explorer", FolderPath);
    }
}
