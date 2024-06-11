using System;
using System.Collections.Generic;
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

    private readonly SettingsDto _settings;
    private ThemeHelper _themeHelper;
    private bool _isInit = false;

    public SettingsPageViewModel()
    {
        _themeHelper = new(App.MainWindow);

        _settings = App.GetRequiredService<SettingsDto>();
        _settings.Adapt(this);
        _isInit = true;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!_isInit)
            return;

        this.Adapt(_settings);
        if (e.PropertyName == nameof(AppTheme) || e.PropertyName == nameof(AppBackdropMaterial))
            ApplyTheme();
    }

    public void ApplyTheme()
    {
        _themeHelper.ApllyTheme(_settings.AppTheme, _settings.AppBackdropMaterial);
    }
}
