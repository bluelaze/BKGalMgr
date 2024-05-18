using BKGalMgr.Helpers;
using BKGalMgr.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace BKGalMgr.Models;

public class SettingsModel
{

    [Serializable]
    public class Settings
    {
        public List<string> RepositoryPath { get; set; } = new();
        public string SelectedRepositoryPath { get; set; }

        public Theme AppTheme { get; set; } = Theme.Light;

        public BackdropMaterial AppBackdropMaterial { get; set; } = BackdropMaterial.Mica;

        public CompressionLevel ZipLevel { get; set; } = CompressionLevel.NoCompression;

        public Settings()
        {
        }
    }
    private Settings _settings;
    public Settings LoadedSettings => _settings;

    private string _settingsFilePath => Path.Combine(Directory.GetCurrentDirectory(), "BKGalMgrSettings.json");

    public SettingsModel()
    {
        LoadSettings();
    }

    public Settings LoadSettings()
    {
        if (_settings == null)
        {
            try
            {
                _settings = JsonMisc.Deserialize<Settings>(File.ReadAllText(_settingsFilePath));
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
            }
            if (_settings == null)
                _settings = new();
        }
        return _settings;
    }

    public void SaveSettings()
    {
        File.WriteAllText(_settingsFilePath, JsonMisc.Serialize(_settings));
    }
}
