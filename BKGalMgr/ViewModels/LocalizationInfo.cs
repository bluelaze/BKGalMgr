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
    private string _webaddress;
    [ObservableProperty]
    private string _description;
    [ObservableProperty]
    private ObservableCollection<ContributorInfo> _contributors;

    [property: JsonIgnore]
    public string ZipPath => Path.Combine(Path.GetDirectoryName(JsonPath), GlobalInfo.LocalizationZipName);

    public LocalizationInfo() { }

    public bool IsValid()
    {
        return !Name.IsNullOrEmpty() && !JsonPath.IsNullOrEmpty() && !StartupName.IsNullOrEmpty();
    }

    public static LocalizationInfo Open(string dirPath)
    {
        var path = Path.Combine(dirPath, GlobalInfo.LocalizationJsonName);
        if (!File.Exists(path))
            return null;

        var localizationInfo = JsonSerializer.Deserialize<LocalizationInfo>(File.ReadAllBytes(path));
        localizationInfo.JsonPath = path;

        return localizationInfo;
    }

    public void SetGamePath(string dirPath)
    {
        JsonPath = Path.Combine(
            dirPath, GlobalInfo.LocalizationsFolderName, CreateDate.ToString(GlobalInfo.FolderFormatStr), GlobalInfo.LocalizationJsonName);
    }

    public void SaveJsonFile()
    {
        string jsonStr = JsonMisc.Serialize(this);
        Directory.CreateDirectory(Path.GetDirectoryName(JsonPath));
        File.WriteAllText(JsonPath, jsonStr);
    }

    [RelayCommand]
    public void OpenJsonFolder()
    {
        Process.Start("explorer", Path.GetDirectoryName(JsonPath));
    }

    public async Task CompressLocalizationFolder(string localizationFolderPath)
    {
        if (!Directory.Exists(localizationFolderPath))
            return;
        await Task.Run(() =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ZipPath));
            ZipFile.CreateFromDirectory(localizationFolderPath, ZipPath);
            SaveJsonFile();
        });
    }
}
