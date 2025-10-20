using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using BKGalMgr.Converters;
using BKGalMgr.Models.Bangumi;
using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BKGalMgr.Views.Controls;

public sealed partial class ThemeInfoControl : UserControl
{
    public bool IsGameMode
    {
        get { return (bool)GetValue(IsGameModeProperty); }
        set { SetValue(IsGameModeProperty, value); }
    }
    public static readonly DependencyProperty IsGameModeProperty = DependencyProperty.Register(
        nameof(IsGameMode),
        typeof(bool),
        typeof(BangumiSubjectControl),
        new PropertyMetadata(false)
    );

    public string ImageFolder
    {
        get { return (string)GetValue(ImageFolderProperty); }
        set { SetValue(ImageFolderProperty, value); }
    }
    public static readonly DependencyProperty ImageFolderProperty = DependencyProperty.Register(
        nameof(ImageFolder),
        typeof(string),
        typeof(BangumiSubjectControl),
        new PropertyMetadata(default(string))
    );

    public ThemeInfoControl()
    {
        InitializeComponent();

        LinearGradientStartColor_ColorPickerButton.ApplyTemplate();
        LinearGradientStartColor_ColorPickerButton.ColorPicker.IsAlphaEnabled = true;
        LinearGradientEndColor_ColorPickerButton.ApplyTemplate();
        LinearGradientEndColor_ColorPickerButton.ColorPicker.IsAlphaEnabled = true;
    }

    private async void pick_background_image_Button_Click(object sender, RoutedEventArgs e)
    {
        if (ImageFolder.IsNullOrEmpty())
        {
            Windows.Storage.StorageFile file = await FileSystemMisc.PickFile(
                GlobalInfo.GameCoverSupportFormats.ToList()
            );
            if (file != null)
                pick_background_image_TextBox.Text = file.Path;
        }
        else
        {
            var files = FileSystemMisc.PickFile(ImageFolder, new() { "Image Files|*.*" });
            if (files?.Any() == true)
            {
                pick_background_image_TextBox.Text = files.First();
            }
        }
    }

    private void switch_color_Button_Click(object sender, RoutedEventArgs e)
    {
        var tempColor = LinearGradientStartColor_ColorPickerButton.SelectedColor;
        LinearGradientStartColor_ColorPickerButton.SelectedColor =
            LinearGradientEndColor_ColorPickerButton.SelectedColor;
        LinearGradientEndColor_ColorPickerButton.SelectedColor = tempColor;
    }
}
