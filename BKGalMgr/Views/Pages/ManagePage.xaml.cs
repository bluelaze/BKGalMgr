using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using BKGalMgr.Helpers;
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
    public GamesManagePageViewModel ViewModel
    {
        get { return _viewModel; }
    }

    public ManagePage()
    {
        _viewModel = App.GetRequiredService<GamesManagePageViewModel>();
        DataContext = this;
        this.InitializeComponent();
    }

    private async void add_repository_button_Click(object sender, RoutedEventArgs e)
    {
        RepositoryInfo newRepository = new();
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = LanguageHelper.GetString("Dlg_Repository_New"),
            PrimaryButtonText = LanguageHelper.GetString("Dlg_Add"),
            CloseButtonText = LanguageHelper.GetString("Dlg_Cancel"),
            DefaultButton = ContentDialogButton.Primary,
            Content = new RepositoryInfoControl() { Width = 720, DataContext = newRepository },
            RequestedTheme = App.MainWindow.RequestedTheme(),
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

    private void add_game_button_Click(object sender, RoutedEventArgs e)
    {
        GameInfo newGame = ViewModel.SelectedRepository.NewGame();
        ViewModel.SelectedRepository.AddGame(newGame);
        ViewModel.SelectedRepository.SelectedGame = newGame;
    }

    private async void add_target_button_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = ViewModel.SelectedRepository.SelectedGame.NewTarget();

        var result = await EditTargetInfo(targetInfo);
        if (result == ContentDialogResult.Primary)
        {
            App.ShowLoading();
            await ViewModel.SelectedRepository.SelectedGame.AddTarget(targetInfo);
            App.HideLoading();
        }
    }

    private async void edit_repository_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        RepositoryInfo editRepository = ViewModel.SelectedRepository;
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = LanguageHelper.GetString("Dlg_Repository_Edit"),
            PrimaryButtonText = LanguageHelper.GetString("Dlg_Confirm"),
            CloseButtonText = LanguageHelper.GetString("Dlg_Cancel"),
            DefaultButton = ContentDialogButton.Primary,
            Content = new RepositoryInfoControl()
            {
                Width = 720,
                DataContext = editRepository,
                FolderPathVisible = false
            },
            RequestedTheme = App.MainWindow.RequestedTheme(),
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

    private async Task<ContentDialogResult> EditSourceInfo(SourceInfo sourceInfo)
    {
        SourceInfoControl sourceInfoControl = new() { Width = 720, DataContext = sourceInfo };
        ContentDialog dialog =
            new()
            {
                XamlRoot = this.XamlRoot,
                Title = LanguageHelper.GetString("Dlg_Source_Edit"),
                PrimaryButtonText = LanguageHelper.GetString("Dlg_Confirm"),
                CloseButtonText = LanguageHelper.GetString("Dlg_Cancel"),
                DefaultButton = ContentDialogButton.Primary,
                Content = sourceInfoControl,
                RequestedTheme = App.MainWindow.RequestedTheme(),
            };
        // https://github.com/microsoft/microsoft-ui-xaml/issues/424
        dialog.Resources["ContentDialogMaxWidth"] = 1080;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !sourceInfoControl.IsValidSource();
        };

        return await dialog.ShowAsync();
    }

    private async void edit_source_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        SourceInfo sourceInfo = (sender as MenuFlyoutItem).DataContext as SourceInfo;
        var editSourceInfo = SourceInfo.Open(Path.GetDirectoryName(sourceInfo.JsonPath));
        var result = await EditSourceInfo(editSourceInfo);
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.UpdateSource(editSourceInfo);
        }
    }

    private async Task<ContentDialogResult> EditLocalizationInfo(LocalizationInfo localizationInfo)
    {
        LocalizationInfoControl localizationInfoControl = new() { Width = 720, DataContext = localizationInfo };
        ContentDialog dialog =
            new()
            {
                XamlRoot = this.XamlRoot,
                Title = LanguageHelper.GetString("Dlg_Localization_Edit"),
                PrimaryButtonText = LanguageHelper.GetString("Dlg_Confirm"),
                CloseButtonText = LanguageHelper.GetString("Dlg_Cancel"),
                DefaultButton = ContentDialogButton.Primary,
                Content = localizationInfoControl,
                RequestedTheme = App.MainWindow.RequestedTheme(),
            };
        dialog.Resources["ContentDialogMaxWidth"] = 1080;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !localizationInfoControl.IsValidLocalization();
        };

        return await dialog.ShowAsync();
    }

    private async void edit_localization_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        LocalizationInfo localizationInfo = (sender as MenuFlyoutItem).DataContext as LocalizationInfo;
        var editLocalizationInfo = LocalizationInfo.Open(Path.GetDirectoryName(localizationInfo.JsonPath));
        var result = await EditLocalizationInfo(editLocalizationInfo);
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.UpdateLocalization(editLocalizationInfo);
        }
    }

    private async Task<ContentDialogResult> EditTargetInfo(TargetInfo targetInfo)
    {
        TargetInfoControl targetInfoControl = new() { Width = 720, DataContext = targetInfo, };
        ContentDialog dialog =
            new()
            {
                XamlRoot = this.XamlRoot,
                Title = LanguageHelper.GetString("Dlg_Target_Edit"),
                PrimaryButtonText = LanguageHelper.GetString("Dlg_Confirm"),
                CloseButtonText = LanguageHelper.GetString("Dlg_Cancel"),
                DefaultButton = ContentDialogButton.Primary,
                Content = targetInfoControl,
                RequestedTheme = App.MainWindow.RequestedTheme(),
            };
        dialog.Resources["ContentDialogMaxWidth"] = 1080;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !targetInfoControl.IsValidTarget();
        };

        return await dialog.ShowAsync();
    }

    private async void edit_target_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = (sender as MenuFlyoutItem).DataContext as TargetInfo;
        var editTargetInfo = TargetInfo.Open(Path.GetDirectoryName(targetInfo.JsonPath));
        editTargetInfo.Game = ViewModel.SelectedRepository.SelectedGame;
        editTargetInfo.Localization =
            ViewModel.SelectedRepository.SelectedGame.FindLocalization(targetInfo.Localization)
            ?? targetInfo.Localization;
        editTargetInfo.Source =
            ViewModel.SelectedRepository.SelectedGame.FindSource(targetInfo.Source) ?? targetInfo.Source;

        var result = await EditTargetInfo(editTargetInfo);
        if (result == ContentDialogResult.Primary)
        {
            // replace new source
            if (editTargetInfo.Source != null && editTargetInfo.Source.CreateDate != targetInfo.Source.CreateDate)
            {
                App.ShowLoading();
                await editTargetInfo.DecompressSource();
                App.HideLoading();
            }
            else
            {
                editTargetInfo.Source = targetInfo.Source;
            }

            // replace new localization
            if (
                editTargetInfo.Localization != null
                && editTargetInfo.Localization.CreateDate != targetInfo.Localization?.CreateDate
            )
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

    private async void add_source_folder_button_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFolder folder = await FileSystemMisc.PickFolder(new() { "*" });
        if (folder != null)
        {
            App.ShowLoading();
            var existSource = await ViewModel.CopySource(folder.Path);

            if (!existSource)
            {
                SourceInfo sourceInfo = ViewModel.SelectedRepository.SelectedGame.NewSource();
                var result = await EditSourceInfo(sourceInfo);
                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.SelectedRepository.SelectedGame.AddSource(folder.Path, sourceInfo);
                }
            }

            App.HideLoading();
        }
    }

    private async void add_localization_folder_button_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFolder folder = await FileSystemMisc.PickFolder(new() { "*" });
        if (folder != null)
        {
            App.ShowLoading();
            var existLocalization = await ViewModel.CopyLocalization(folder.Path);
            if (!existLocalization)
            {
                LocalizationInfo localizationInfo = ViewModel.SelectedRepository.SelectedGame.NewLocalization();
                var result = await EditLocalizationInfo(localizationInfo);
                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.SelectedRepository.SelectedGame.AddLocalization(folder.Path, localizationInfo);
                }
            }
            App.HideLoading();
        }
    }

    private async void add_target_folder_button_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFolder folder = await FileSystemMisc.PickFolder(new() { "*" });
        if (folder != null)
        {
            App.ShowLoading();
            // check whether exist valid target that archive or not
            var targetInfo = TargetInfo.Open(folder.Path);
            if (targetInfo != null)
            {
                // copy to local
                targetInfo.CreateDate = DateTime.Now;
                targetInfo.SetGamePath(ViewModel.SelectedRepository.SelectedGame.FolderPath);
                await ViewModel.SelectedRepository.SelectedGame.CopyTarget(folder.Path, targetInfo);
                App.HideLoading();
                return;
            }

            // check exist source or localization
            targetInfo = ViewModel.SelectedRepository.SelectedGame.NewTarget();
            targetInfo.Source = SourceInfo.Open(folder.Path);
            targetInfo.Localization = LocalizationInfo.Open(folder.Path);

            if (targetInfo.Localization == null && targetInfo.Source == null)
            {
                // don't both exist, create as new source to add target
                targetInfo.Source = ViewModel.SelectedRepository.SelectedGame.NewSource();
                var result = await EditSourceInfo(targetInfo.Source);
                if (result == ContentDialogResult.Primary)
                {
                    targetInfo.SeletedSource();
                    await ViewModel.SelectedRepository.SelectedGame.CopyTarget(folder.Path, targetInfo);
                }
            }
            else
            {
                // dezip to add target
                if (targetInfo.Localization != null)
                    targetInfo.SeletedLocalization();
                else
                    targetInfo.SeletedSource();

                if (!targetInfo.IsValid())
                    App.ShowDialogError(LanguageHelper.GetString("Msg_Target_Add_Invalid"));
                else
                    await ViewModel.SelectedRepository.SelectedGame.AddTarget(targetInfo);
            }

            App.HideLoading();
        }
    }

    private async void export_target_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = (sender as MenuFlyoutItem).DataContext as TargetInfo;

        if (targetInfo.IsArchive && !await App.ShowDialogConfirm(LanguageHelper.GetString("Msg_Target_Archive_Copy")))
            return;

        Windows.Storage.StorageFolder folder = await FileSystemMisc.PickFolder(new() { "*" });
        if (folder != null)
        {
            App.ShowLoading();
            await targetInfo.CopyTargetAsSourceToFolder(folder.Path);
            App.HideLoading();
        }
    }

    private async void delete_game_button_Click(object sender, RoutedEventArgs e)
    {
        if (await App.ShowDialogConfirm(LanguageHelper.GetString("Msg_Delete_Confirm")))
        {
            App.ShowLoading();
            await ViewModel.DeleteGame(ViewModel.SelectedRepository.SelectedGame);
            App.HideLoading();
        }
    }

    private async void delete_source_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        SourceInfo sourceInfo = (sender as MenuFlyoutItem).DataContext as SourceInfo;
        if (await App.ShowDialogConfirm(LanguageHelper.GetString("Msg_Delete_Confirm")))
        {
            App.ShowLoading();
            await ViewModel.DeleteSource(sourceInfo);
            App.HideLoading();
        }
    }

    private async void delete_localization_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        LocalizationInfo localizationInfo = (sender as MenuFlyoutItem).DataContext as LocalizationInfo;
        if (await App.ShowDialogConfirm(LanguageHelper.GetString("Msg_Delete_Confirm")))
        {
            App.ShowLoading();
            await ViewModel.DeleteLocalization(localizationInfo);
            App.HideLoading();
        }
    }

    private async void delete_target_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = (sender as MenuFlyoutItem).DataContext as TargetInfo;
        if (await App.ShowDialogConfirm(LanguageHelper.GetString("Msg_Delete_Confirm")))
        {
            App.ShowLoading();
            await ViewModel.DeleteTarget(targetInfo);
            App.HideLoading();
        }
    }

    private async void archive_target_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = (sender as MenuFlyoutItem).DataContext as TargetInfo;
        if (await App.ShowDialogConfirm(LanguageHelper.GetString("Msg_Target_Archive").Format(App.ZipLevel())))
        {
            App.ShowLoading();
            await targetInfo.Archive();
            targetInfo.CheckArchiveStatus();
            App.HideLoading();
        }
    }

    private async void dearchive_target_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = (sender as MenuFlyoutItem).DataContext as TargetInfo;
        if (await App.ShowDialogConfirm(LanguageHelper.GetString("Msg_Target_DeArchive")))
        {
            App.ShowLoading();
            await targetInfo.DeArchive();
            targetInfo.CheckArchiveStatus();
            App.HideLoading();
        }
    }

    private async void delete_target_folder_only_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = (sender as MenuFlyoutItem).DataContext as TargetInfo;
        if (await App.ShowDialogConfirm(LanguageHelper.GetString("Msg_Target_Delete_Folder_Only")))
        {
            App.ShowLoading();
            targetInfo.CheckArchiveStatus();
            await targetInfo.DeleteFolderOnly();
            App.HideLoading();
        }
    }
}
