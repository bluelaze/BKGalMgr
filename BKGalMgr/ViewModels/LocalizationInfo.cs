using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels;

[Serializable]
public partial class LocalizationInfo : ObservableObject
{
    [ObservableProperty]
    private string _name;
    [ObservableProperty]
    [property: JsonIgnore]
    private string _jsonPath;
    [ObservableProperty]
    private string _startupName;
    [ObservableProperty]
    private DateTime _createDate = DateTime.Now;
    [ObservableProperty]
    private string _description;
    [ObservableProperty]
    private ObservableCollection<ContributorInfo> _contributors = new();

    [property: JsonIgnore]
    public string FolderPath => Path.GetDirectoryName(JsonPath);
    [property: JsonIgnore]
    public string ZipPath => Path.Combine(FolderPath, GlobalInfo.LocalizationZipName);

    public LocalizationInfo() { }

    public static LocalizationInfo Open(string dirPath)
    {
        var path = Path.Combine(dirPath, GlobalInfo.LocalizationJsonName);
        if (!File.Exists(path))
            return null;

        var localizationInfo = JsonMisc.Deserialize<LocalizationInfo>(File.ReadAllText(path));
        if (localizationInfo == null)
            return null;

        localizationInfo.JsonPath = path;
        if (!localizationInfo.IsValid())
            return null;


        if (!File.Exists(localizationInfo.ZipPath))
            return null;

        return localizationInfo;
    }

    public bool IsValid()
    {
        return !Name.IsNullOrEmpty() && !JsonPath.IsNullOrEmpty() && !StartupName.IsNullOrEmpty();
    }

    public void SetGamePath(string dirPath)
    {
        JsonPath = Path.Combine(
            dirPath, GlobalInfo.LocalizationsFolderName, CreateDate.ToString(GlobalInfo.FolderFormatStr), GlobalInfo.LocalizationJsonName);
    }

    public void SaveJsonFile()
    {
        string jsonStr = JsonMisc.Serialize(this);
        Directory.CreateDirectory(FolderPath);
        File.WriteAllText(JsonPath, jsonStr);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void OpenJsonFolder()
    {
        Process.Start("explorer", FolderPath);
    }

    public async Task CompressLocalizationFolder(string localizationFolderPath)
    {
        if (!Directory.Exists(localizationFolderPath))
            return;
        await Task.Run(() =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ZipPath));
            ZipFile.CreateFromDirectory(localizationFolderPath, ZipPath, App.ZipLevel(), false);
            SaveJsonFile();
        });
    }

    public async Task CopyLocalizationToFolder(string targetFolderPath)
    {
        await Task.Run(() =>
        {
            Directory.CreateDirectory(targetFolderPath);
            File.Copy(JsonPath, Path.Combine(targetFolderPath, GlobalInfo.LocalizationJsonName), true);
            File.Copy(ZipPath, Path.Combine(targetFolderPath, GlobalInfo.LocalizationZipName), true);
        });
    }
}
