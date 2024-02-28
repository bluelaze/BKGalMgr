using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
using BKGalMgr.Views.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BKGalMgr.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ManagePage : Page
{
    private GamesManagePageViewModel _viewModel;
    public GamesManagePageViewModel ViewModel { get { return _viewModel; } }
    public ManagePage()
    {
        _viewModel = App.GetRequiredService<GamesManagePageViewModel>();
        DataContext = this;
        this.InitializeComponent();
    }

    private async void button_add_repository_Click(object sender, RoutedEventArgs e)
    {
        RepositoryInfo newRepository = new();
        ContentDialog dialog = new ContentDialog()
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = this.XamlRoot,
            Title = "Add new repository",
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = new RepositoryInfoControl()
            {
                Width = 720,
                DataContext = newRepository
            }
        };
        dialog.Resources["ContentDialogMaxWidth"] = 1080;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !newRepository.IsValid();
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.AddRepository(newRepository);
        }
    }

    private void button_add_game_Click(object sender, RoutedEventArgs e)
    {
        GameInfo newGame = ViewModel.SelectedRepository.NewGame();
        ViewModel.SelectedRepository.AddGame(newGame);
        ViewModel.SelectedRepository.SelectedGame = newGame;
    }

    private async void button_add_source_Click(object sender, RoutedEventArgs e)
    {
        SourceInfo sourceInfo = ViewModel.SelectedRepository.SelectedGame.NewSource();
        SourceInfoControl sourceInfoControl = new()
        {
            Width = 720,
            DataContext = sourceInfo
        };
        ContentDialog dialog = new()
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = this.XamlRoot,
            Title = "Add new source",
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = sourceInfoControl
        };
        // https://github.com/microsoft/microsoft-ui-xaml/issues/424
        dialog.Resources["ContentDialogMaxWidth"] = 1080;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !sourceInfoControl.IsValidSource();
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            App.ShowLoading();
            await ViewModel.SelectedRepository.SelectedGame.AddSource(sourceInfoControl.FolderPath, sourceInfo);
            App.HideLoading();
        }
    }

    private async void button_add_localization_Click(object sender, RoutedEventArgs e)
    {
        LocalizationInfo localizationInfo = ViewModel.SelectedRepository.SelectedGame.NewLocalization();
        LocalizationInfoControl localizationInfoControl = new()
        {
            Width = 720,
            DataContext = localizationInfo
        };
        ContentDialog dialog = new()
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = this.XamlRoot,
            Title = "Add new localization",
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = localizationInfoControl
        };
        // https://github.com/microsoft/microsoft-ui-xaml/issues/424
        dialog.Resources["ContentDialogMaxWidth"] = 1080;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !localizationInfoControl.IsValidLocalization();
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            App.ShowLoading();
            await ViewModel.SelectedRepository.SelectedGame.AddLocalization(localizationInfoControl.FolderPath, localizationInfo);
            App.HideLoading();
        }
    }

    private async void button_add_target_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = ViewModel.SelectedRepository.SelectedGame.NewTarget();
        TargetInfoControl targetInfoControl = new()
        {
            Width = 720,
            DataContext = targetInfo
        };
        ContentDialog dialog = new()
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = this.XamlRoot,
            Title = "Add new target",
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = targetInfoControl
        };
        // https://github.com/microsoft/microsoft-ui-xaml/issues/424
        dialog.Resources["ContentDialogMaxWidth"] = 1080;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !targetInfoControl.IsValidTarget();
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            App.ShowLoading();
            await ViewModel.SelectedRepository.SelectedGame.AddTarget(targetInfo);
            App.HideLoading();
        }
    }

    private async void menuflyoutitem_edit_repository_Click(object sender, RoutedEventArgs e)
    {
        RepositoryInfo editRepository = ViewModel.SelectedRepository;
        ContentDialog dialog = new ContentDialog()
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = this.XamlRoot,
            Title = "Edit repository",
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = new RepositoryInfoControl()
            {
                Width = 720,
                DataContext = editRepository,
                FolderPathVisible = false
            }
        };
        dialog.Resources["ContentDialogMaxWidth"] = 1080;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !editRepository.IsValid();
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            editRepository.SaveJsonFile();
        }
    }

    private async void menuflyoutitem_edit_source_Click(object sender, RoutedEventArgs e)
    {
        SourceInfo sourceInfo = (sender as MenuFlyoutItem).DataContext as SourceInfo;
        var editSourceInfo = SourceInfo.Open(Path.GetDirectoryName(sourceInfo.JsonPath));
        SourceInfoControl sourceInfoControl = new()
        {
            Width = 720,
            DataContext = editSourceInfo,
            FolderPathVisible = false
        };
        ContentDialog dialog = new()
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = this.XamlRoot,
            Title = "Edit source",
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = sourceInfoControl
        };
        // https://github.com/microsoft/microsoft-ui-xaml/issues/424
        dialog.Resources["ContentDialogMaxWidth"] = 1080;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !sourceInfoControl.IsValidSource();
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.UpdateSource(editSourceInfo);
        }
    }

    private async void menuflyoutitem_edit_localization_Click(object sender, RoutedEventArgs e)
    {
        LocalizationInfo localizationInfo = (sender as MenuFlyoutItem).DataContext as LocalizationInfo;
        var editLocalizationInfo = LocalizationInfo.Open(Path.GetDirectoryName(localizationInfo.JsonPath));
        LocalizationInfoControl localizationInfoControl = new()
        {
            Width = 720,
            DataContext = editLocalizationInfo,
            FolderPathVisible = false
        };
        ContentDialog dialog = new()
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = this.XamlRoot,
            Title = "Edit localization",
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = localizationInfoControl
        };
        // https://github.com/microsoft/microsoft-ui-xaml/issues/424
        dialog.Resources["ContentDialogMaxWidth"] = 1080;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !localizationInfoControl.IsValidLocalization();
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.UpdateLocalization(editLocalizationInfo);
        }
    }

    private async void menuflyoutitem_edit_target_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = (sender as MenuFlyoutItem).DataContext as TargetInfo;
        var editTargetInfo = TargetInfo.Open(Path.GetDirectoryName(targetInfo.JsonPath));
        editTargetInfo.Game = ViewModel.SelectedRepository.SelectedGame;
        editTargetInfo.Localization = ViewModel.SelectedRepository.SelectedGame.FindLocalization(targetInfo.Localization);
        TargetInfoControl targetInfoControl = new()
        {
            Width = 720,
            DataContext = editTargetInfo,
            SourcesVisible = false,
        };
        ContentDialog dialog = new()
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = this.XamlRoot,
            Title = "Edit target",
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = targetInfoControl
        };
        // https://github.com/microsoft/microsoft-ui-xaml/issues/424
        dialog.Resources["ContentDialogMaxWidth"] = 1080;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !targetInfoControl.IsValidTarget();
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            // replace new localization
            if (editTargetInfo.Localization != null && editTargetInfo.Localization.CreateDate != targetInfo.Localization.CreateDate)
            {
                App.ShowLoading();
                await editTargetInfo.DecompressLocalization();
                App.HideLoading();
            }
            else
            {
                editTargetInfo.Localization = targetInfo.Localization;
            }

            ViewModel.UpdateTarget(editTargetInfo);
        }
    }

    private async void button_add_source_folder_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFile file = await FileSystemMisc.PickFile(new() { ".json" });
        if (file != null)
        {
            App.ShowLoading();
            var result = await ViewModel.CopySource(file.Path);
            App.HideLoading();
            if (!result)
                App.ShowDialogError("Invalid source");
        }
    }

    private async void button_add_localization_folder_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFile file = await FileSystemMisc.PickFile(new() { ".json" });
        if (file != null)
        {
            App.ShowLoading();
            var result = await ViewModel.CopyLocalization(file.Path);
            App.HideLoading();
            if (!result)
                App.ShowDialogError("Invalid localization");
        }
    }

    private async void menuflyoutitem_export_target_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = (sender as MenuFlyoutItem).DataContext as TargetInfo;
        Windows.Storage.StorageFolder folder = await FileSystemMisc.PickFolder(new() { "*" });
        if (folder != null)
        {
            App.ShowLoading();
            await targetInfo.CopyTargetAsSourceToFolder(folder.Path);
            App.HideLoading();
        }
    }
}
