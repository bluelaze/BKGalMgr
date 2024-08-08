using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using BKGalMgr.Helpers;
using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
using BKGalMgr.Views.Controls;
using H.NotifyIcon;
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
public sealed partial class GamesPage : Page
{
    public GamesManagePageViewModel ViewModel { get; }

    public GamesPage()
    {
        ViewModel = App.GetRequiredService<GamesManagePageViewModel>();
        DataContext = this;
        this.InitializeComponent();
    }

    private void gamename_linkbutton_Click(object sender, RoutedEventArgs e)
    {
        var gameInfo = (sender as FrameworkElement).DataContext as GameInfo;
        if (gameInfo != null)
        {
            ViewModel.SelectedRepository.SelectedGame = gameInfo;
            MainPage.NavigateTo(typeof(ManagePage));
        }
    }

    private async void play_splitbutton_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        var playBtn = sender;
        var gameInfo = playBtn.DataContext as GameInfo;
        var targetInfo = gameInfo.SelectedTarget;
        if (targetInfo != null)
        {
            if (gameInfo.PlayStatus == PlayStatus.Playing)
            {
                gameInfo.PlayStatus = PlayStatus.Pause;
                targetInfo.PlayStatus = PlayStatus.Pause;
                return;
            }
            else if (gameInfo.PlayStatus == PlayStatus.Pause)
            {
                gameInfo.PlayStatus = PlayStatus.Playing;
                targetInfo.PlayStatus = PlayStatus.Playing;
                return;
            }

            if (!File.Exists(targetInfo.TargetExePath))
            {
                if (
                    targetInfo.IsArchive && await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Target_Deachive"))
                )
                {
                    App.ShowLoading();
                    await targetInfo.DeArchive();
                    App.HideLoading();
                }
                if (!File.Exists(targetInfo.TargetExePath))
                {
                    DialogHelper.ShowError(
                        LanguageHelper.GetString("Msg_TargetExe_Invalid").Format(targetInfo.TargetExePath)
                    );
                    return;
                }
            }
            Process gameProcess = new();
            gameProcess.StartInfo.FileName = targetInfo.TargetExePath;
            gameProcess.StartInfo.WorkingDirectory = targetInfo.TargetPath;
            gameProcess.StartInfo.UseShellExecute = true;

            bool startSuccess = false;
            try
            {
                startSuccess = gameProcess.Start();
            }
            catch
            {
                startSuccess = false;
            }

            if (!startSuccess)
            {
                DialogHelper.ShowError(
                    LanguageHelper.GetString("Msg_TargetExe_Start_Fail").Format(targetInfo.TargetExePath)
                );
            }
            else
            {
                targetInfo.LastPlayDate = DateTime.Now;
                targetInfo.SaveJsonFile();

                gameInfo.LastPlayDate = targetInfo.LastPlayDate;
                gameInfo.SaveJsonFile();

                var savePlayedTime = () =>
                {
                    if (gameInfo.PlayStatus == PlayStatus.Stop || gameInfo.PlayStatus == PlayStatus.Pause)
                        return;

                    var timeCost = TimeSpan.FromSeconds(1);

                    targetInfo.PlayedTime += timeCost;
                    targetInfo.SaveJsonFile();

                    gameInfo.PlayedTime += timeCost;
                    gameInfo.SaveJsonFile();
                };

                // update game here, but need in open target?
                targetInfo.Game = gameInfo;

                gameInfo.PlayStatus = PlayStatus.Playing;
                targetInfo.PlayStatus = PlayStatus.Playing;

                var timer = Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(_ =>
                    {
                        savePlayedTime();
                    });

                // check child process, maybe startup exe is a launcher
                await gameProcess.WaitForExitAsync();
                foreach (var childProcess in gameProcess.GetChildProcesses())
                    await childProcess.WaitForExitAsync();

                timer.Dispose();

                gameInfo.PlayStatus = PlayStatus.Stop;
                targetInfo.PlayStatus = PlayStatus.Stop;

                gameInfo.PlayedPeriods.Insert(0, new(gameInfo.LastPlayDate, DateTime.Now));
                gameInfo.SaveJsonFile();
            }
        }
    }

    private void capture_hotkey_button_Click(object sender, RoutedEventArgs e)
    {
        var targetInfo = (sender as FrameworkElement).DataContext as TargetInfo;
        App.MainWindow.Hide();
        Task.Delay(225)
            .ContinueWith(
                t =>
                {
                    targetInfo.DoScreenCapture();
                    App.MainWindow.Show();
                },
                TaskScheduler.FromCurrentSynchronizationContext()
            );
    }

    private void group_togglebutton_IsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedRepository.IsEnableGroup)
        {
            // delay to IsChecked TwoWay binding valid
            Task.Delay(33)
                .ContinueWith(
                    _ => ViewModel.SelectedRepository.GamesViewRefreshFilter(),
                    TaskScheduler.FromCurrentSynchronizationContext()
                );
        }
    }

    private async Task<ContentDialogResult> EditGroupInfo(GroupInfo groupInfo)
    {
        GroupInfoControl groupInfoControl = new() { Width = 720, DataContext = groupInfo };
        ContentDialog dialog =
            new()
            {
                XamlRoot = this.XamlRoot,
                Title = LanguageHelper.GetString("Dlg_Group_Edit"),
                PrimaryButtonText = LanguageHelper.GetString("Dlg_Confirm"),
                CloseButtonText = LanguageHelper.GetString("Dlg_Cancel"),
                DefaultButton = ContentDialogButton.Primary,
                Content = groupInfoControl,
                RequestedTheme = App.MainWindow.RequestedTheme(),
            };
        dialog.Resources["ContentDialogMaxWidth"] = 1080;
        dialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
        {
            args.Cancel = groupInfo.Name.IsNullOrEmpty();
        };

        return await dialog.ShowAsync();
    }

    private async void add_group_button_Click(object sender, RoutedEventArgs e)
    {
        App.ShowLoading();
        GroupInfo groupInfo = new();
        var result = await EditGroupInfo(groupInfo);
        if (result == ContentDialogResult.Primary && !groupInfo.Name.IsNullOrEmpty())
        {
            ViewModel.SelectedRepository.GroupChanged(null, groupInfo, GroupChangedType.Add);
        }
        App.HideLoading();
    }

    private async void edit_group_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        GroupInfo groupInfo = (sender as MenuFlyoutItem).DataContext as GroupInfo;
        GroupInfo newGroupInfo = JsonMisc.Deserialize<GroupInfo>(JsonMisc.Serialize(groupInfo));
        var result = await EditGroupInfo(newGroupInfo);
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.SelectedRepository.GroupChanged(groupInfo, newGroupInfo, GroupChangedType.Edit);
        }
    }

    private async void delete_group_menuflyoutitem_Click(object sender, RoutedEventArgs e)
    {
        GroupInfo groupInfo = (sender as MenuFlyoutItem).DataContext as GroupInfo;
        if (await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Delete_Confirm")))
        {
            App.ShowLoading();
            ViewModel.SelectedRepository.GroupChanged(groupInfo, null, GroupChangedType.Remove);
            App.HideLoading();
        }
    }
}
