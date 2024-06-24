using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.Helpers;
using BKGalMgr.Models;
using Mapster;

namespace BKGalMgr.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private Theme _appTheme;

    [ObservableProperty]
    private BackdropMaterial _appBackdropMaterial;

    [ObservableProperty]
    private CompressionLevel _zipLevel;

    [ObservableProperty]
    private SupportLanguages _language;

    [ObservableProperty]
    private int _languageIndex = -1;

    [ObservableProperty]
    private Dictionary<SupportLanguages, string> _languages;

    private readonly SettingsDto _settings;
    private ThemeHelper _themeHelper;
    private bool _isInit = false;

    public SettingsPageViewModel()
    {
        _themeHelper = new(App.MainWindow);

        _settings = App.GetRequiredService<SettingsDto>();
        _settings.Adapt(this);

        _languages = LanguageHelper.GetSupportLangugesName();
        foreach (var language in _languages)
        {
            LanguageIndex++;
            if (language.Key == _language)
                break;
        }

        _isInit = true;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!_isInit)
            return;

        this.Adapt(_settings);
        if (e.PropertyName == nameof(AppTheme) || e.PropertyName == nameof(AppBackdropMaterial))
        {
            ApplyTheme();
        }
        else if (e.PropertyName == nameof(LanguageIndex))
        {
            Language = Languages.ToList().ElementAt(LanguageIndex).Key;
            // TODO: change language
        }
    }

    public void ApplyTheme()
    {
        _themeHelper.ApllyTheme(_settings.AppTheme, _settings.AppBackdropMaterial);
    }
}
