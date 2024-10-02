using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using BKGalMgr.Helpers;
using BKGalMgr.ViewModels;
using Mapster;

namespace BKGalMgr.Models;

public class SettingsDto
{
    // settings
    public List<string> RepositoryPath { get; set; }
    public string SelectedRepositoryPath { get; set; }
    public Theme AppTheme { get; set; }
    public BackdropMaterial AppBackdropMaterial { get; set; }
    public CompressionLevel ZipLevel { get; set; }
    public SupportLanguages Language { get; set; }
    public BangumiInfo Bangumi { get; set; }

    // dto
    private readonly Settings _settings;
    private string _settingsFilePath => Path.Combine(Directory.GetCurrentDirectory(), "BKGalMgrSettings.json");

    public SettingsDto()
    {
        _settings = LoadSettings();
        _settings.Adapt(this);
    }

    private Settings LoadSettings()
    {
        if (_settings != null)
            return _settings;

        try
        {
            return JsonMisc.Deserialize<Settings>(File.ReadAllText(_settingsFilePath));
        }
        catch (Exception e)
        {
            Debug.Print(e.ToString());
        }
        return new();
    }

    public void SaveSettings()
    {
        this.Adapt(_settings);
        File.WriteAllText(_settingsFilePath, JsonMisc.Serialize(_settings));
    }
}
