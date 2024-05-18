using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.Helpers;
using BKGalMgr.Models;
using Microsoft.UI.Xaml.Media;

namespace BKGalMgr.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableObject
{
    public Theme AppTheme
    {
        get => _settings.LoadedSettings.AppTheme;
        set
        {
            if (
                SetProperty(
                    _settings.LoadedSettings.AppTheme,
                    value,
                    (newValue) =>
                    {
                        _settings.LoadedSettings.AppTheme = newValue;
                    }
                )
            )
            {
                ApplyTheme();
            }
        }
    }

    public BackdropMaterial AppBackdropMaterial
    {
        get => _settings.LoadedSettings.AppBackdropMaterial;
        set
        {
            if (
                SetProperty(
                    _settings.LoadedSettings.AppBackdropMaterial,
                    value,
                    (newValue) =>
                    {
                        _settings.LoadedSettings.AppBackdropMaterial = newValue;
                    }
                )
            )
            {
                ApplyTheme();
            }
        }
    }

    public CompressionLevel ZipLevel
    {
        get => _settings.LoadedSettings.ZipLevel;
        set
        {
            if (
                SetProperty(
                    _settings.LoadedSettings.ZipLevel,
                    value,
                    (newValue) =>
                    {
                        _settings.LoadedSettings.ZipLevel = newValue;
                    }
                )
            ) { }
        }
    }

    private SettingsModel _settings;
    private ThemeHelper _themeHelper;

    public SettingsPageViewModel()
    {
        _settings = App.GetRequiredService<SettingsModel>();
        _themeHelper = new(App.MainWindow);
    }

    public void ApplyTheme()
    {
        _themeHelper.ApllyTheme(_settings.LoadedSettings.AppTheme, _settings.LoadedSettings.AppBackdropMaterial);
    }
}
