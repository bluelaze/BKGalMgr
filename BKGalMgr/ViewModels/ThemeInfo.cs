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
    public partial CustomThemeType ThemeType { get; set; } = CustomThemeType.Default;

    [JsonIgnore]
    public int ThemeTypeValue => ((int)ThemeType);

    [ObservableProperty]
    public partial ElementTheme RequestedTheme { get; set; } = ElementTheme.Dark;

    [ObservableProperty]
    public partial bool LastScreenshotAsBackground { get; set; } = true;

    [ObservableProperty]
    public partial bool AutomaticImageThemeType { get; set; } = true;

    [ObservableProperty]
    public partial bool HideReturn { get; set; } = false;

    [ObservableProperty]
    public partial bool HideCover { get; set; } = false;

    [ObservableProperty]
    public partial bool HideGameInfo { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundImageSource))]
    public partial string BackgroundImage { get; set; }

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
    public partial double BackgroundImageOpacity { get; set; } = 1;

    [ObservableProperty]
    public partial string LinearGradientStartColor { get; set; } = "#FF575757";

    [ObservableProperty]
    public partial string LinearGradientEndColor { get; set; } = "#FF575757";

    public ThemeInfo() { }

    public async Task SetBackgroundImageAsync(string imagePath)
    {
        BackgroundImage = imagePath;
        if (AutomaticImageThemeType)
        {
            ThemeType = CustomThemeType.Image;
            HideReturn = true;
            HideCover = true;
            RequestedTheme = await Task.Run(
                () => ColorHelper.IsDarkImage(imagePath) ? ElementTheme.Dark : ElementTheme.Light
            );
        }
    }
}
