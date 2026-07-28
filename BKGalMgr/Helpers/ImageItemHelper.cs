using System;
using System.Collections.Generic;
using System.Text;
using BKGalMgr.Interfaces;
using BKGalMgr.Models;

namespace BKGalMgr.Helpers;

// 考虑以后可能会出个全游戏截图浏览的功能，这样写会好点，就是搞得有点乱
public class ImageItemHelper : IImageItem
{
    public enum DeleteImageType
    {
        OnlyGame,
        OnlySystem,
        All,
    }

    public ImageItemHelper(IImageItem imageOwner, string image)
    {
        ImageOwner = imageOwner;
        Image = image;
    }

    public IImageItem ImageOwner { get; set; }

    public object Args { get; set; }
    public string Image { get; set; }

    public void DeleteImage()
    {
        ImageOwner.Args = Args;
        ImageOwner.Image = Image;
        ImageOwner.DeleteImage();
    }

    public void SetAsGameBackground()
    {
        ImageOwner.Args = Args;
        ImageOwner.Image = Image;
        ImageOwner.SetAsGameBackground();
    }

    public void SetAsAppBackground()
    {
        var settingsDto = App.GetRequiredService<SettingsDto>();
        settingsDto.CustomTheme.BackgroundImage = Image;
        settingsDto.CustomTheme.ThemeType = ViewModels.CustomThemeType.Image;
        App.GetRequiredService<SettingsDto>().SaveSettings();
    }
}
