using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using BKGalMgr.Extensions;
using BKGalMgr.Helpers;
using BKGalMgr.ThirdParty;
using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
using BKGalMgr.Views.Controls;
using Mapster;
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
    public LibraryAndManagePageViewModel ViewModel { get; }

    public ManagePage()
    {
        ViewModel = App.GetRequiredService<LibraryAndManagePageViewModel>();
        DataContext = this;
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (!ViewModel.IsLoadedRepository)
        {
            App.ShowLoading();
            await ViewModel.LoadRepository();
            App.HideLoading();
        }
        if (e.Parameter is GameInfo game)
        {
            game.Repository.SelectedGame = game;
            ViewModel.SelectedRepository = game.Repository;
        }
    }

    private void add_game_button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddNewGame();
    }

    private async Task<(ContentDialogResult result, string subjectUrl)> EditBangumiGameInfo(string bangumiSubjectId)
    {
        BangumiSubjectControl bangumiSubjectControl = new BangumiSubjectControl()
        {
            Width = 720,
            AccessToken = ViewModel.BangumiAccessToken,
            SubjectUrl = bangumiSubjectId,
        };

        ContentDialog dialog = DialogHelper.GetConfirmDialog();
        dialog.Title = LanguageHelper.GetString("Bangumi_Edit_Game_Info");
        dialog.Content = bangumiSubjectControl;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel =
                bangumiSubjectControl.AccessToken.IsNullOrEmpty() || bangumiSubjectControl.SubjectUrl.IsNullOrEmpty();
        };

        var result = await dialog.ShowAsync();

        return (result, bangumiSubjectControl.SubjectUrl);
    }

    private async Task<GameInfo> PullBangumiGameInfo(string bangumiSubjectId)
    {
        (ContentDialogResult result, string subjectUrl) = await EditBangumiGameInfo(bangumiSubjectId);

        if (result == ContentDialogResult.Primary)
        {
            App.ShowLoading();

            var response = await ViewModel.PullGameFromBangumi(ViewModel.BangumiAccessToken, subjectUrl);

            if (!response.ErrorMessage.IsNullOrEmpty())
                App.ShowErrorMessage(LanguageHelper.GetString("Bangumi_Pull_Game_Eorr").Format(response.ErrorMessage));

            App.HideLoading();
            return response.Game;
        }
        return null;
    }

    private async void add_bangumi_game_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        var gameInfo = await PullBangumiGameInfo("");
        if (gameInfo != null)
        {
            ViewModel.AddNewGame(gameInfo);
            var errMsg = await ViewModel.PullGameCharacterFromBangumi(gameInfo);
            if (!errMsg.IsNullOrEmpty())
                App.ShowErrorMessage(errMsg);
        }
    }

    private async void update_bangumi_game_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        var gameInfo = await PullBangumiGameInfo(ViewModel.SelectedRepository.SelectedGame.BangumiSubjectId);
        if (gameInfo != null)
        {
            ViewModel.UpdateGame(gameInfo);
            var errMsg = await ViewModel.PullGameCharacterFromBangumi(ViewModel.SelectedRepository.SelectedGame);
            if (!errMsg.IsNullOrEmpty())
                App.ShowErrorMessage(errMsg);
        }
    }

    private async void edit_bangumi_game_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        (ContentDialogResult result, string subjectUrl) = await EditBangumiGameInfo(
            ViewModel.SelectedRepository.SelectedGame.BangumiSubjectId
        );
        if (result == ContentDialogResult.Primary)
            ViewModel.UpdateBangumiSubjectId(subjectUrl);
    }

    private async void open_bangumi_game_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedRepository.SelectedGame.BangumiSubjectId.IsNullOrEmpty())
        {
            (ContentDialogResult result, string subjectUrl) = await EditBangumiGameInfo("");
            if (result != ContentDialogResult.Primary)
                return;
            ViewModel.UpdateBangumiSubjectId(subjectUrl);
        }
        ViewModel.OpenBangumiGame(ViewModel.SelectedRepository.SelectedGame);
    }

    private async Task<(ContentDialogResult result, string subjectUrl)> Edit2DFanGameInfo(string t2dfanSubjectId)
    {
        var t2dfanSubjectControl = new T2DFanSubjectControl() { Width = 720, SubjectUrl = t2dfanSubjectId };

        var dialog = DialogHelper.GetConfirmDialog();
        dialog.Title = LanguageHelper.GetString("T2DFan_Edit_Game_Info");
        dialog.Content = t2dfanSubjectControl;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = t2dfanSubjectControl.SubjectUrl.IsNullOrEmpty();
        };

        var result = await dialog.ShowAsync();

        return (result, t2dfanSubjectControl.SubjectUrl);
    }

    private async void edit_t2dfan_game_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        (ContentDialogResult result, string subjectUrl) = await Edit2DFanGameInfo(
            ViewModel.SelectedRepository.SelectedGame.T2DFanSubjectId
        );
        if (result == ContentDialogResult.Primary)
            ViewModel.Update2DFanSubjectId(subjectUrl);
    }

    private async void open_t2dfan_game_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedRepository.SelectedGame.T2DFanSubjectId.IsNullOrEmpty())
        {
            (ContentDialogResult result, string subjectUrl) = await Edit2DFanGameInfo("");
            if (result != ContentDialogResult.Primary)
                return;
            ViewModel.Update2DFanSubjectId(subjectUrl);
        }
        ViewModel.Open2dfanGame(ViewModel.SelectedRepository.SelectedGame);
    }

    private void play_game_Button_Click(object sender, RoutedEventArgs e)
    {
        App.MainWindow.NavigateToGamePlayPage(ViewModel.SelectedRepository.SelectedGame);
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

    private async Task<ContentDialogResult> EditSourceInfo(SourceInfo sourceInfo)
    {
        SourceInfoControl sourceInfoControl = new() { Width = 720, DataContext = sourceInfo };

        ContentDialog dialog = DialogHelper.GetConfirmDialog();
        dialog.Title = LanguageHelper.GetString("Dlg_Source_Edit");
        dialog.Content = sourceInfoControl;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !sourceInfoControl.IsValidSource();
        };

        return await dialog.ShowAsync();
    }

    private async void edit_source_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        SourceInfo sourceInfo = (sender as MenuFlyoutItem).DataContext as SourceInfo;
        var editSourceInfo = SourceInfo.Open(sourceInfo.FolderPath);
        var result = await EditSourceInfo(editSourceInfo);
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.UpdateSource(editSourceInfo);
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

    private async Task<ContentDialogResult> EditLocalizationInfo(LocalizationInfo localizationInfo)
    {
        LocalizationInfoControl localizationInfoControl = new() { Width = 720, DataContext = localizationInfo };

        ContentDialog dialog = DialogHelper.GetConfirmDialog();
        dialog.Title = LanguageHelper.GetString("Dlg_Localization_Edit");
        dialog.Content = localizationInfoControl;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !localizationInfoControl.IsValidLocalization();
        };

        return await dialog.ShowAsync();
    }

    private async void edit_localization_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        LocalizationInfo localizationInfo = (sender as MenuFlyoutItem).DataContext as LocalizationInfo;
        var editLocalizationInfo = LocalizationInfo.Open(localizationInfo.FolderPath);
        var result = await EditLocalizationInfo(editLocalizationInfo);
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.UpdateLocalization(editLocalizationInfo);
        }
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
                    App.ShowErrorMessage(LanguageHelper.GetString("Msg_Target_Add_Invalid"));
                else
                    await ViewModel.SelectedRepository.SelectedGame.AddTarget(targetInfo);
            }
            targetInfo.Description = targetInfo.Source?.Description;
            App.HideLoading();
        }
    }

    private async void add_target_shortcut_Button_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFile file = await FileSystemMisc.PickFile(new() { ".exe", ".lnk" });
        if (file == null)
            return;

        // 创建一个临时文件夹存放快捷方式，作为源来创建本
        if (!ViewModel.SelectedRepository.SelectedGame.CreateShortcut(file.Path))
        {
            App.ShowErrorMessage(LanguageHelper.GetString("Msg_Target_Create_Shortcut_Fail"));
            return;
        }

        App.ShowLoading();

        // shortcut folder as source
        var targetInfo = ViewModel.SelectedRepository.SelectedGame.NewTarget();
        targetInfo.Source = ViewModel.SelectedRepository.SelectedGame.NewSource();
        targetInfo.Source.StartupName = Path.GetFileName(ShortcutHelpers.GetShortcutPath(file.Path));
        var result = await EditSourceInfo(targetInfo.Source);
        if (result == ContentDialogResult.Primary)
        {
            targetInfo.SeletedSource();
            targetInfo.Description = targetInfo.Source.Description;
            await ViewModel.SelectedRepository.SelectedGame.CopyTarget(
                ViewModel.SelectedRepository.SelectedGame.ShortcutFolderPath,
                targetInfo
            );
        }

        App.HideLoading();
    }

    private async Task<ContentDialogResult> EditTargetInfo(TargetInfo targetInfo)
    {
        TargetInfoControl targetInfoControl = new() { Width = 720, DataContext = targetInfo };

        ContentDialog dialog = DialogHelper.GetConfirmDialog();
        dialog.Title = LanguageHelper.GetString("Dlg_Target_Edit");
        dialog.Content = targetInfoControl;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !targetInfoControl.IsValidTarget();
        };

        return await dialog.ShowAsync();
    }

    private async void edit_target_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = (sender as MenuFlyoutItem).DataContext as TargetInfo;
        var editTargetInfo = TargetInfo.Open(targetInfo.FolderPath);
        editTargetInfo.Game = ViewModel.SelectedRepository.SelectedGame;
        // 原本的源和本地化可能被删除了
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
                targetInfo.Source = editTargetInfo.Source;
                await editTargetInfo.DecompressSource();
                App.HideLoading();
            }

            // replace new localization
            if (
                editTargetInfo.Localization != null
                && editTargetInfo.Localization.CreateDate != targetInfo.Localization?.CreateDate
            )
            {
                App.ShowLoading();
                targetInfo.Localization = editTargetInfo.Localization;
                await editTargetInfo.DecompressLocalization();
                App.HideLoading();
            }

            // 手动刷新属性，用Adapt会卡死，也不合适
            targetInfo.Name = editTargetInfo.Name;
            targetInfo.StartupName = editTargetInfo.StartupName;
            targetInfo.Description = editTargetInfo.Description;
            targetInfo.EnableLocalEmulator = editTargetInfo.EnableLocalEmulator;
            targetInfo.SaveJsonFile();
        }
    }

    private async void export_target_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = (sender as MenuFlyoutItem).DataContext as TargetInfo;

        if (
            targetInfo.IsArchive && !await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Target_Archive_Copy"))
        )
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
        if (await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Delete_Confirm")))
        {
            App.ShowLoading();
            await ViewModel.DeleteGame(ViewModel.SelectedRepository.SelectedGame);
            App.HideLoading();
        }
    }

    private async void delete_source_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        SourceInfo sourceInfo = (sender as MenuFlyoutItem).DataContext as SourceInfo;
        if (await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Delete_Confirm")))
        {
            App.ShowLoading();
            await ViewModel.DeleteSource(sourceInfo);
            App.HideLoading();
        }
    }

    private async void delete_localization_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        LocalizationInfo localizationInfo = (sender as MenuFlyoutItem).DataContext as LocalizationInfo;
        if (await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Delete_Confirm")))
        {
            App.ShowLoading();
            await ViewModel.DeleteLocalization(localizationInfo);
            App.HideLoading();
        }
    }

    private async void delete_target_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = (sender as MenuFlyoutItem).DataContext as TargetInfo;
        if (await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Delete_Confirm")))
        {
            App.ShowLoading();
            await ViewModel.DeleteTarget(targetInfo);
            App.HideLoading();
        }
    }

    private async void archive_target_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        TargetInfo targetInfo = (sender as MenuFlyoutItem).DataContext as TargetInfo;
        if (await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Target_Archive").Format(App.ZipLevel())))
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
        if (await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Target_DeArchive")))
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
        if (await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Target_Delete_Folder_Only")))
        {
            App.ShowLoading();
            await targetInfo.DeleteFolderOnly();
            App.HideLoading();
        }
    }

    private async Task OpenSaveDataSettingsDialog()
    {
        var savedataSettingsInfo = ViewModel.SelectedRepository.SelectedGame.SaveDataSettings;

        SaveDataSettingsInfoControl savedataSettingsInfoControl = new()
        {
            Width = 720,
            DataContext = savedataSettingsInfo,
        };

        ContentDialog dialog = DialogHelper.GetConfirmDialog();
        dialog.Title = LanguageHelper.GetString("Dlg_SaveData_Settings");
        dialog.Content = savedataSettingsInfoControl;
        //dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        //{
        //    args.Cancel = !savedataSettingsInfoControl.IsValidSaveDataSettings();
        //};

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            savedataSettingsInfo.SaveJsonFile();
        }
    }

    private async void savedata_settings_Button_Click(object sender, RoutedEventArgs e)
    {
        await OpenSaveDataSettingsDialog();
    }

    private async Task<ContentDialogResult> EditSaveDataInfo(SaveDataInfo savedataInfo)
    {
        SaveDataInfoControl savedataInfoControl = new() { Width = 720, DataContext = savedataInfo };

        ContentDialog dialog = DialogHelper.GetConfirmDialog();
        dialog.Title = LanguageHelper.GetString("Dlg_SaveData_Edit");
        dialog.Content = savedataInfoControl;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !savedataInfoControl.IsValidSaveData();
        };

        return await dialog.ShowAsync();
    }

    private async void savedata_Button_Click(object sender, RoutedEventArgs e)
    {
        var savedataSettingsInfo = ViewModel.SelectedRepository.SelectedGame.SaveDataSettings;
        if (!savedataSettingsInfo.IsValid())
            await OpenSaveDataSettingsDialog();

        if (
            savedataSettingsInfo.SaveDataFolderPath.IsNullOrEmpty()
            || !Directory.Exists(savedataSettingsInfo.SaveDataFolderPath)
        )
        {
            App.ShowErrorMessage(LanguageHelper.GetString("Msg_SaveData_Invalid_Path"));
            return;
        }

        App.ShowLoading();

        var saveDataInfo = ViewModel.SelectedRepository.SelectedGame.NewSaveData();
        if (await EditSaveDataInfo(saveDataInfo) == ContentDialogResult.Primary)
        {
            if (!await ViewModel.SelectedRepository.SelectedGame.AddSaveData(saveDataInfo))
                App.ShowErrorMessage(LanguageHelper.GetString("Msg_SaveData_Backup_Fail"));
        }

        App.HideLoading();
    }

    private async void edit_savedata_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var savedataInfo = (sender as MenuFlyoutItem).DataContext as SaveDataInfo;
        var editSaveDataInfo = SaveDataInfo.Open(savedataInfo.FolderPath);
        var result = await EditSaveDataInfo(editSaveDataInfo);
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.UpdateSaveData(editSaveDataInfo);
        }
    }

    private async void restore_savedata_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var savedataInfo = (sender as MenuFlyoutItem).DataContext as SaveDataInfo;
        if (ViewModel.SelectedRepository.SelectedGame.PlayStatus == PlayStatus.Playing)
        {
            App.ShowErrorMessage(LanguageHelper.GetString("Msg_Game_Need_Stop"));
            return;
        }

        if (await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_SaveData_Restore_Comfirm")))
        {
            if (await ViewModel.SelectedRepository.SelectedGame.RestoreSaveData(savedataInfo))
            {
                App.ShowSuccessMessage(LanguageHelper.GetString("Msg_SaveData_Restore_Success"));
            }
            else
            {
                App.ShowErrorMessage(LanguageHelper.GetString("Msg_SaveData_Restore_Fail"));
            }
        }
    }

    private async void delete_savedata_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var savedataInfo = (sender as MenuFlyoutItem).DataContext as SaveDataInfo;
        if (await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Delete_Confirm")))
        {
            App.ShowLoading();
            await ViewModel.DeleteSaveData(savedataInfo);
            App.HideLoading();
        }
    }

    private async void add_illustration_Button_Click(object sender, RoutedEventArgs e)
    {
        var charater = (sender as Button).DataContext as CharacterInfo;
        if (charater.Illustration.IsNullOrEmpty())
        {
            Windows.Storage.StorageFile file = await FileSystemMisc.PickFile(
                GlobalInfo.GameCoverSupportFormats.ToList()
            );
            if (file != null)
            {
                charater.Illustration = file.Path;
                await charater.SaveIllustration();
            }
        }
        else
        {
            var images = ViewModel.SelectedRepository.SelectedGame.Characters.Select(c => c.Illustration).ToList();
            App.ShowImages(images, images.IndexOf(charater.Illustration));
        }
    }

    private async Task<ContentDialogResult> EditCharacterInfo(CharacterInfo characterInfo)
    {
        CharacterInfoControl characterInfoControl = new() { Width = 720, DataContext = characterInfo };

        ContentDialog dialog = DialogHelper.GetConfirmDialog();
        dialog.Title = LanguageHelper.GetString("Dlg_Character_Edit");
        dialog.Content = characterInfoControl;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = !characterInfoControl.IsValidCharacter();
        };

        return await dialog.ShowAsync();
    }

    private async void edit_character_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var characterInfo = (sender as MenuFlyoutItem).DataContext as CharacterInfo;
        var editCharacterInfo = new CharacterInfo();
        characterInfo.Adapt(editCharacterInfo);
        var result = await EditCharacterInfo(editCharacterInfo);
        if (result == ContentDialogResult.Primary)
        {
            editCharacterInfo.Adapt(characterInfo);
            await characterInfo.SaveIllustration();
        }
    }

    private void move_up_character_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var characterInfo = (sender as MenuFlyoutItem).DataContext as CharacterInfo;
        ViewModel.SelectedRepository.SelectedGame.MoveUpCharacter(characterInfo);
    }

    private void move_down_character_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var characterInfo = (sender as MenuFlyoutItem).DataContext as CharacterInfo;
        ViewModel.SelectedRepository.SelectedGame.MoveDwonCharacter(characterInfo);
    }

    private async void delete_character_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var characterInfo = (sender as MenuFlyoutItem).DataContext as CharacterInfo;
        if (await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Delete_Confirm")))
        {
            ViewModel.SelectedRepository.SelectedGame.DeleteCharacter(characterInfo);
        }
    }

    private void refresh_Button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshGame();
    }

    private void goto_top_Button_Click(object sender, RoutedEventArgs e)
    {
        gameinfo_ScrollViewer.ChangeView(0, 0, null);
    }

    private void cover_Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        App.ShowImages(ViewModel.SelectedRepository.SelectedGame.Covers, covers_FlipView.SelectedIndex);
    }

    private void gallery_GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        var images = ViewModel.SelectedRepository.SelectedGame.Gallery;
        App.ShowImages(images, images.IndexOf(e.ClickedItem as string));
    }

    private void special_GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        var images = ViewModel.SelectedRepository.SelectedGame.Special;
        App.ShowImages(images, images.IndexOf(e.ClickedItem as string));
    }

    private void screenshot_GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        var images = ViewModel.SelectedRepository.SelectedGame.ScreenCaptures;
        App.ShowImages(images, images.IndexOf(e.ClickedItem as string));
    }
}
