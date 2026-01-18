using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BKGalMgr.Helpers;

namespace BKGalMgr.ViewModels;

public enum CustomThemeType
{
    Default,
    Image,
    LinearGradient,
}

public partial class ThemeInfo : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeTypeValue))]
    private CustomThemeType _themeType = CustomThemeType.Default;

    [JsonIgnore]
    public int ThemeTypeValue => ((int)ThemeType);

    [ObservableProperty]
    private ElementTheme _requestedTheme = ElementTheme.Dark;

    [ObservableProperty]
    private bool _lastScreenshotAsBackground = true;

    [ObservableProperty]
    private bool _hideReturn = false;

    [ObservableProperty]
    private bool _hideCover = false;

    [ObservableProperty]
    private bool _hideGameInfo = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundImageSource))]
    private string _backgroundImage;

    // Resource里不能用Banding
    private Microsoft.UI.Xaml.Media.Imaging.BitmapImage _backgroundImageSource;

    [JsonIgnore]
    public Microsoft.UI.Xaml.Media.Imaging.BitmapImage BackgroundImageSource
    {
        get
        {
            if (BackgroundImage.IsNullOrEmpty())
                return null;

            if (_backgroundImageSource == null)
                _backgroundImageSource = new();

            _backgroundImageSource.UriSource = new(BackgroundImage);
            return _backgroundImageSource;
        }
    }

    [ObservableProperty]
    private double _backgroundImageOpacity = 1;

    [ObservableProperty]
    private string _linearGradientStartColor = "#FF575757";

    [ObservableProperty]
    private string _linearGradientEndColor = "#FF575757";
}
