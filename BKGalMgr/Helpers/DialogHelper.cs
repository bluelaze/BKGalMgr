using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.Views;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace BKGalMgr.Helpers;

public class DialogHelper
{
    public static ContentDialog GetConfirmDialog()
    {
        ContentDialog dialog = new ContentDialog();

        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
        dialog.RequestedTheme = App.MainWindow.RequestedTheme();
        dialog.PrimaryButtonText = LanguageHelper.GetString("Dlg_Confirm");
        dialog.CloseButtonText = LanguageHelper.GetString("Dlg_Cancel");
        dialog.DefaultButton = ContentDialogButton.Primary;
        // https://github.com/microsoft/microsoft-ui-xaml/issues/424
        dialog.Resources["ContentDialogMaxWidth"] = 1080;

        return dialog;
    }

    public static async Task ShowError(string errorMsg)
    {
        ContentDialog dialog = new ContentDialog();

        dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
        dialog.RequestedTheme = App.MainWindow.RequestedTheme();
        dialog.PrimaryButtonText = LanguageHelper.GetString("Dlg_Confirm");
        dialog.DefaultButton = ContentDialogButton.Primary;

        dialog.Content = errorMsg;
        dialog.Title = new FontIcon()
        {
            Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"],
            Glyph = "\uE783",
        };

        await dialog.ShowAsync();
    }

    public static async Task<bool> ShowConfirm(string confirmMsg)
    {
        ContentDialog dialog = GetConfirmDialog();

        dialog.Title = new FontIcon()
        {
            Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCautionBrush"],
            Glyph = "\uE7BA",
        };
        dialog.Content = confirmMsg;

        return ContentDialogResult.Primary == await dialog.ShowAsync();
    }
}
