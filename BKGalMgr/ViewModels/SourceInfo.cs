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
using Windows.Devices.Lights;

namespace BKGalMgr.ViewModels;

[Serializable]
public partial class SourceInfo : ObservableObject
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
    public string ZipPath => Path.Combine(FolderPath, GlobalInfo.SourceZipName);

    public SourceInfo() { }

    public static SourceInfo Open(string dirPath)
    {
        var path = Path.Combine(dirPath, GlobalInfo.SourceJsonName);
        if (!File.Exists(path))
            return null;

        var sourceInfo = JsonSerializer.Deserialize<SourceInfo>(File.ReadAllBytes(path));
        if (sourceInfo == null)
            return null;

        sourceInfo.JsonPath = path;
        if (!sourceInfo.IsValid())
            return null;

        if (!File.Exists(sourceInfo.ZipPath))
            return null;

        return sourceInfo;
    }

    public bool IsValid()
    {
        return !Name.IsNullOrEmpty() && !JsonPath.IsNullOrEmpty() && !StartupName.IsNullOrEmpty();
    }

    public void SetGamePath(string dirPath)
    {
        JsonPath = Path.Combine(dirPath, GlobalInfo.SourcesFolderName, CreateDate.ToString(GlobalInfo.FolderFormatStr), GlobalInfo.SourceJsonName);
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

    public async Task CompressSourceFolder(string sourceFolderPath)
    {
        if (!Directory.Exists(sourceFolderPath))
            return;
        await Task.Run(() =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ZipPath));
            ZipFile.CreateFromDirectory(sourceFolderPath, ZipPath, App.ZipLevel(), false);
            SaveJsonFile();
        });
    }

    public async Task CopySourceToFolder(string targetFolderPath)
    {
        await Task.Run(() =>
        {
            Directory.CreateDirectory(targetFolderPath);
            File.Copy(JsonPath, Path.Combine(targetFolderPath, GlobalInfo.SourceJsonName), true);
            File.Copy(ZipPath, Path.Combine(targetFolderPath, GlobalInfo.SourceZipName), true);
        });
    }
}
