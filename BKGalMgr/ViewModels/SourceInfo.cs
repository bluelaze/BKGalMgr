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
    private string _webaddress;
    [ObservableProperty]
    private string _description;
    [ObservableProperty]
    private ObservableCollection<ContributorInfo> _contributors = new();

    [property: JsonIgnore]
    public string ZipPath => Path.Combine(Path.GetDirectoryName(JsonPath), GlobalInfo.SourceZipName);

    public SourceInfo() { }

    public static SourceInfo Open(string dirPath)
    {
        var path = Path.Combine(dirPath, GlobalInfo.SourceJsonName);
        if (!File.Exists(path))
            return null;

        var sourceInfo = JsonSerializer.Deserialize<SourceInfo>(File.ReadAllBytes(path));
        sourceInfo.JsonPath = path;

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
        Directory.CreateDirectory(Path.GetDirectoryName(JsonPath));
        File.WriteAllText(JsonPath, jsonStr);
    }

    [RelayCommand]
    public void OpenJsonFolder()
    {
        Process.Start("explorer", Path.GetDirectoryName(JsonPath));
    }

    public async Task CompressSourceFolder(string sourceFolderPath)
    {
        if (!Directory.Exists(sourceFolderPath))
            return;
        await Task.Run(() =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ZipPath));
            ZipFile.CreateFromDirectory(sourceFolderPath, ZipPath, CompressionLevel.NoCompression, false);
            SaveJsonFile();
        });
    }
}
