using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BKGalMgr.ViewModels;

public partial class SaveDataInfo : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    [property: JsonIgnore]
    private string _jsonPath;

    [ObservableProperty]
    private DateTime _createDate = DateTime.Now;

    [ObservableProperty]
    private string _description;

    [property: JsonIgnore]
    public string FolderPath => Path.GetDirectoryName(JsonPath);

    [property: JsonIgnore]
    public string ZipPath => Path.Combine(FolderPath, GlobalInfo.SaveDataZipName);

    public SaveDataInfo() { }

    public static SaveDataInfo Open(string dirPath)
    {
        var path = Path.Combine(dirPath, GlobalInfo.SaveDataJsonName);
        if (!File.Exists(path))
            return null;

        var savedataInfo = JsonMisc.Deserialize<SaveDataInfo>(File.ReadAllText(path));
        if (savedataInfo == null)
            return null;

        savedataInfo.JsonPath = path;
        if (!savedataInfo.IsValid())
            return null;

        if (!File.Exists(savedataInfo.ZipPath))
            return null;
        return savedataInfo;
    }

    public bool IsValid()
    {
        return !Name.IsNullOrEmpty() && !JsonPath.IsNullOrEmpty();
    }

    public void SetGamePath(string dirPath)
    {
        JsonPath = Path.Combine(
            dirPath,
            GlobalInfo.SaveDatasFolderName,
            CreateDate.ToString(GlobalInfo.FolderFormatStr),
            GlobalInfo.SaveDataJsonName
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

    public async Task<bool> BackupSaveDataFolder(string savedataFolderPath)
    {
        if (!Directory.Exists(savedataFolderPath))
            return false;
        return await Task.Run(() =>
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ZipPath));
                ZipFile.CreateFromDirectory(savedataFolderPath, ZipPath, App.ZipLevel(), false);
                SaveJsonFile();
                return true;
            }
            catch { }
            return false;
        });
    }

    public async Task<bool> RestoreToSaveDataFolder(string savedataFolderPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                // backup savedata
                var backupFolder = Path.Combine(
                    Path.GetDirectoryName(savedataFolderPath),
                    Path.GetFileName(savedataFolderPath) + "_backup"
                );
                if (Directory.Exists(backupFolder))
                    Directory.Delete(backupFolder, true);
                Directory.Move(savedataFolderPath, backupFolder);

                // overwrite files
                Directory.CreateDirectory(savedataFolderPath);
                ZipFile.ExtractToDirectory(ZipPath, savedataFolderPath, true);
                return true;
            }
            catch { }
            return false;
        });
    }
}
