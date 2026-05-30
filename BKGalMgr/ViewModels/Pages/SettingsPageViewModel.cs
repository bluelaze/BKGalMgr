using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.Helpers;
using BKGalMgr.Models;
using BKGalMgr.Services;
using BKGalMgr.Views;
using Mapster;

namespace BKGalMgr.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableObject
{
    public string BKGalMgrVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();

    [ObservableProperty]
    public partial ElementTheme AppTheme { get; set; }

    [ObservableProperty]
    public partial BackdropMaterial AppBackdropMaterial { get; set; }

    [ObservableProperty]
    public partial CompressionLevel ZipLevel { get; set; }

    [ObservableProperty]
    public partial bool AutoCropScreenshotBlackBorder { get; set; }

    [ObservableProperty]
    public partial SupportLanguages Language { get; set; }

    [ObservableProperty]
    public partial int LanguageIndex { get; set; } = -1;

    [ObservableProperty]
    public partial Dictionary<SupportLanguages, string> Languages { get; set; }

    [ObservableProperty]
    public partial string CheckForUpdatesContent { get; set; }

    public BangumiInfo Bangumi => _settings.Bangumi;
    public LocalEmulatorInfo LocalEmulator => _settings.LocalEmulator;
    public ThemeInfo CustomTheme => _settings.CustomTheme;

    private readonly SettingsDto _settings;
    private ThemeHelper _themeHelper;

    private UpdateService _updateService;

    private bool _isInit = false;

    public SettingsPageViewModel(MainWindow mainWindow, SettingsDto settings, UpdateService updateService)
    {
        _themeHelper = new(mainWindow);

        _settings = settings;
        _settings.Adapt(this);

        _updateService = updateService;
        _updateService.UpdateStatusChanged = (UpdateStatus updateStatus, string message) =>
        {
            CheckForUpdatesContent = message;
        };
        CheckForUpdatesContent = _updateService.GetStatusMessage("");
        Languages = LanguageHelper.GetSupportLangugesName();
        foreach (var language in Languages)
        {
            LanguageIndex++;
            if (language.Key == Language)
                break;
        }

        if (LocalEmulator.LEProcPath.IsNullOrEmpty())
        {
            LocalEmulator.LEProcPath = LocaleEmulatorHelper.GetLEProcInstalledPath();
        }
        LocalEmulatorLoadProfiles();

        _isInit = true;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!_isInit)
            return;

        ApplySettings();
        if (e.PropertyName == nameof(AppTheme) || e.PropertyName == nameof(AppBackdropMaterial))
        {
            ApplyTheme();
        }
        else if (e.PropertyName == nameof(LanguageIndex))
        {
            Language = Languages.ToList().ElementAt(LanguageIndex).Key;
            _settings.Language = Language;
            ApplyLanguage(_settings);
        }
    }

    [RelayCommand]
    public void ApplySettings()
    {
        this.Adapt(_settings);
        _settings.SaveSettings();
    }

    public void ApplyTheme()
    {
        _themeHelper.ApllyTheme(_settings.AppTheme, _settings.AppBackdropMaterial);
    }

    public static void ApplyLanguage(SettingsDto settings)
    {
        if (settings.Language == SupportLanguages.system)
            return;
        var lang = settings.Language.ToString().Replace('_', '-');
        Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = lang;
    }

    public async Task CheckForUpdates()
    {
        await _updateService.CheckForUpdates();
    }

    [RelayCommand]
    public void LocalEmulatorEditGlobal()
    {
        if (LocalEmulator.LEProcPath.IsNullOrEmpty())
            return;
        LocaleEmulatorHelper.ManageAll(LocalEmulator.LEProcPath);
    }

    [RelayCommand]
    public void LocalEmulatorLoadProfiles()
    {
        if (LocalEmulator.LEProcPath.IsNullOrEmpty())
            return;
        LocalEmulator.Profiles = LocaleEmulatorHelper.GetProfiles(LocalEmulator.LEProcPath);
    }
}
