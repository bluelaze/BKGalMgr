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
    public partial string Name { get; set; }

    [ObservableProperty]
    [JsonIgnore]
    public partial string JsonPath { get; set; }

    [ObservableProperty]
    public partial string StartupName { get; set; }

    [ObservableProperty]
    public partial DateTime CreateDate { get; set; } = DateTime.Now;

    [ObservableProperty]
    public partial string Description { get; set; }

    [ObservableProperty]
    public partial bool EnableLocalEmulator { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ContributorInfo> Contributors { get; set; } = new();

    [JsonIgnore]
    public string FolderPath => Path.GetDirectoryName(JsonPath);

    [JsonIgnore]
    public string ZipPath => Path.Combine(FolderPath, GlobalInfo.SourceZipName);

    public SourceInfo() { }

    public static SourceInfo Open(string dirPath)
    {
        var path = Path.Combine(dirPath, GlobalInfo.SourceJsonName);
        if (!File.Exists(path))
            return null;

        var sourceInfo = JsonMisc.Deserialize<SourceInfo>(File.ReadAllText(path));
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
        JsonPath = Path.Combine(
            dirPath,
            GlobalInfo.SourcesFolderName,
            CreateDate.ToString(GlobalInfo.FolderFormatStr),
            GlobalInfo.SourceJsonName
        );
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

        var ret = await FileSystemMisc.CreateZipFromDirectoryAsync(sourceFolderPath, ZipPath, App.ZipLevel());
        if (!ret.success)
        {
            App.ShowErrorMessage(ret.message);
            return;
        }
        SaveJsonFile();
    }

    public async Task CopySourceToFolder(string targetFolderPath)
    {
        await FileSystemMisc.CopyFileAsync(JsonPath, Path.Combine(targetFolderPath, GlobalInfo.SourceJsonName));
        await FileSystemMisc.CopyFileAsync(ZipPath, Path.Combine(targetFolderPath, GlobalInfo.SourceZipName));
    }
}
