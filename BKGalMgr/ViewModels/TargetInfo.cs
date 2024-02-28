﻿using System;
using System.Collections.Generic;
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
public partial class TargetInfo : ObservableObject
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
    private DateTime _lastPlayDate;
    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private GameInfo _game;
    [ObservableProperty]
    private SourceInfo _source;
    [ObservableProperty]
    private LocalizationInfo _localization;

    [property: JsonIgnore]
    public string TargetPath => Path.Combine(Path.GetDirectoryName(JsonPath), GlobalInfo.TargetName);
    [property: JsonIgnore]
    public string TargetExePath => Path.Combine(TargetPath, StartupName);

    public TargetInfo() { }

    public static TargetInfo Open(string dirPath)
    {
        var path = Path.Combine(dirPath, GlobalInfo.TargetJsonName);
        if (!File.Exists(path))
            return null;

        var targetInfo = JsonSerializer.Deserialize<TargetInfo>(File.ReadAllBytes(path));
        targetInfo.JsonPath = path;

        return targetInfo;
    }

    public bool IsValid()
    {
        return !Name.IsNullOrEmpty() && !JsonPath.IsNullOrEmpty() && !StartupName.IsNullOrEmpty() && Source != null;
    }

    public void SetGamePath(string dirPath)
    {
        JsonPath = Path.Combine(dirPath, GlobalInfo.TargetsFolderName, CreateDate.ToString(GlobalInfo.FolderFormatStr), GlobalInfo.TargetJsonName);
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

    public async Task DecompressLocalization()
    {
        if (Localization == null || !File.Exists(Localization.ZipPath))
            return;

        await Task.Run(() =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(TargetPath));
            ZipFile.ExtractToDirectory(Localization.ZipPath, TargetPath, true);
        });
    }

    public async Task DecompressSourceAndLocalization()
    {
        if (Source == null || !File.Exists(Source.ZipPath))
            return;

        await Task.Run(async () =>
        {
            // dezip source
            Directory.CreateDirectory(Path.GetDirectoryName(TargetPath));
            ZipFile.ExtractToDirectory(Source.ZipPath, TargetPath, true);

            // dezip target
            await DecompressLocalization();

            SaveJsonFile();
        });
    }
}
