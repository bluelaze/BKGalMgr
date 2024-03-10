using BKGalMgr.Models;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableObject
{
    public Theme AppTheme
    {
        get
        {
            return _settings.LoadedSettings.AppTheme;
        }
        set
        {
            if (SetProperty(
                _settings.LoadedSettings.AppTheme, value, (newValue) => { _settings.LoadedSettings.AppTheme = newValue; }))
            {
                ApplyTheme();
            }
        }
    }

    public BackdropMaterial AppBackdropMaterial
    {
        get
        {
            return _settings.LoadedSettings.AppBackdropMaterial;
        }
        set
        {
            if (SetProperty(
                _settings.LoadedSettings.AppBackdropMaterial, value, (newValue) => { _settings.LoadedSettings.AppBackdropMaterial = newValue; }))
            {
                ApplyTheme();
            }
        }
    }

    public CompressionLevel ZipLevel
    {
        get
        {
            return _settings.LoadedSettings.ZipLevel;
        }
        set
        {
            if (SetProperty(
                _settings.LoadedSettings.ZipLevel, value, (newValue) => { _settings.LoadedSettings.ZipLevel = newValue; }))
            {
            }
        }
    }

    private SettingsModel _settings;
    public SettingsPageViewModel()
    {
        _settings = App.GetRequiredService<SettingsModel>();
    }

    public void ApplyTheme()
    {
        switch (_settings.LoadedSettings.AppTheme)
        {
            case Theme.Dark:
                App.MainWindow.ApplyTheme(ElementTheme.Dark);
                break;
            case Theme.Light:
                App.MainWindow.ApplyTheme(ElementTheme.Light);
                break;
            default:
                App.MainWindow.ApplyTheme(ElementTheme.Default);
                break;
        }
        switch (_settings.LoadedSettings.AppBackdropMaterial)
        {
            case BackdropMaterial.Mica:
                App.MainWindow.SystemBackdrop = new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base };
                break;
            case BackdropMaterial.Mica_Alt:
                App.MainWindow.SystemBackdrop = new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };
                break;
            default:
                App.MainWindow.SystemBackdrop = new DesktopAcrylicBackdrop();
                break;
        }
    }
}
