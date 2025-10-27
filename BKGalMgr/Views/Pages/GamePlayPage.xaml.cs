using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using BKGalMgr.Extensions;
using BKGalMgr.Helpers;
using BKGalMgr.Services;
using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
using CommunityToolkit.WinUI.Controls;
using H.NotifyIcon;
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
public sealed partial class GamePlayPage : Page
{
    public GamePlayPageViewModel ViewModel { get; }

    public GamePlayPage()
    {
        ViewModel = App.GetRequiredService<GamePlayPageViewModel>();
        DataContext = this;
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.Game = e.Parameter as GameInfo;
        _ = LoadTheme();
    }

    private async Task LoadTheme()
    {
        var theme = ViewModel.Game.CustomTheme;
        if (theme.ThemeType == CustomThemeType.Default)
        {
            theme.RequestedTheme = ElementTheme.Dark;
            // 每次都重新计算主色调
            if (ViewModel.Game.Cover.IsNullOrEmpty() || ViewModel.Game.Cover.EndsWith(".webp"))
            {
                theme.LinearGradientStartColor = "#FF000000"; // System.Drawing.Color.Black;
                theme.LinearGradientEndColor = "#FF808080"; // System.Drawing.Color.Gray;
            }
            else
            {
                var colors = await Task.Run(() =>
                {
                    var primaryColor = ColorHelper.GetImagePrimaryColor(ViewModel.Game.Cover);
                    // 调暗
                    while (!ColorHelper.IsDarkColor(primaryColor))
                        primaryColor = ColorHelper.GenerateLighterOrDarkerColor(primaryColor, false);

                    var secondColor = primaryColor;
                    if (ColorHelper.IsHarshColor(primaryColor))
                    {
                        secondColor = ColorHelper.GenerateLessHarshColor(primaryColor);
                        primaryColor = ColorHelper.GenerateLighterOrDarkerColor(secondColor, false, 0.2);
                    }
                    else
                    {
                        secondColor = ColorHelper.GenerateLighterOrDarkerColor(primaryColor);
                    }

                    return (ColorHelper.ToWindowsUIColor(primaryColor), ColorHelper.ToWindowsUIColor(secondColor));
                });
                theme.LinearGradientStartColor = colors.Item2.ToString();
                theme.LinearGradientEndColor = colors.Item1.ToString();
            }
        }
    }

    private async Task StartGame(GameInfo gameInfo, string leGuid)
    {
        TargetInfo targetInfo = gameInfo.SelectedTarget;
        if (targetInfo == null)
        {
            App.MainWindow.SelecteTarget(gameInfo);
            return;
        }

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
            if (targetInfo.IsArchive && await DialogHelper.ShowConfirm(LanguageHelper.GetString("Msg_Target_Deachive")))
            {
                App.ShowLoading();
                await targetInfo.DeArchive();
                App.HideLoading();
            }
            if (!File.Exists(targetInfo.TargetExePath))
            {
                App.ShowErrorMessage(
                    LanguageHelper.GetString("Msg_TargetExe_Invalid").Format(targetInfo.TargetExePath)
                );
                return;
            }
        }

        Process gameProcess;
        bool startSuccess = false;

        // 以默认配置运行
        if (targetInfo.EnableLocalEmulator && leGuid.IsNullOrEmpty())
        {
            leGuid = LEProfileInfo.RunGuid;
        }

        if (leGuid.IsNullOrEmpty())
        {
            // normal start
            gameProcess = new();
            gameProcess.StartInfo.FileName = targetInfo.TargetExePath;
            gameProcess.StartInfo.UseShellExecute = true;
            gameProcess.StartInfo.WorkingDirectory = targetInfo.TargetPath;
            try
            {
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
                    // retry
                    gameProcess.StartInfo.UseShellExecute = false;
                    startSuccess = gameProcess.Start();
                }
            }
            catch
            {
                startSuccess = false;
            }
        }
        else if (
            ViewModel.Settings.LocalEmulator.LEProcPath.IsNullOrEmpty()
            || !File.Exists(ViewModel.Settings.LocalEmulator.LEProcPath)
        )
        {
            App.ShowErrorMessage(
                LanguageHelper
                    .GetString("Msg_LocalEmulator_Invalid")
                    .Format(ViewModel.Settings.LocalEmulator.LEProcPath)
            );
            return;
        }
        else if (leGuid == LEProfileInfo.RunGuid)
        {
            gameProcess = LocaleEmulatorHelper.RunDefault(
                ViewModel.Settings.LocalEmulator.LEProcPath,
                targetInfo.TargetExePath
            );
            startSuccess = gameProcess != null;
        }
        else if (leGuid == LEProfileInfo.ManageGuid)
        {
            gameProcess = LocaleEmulatorHelper.ManageApp(
                ViewModel.Settings.LocalEmulator.LEProcPath,
                targetInfo.TargetExePath
            );
            startSuccess = gameProcess != null;
        }
        else
        {
            gameProcess = LocaleEmulatorHelper.RunAs(
                ViewModel.Settings.LocalEmulator.LEProcPath,
                targetInfo.TargetExePath,
                leGuid
            );
            startSuccess = gameProcess != null;
        }

        if (!startSuccess)
        {
            App.ShowErrorMessage(LanguageHelper.GetString("Msg_TargetExe_Start_Fail").Format(targetInfo.TargetExePath));
            return;
        }
        _ = StartTiming(gameInfo, gameProcess);
    }

    record struct MainWindowProcess(Process value);

    private async Task StartTiming(GameInfo gameInfo, Process gameProcess)
    {
        // 尽快获取子进程列表
        var childProcesses = gameProcess.GetChildProcesses();
        var mainWindowProcess = new MainWindowProcess();

        TargetInfo targetInfo = gameInfo.SelectedTarget;
        targetInfo.LastPlayDate = DateTime.Now;
        targetInfo.SaveJsonFile();

        gameInfo.LastPlayDate = targetInfo.LastPlayDate;
        gameInfo.AddPlayedPeriodToFirst(new(gameInfo.LastPlayDate, DateTime.Now, TimeSpan.Zero));
        gameInfo.SaveJsonFile();

        var playedPeriod = gameInfo.PlayedPeriods.First();
        var savePlayedTime = () =>
        {
            if (gameInfo.StopTimingWhenNotActive)
            {
                if (mainWindowProcess.value?.Id == ProcessExtensions.GetForegroundWindowProcess()?.Id)
                {
                    gameInfo.PlayStatus = PlayStatus.Playing;
                    targetInfo.PlayStatus = PlayStatus.Playing;
                }
                else
                {
                    gameInfo.PlayStatus = PlayStatus.Pause;
                    targetInfo.PlayStatus = PlayStatus.Pause;
                }
            }

            if (gameInfo.PlayStatus == PlayStatus.Stop)
                return;

            var timeCost = TimeSpan.FromSeconds(1);

            playedPeriod.EndTime = DateTime.Now;

            if (gameInfo.PlayStatus == PlayStatus.Pause)
            {
                playedPeriod.PauseTime += timeCost;
                gameInfo.SaveJsonFile();
                return;
            }

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

        // 有可能用到LE或者启动器之类的，有可能是链式启动的，需要等待所有有窗口的进程结束
        gameInfo.PlayCancelTokenSource = new CancellationTokenSource();
        try
        {
            // 稍微等下，避免窗口未创建
            // 测试如果使用LE启动，有些游戏是容易断链的，找不到最后的游戏窗口进程
            await Task.Delay(1000);
            // 有可能中间还有子进程中转，但如果连续中转两次，那么延迟一秒可能就导致找不到
            if (gameProcess.IsMainWindowProcess())
            {
                mainWindowProcess.value = gameProcess;
            }
            else
            {
                foreach (var c in childProcesses)
                {
                    if (c.FindMainWindowProcess() is Process p)
                    {
                        mainWindowProcess.value = p;
                        break;
                    }
                }
            }
            // 如果没找到，就直接等待原始进程结束
            if (mainWindowProcess.value == null)
            {
                mainWindowProcess.value = gameProcess;
            }
            await mainWindowProcess.value.WaitForExitAsync(gameInfo.PlayCancelTokenSource.Token);
            do
            {
                // 像是LE会拉起游戏后关闭自身，需要先获取子进程，否则如果子进程自身拉起游戏自身退出，迟了就导致找不到拉起的游戏窗口进程
                var children = mainWindowProcess.value.GetChildProcesses();
                if (!children.Any())
                    break;

                await Task.Delay(1000);
                foreach (var c in children)
                {
                    if (c.FindMainWindowProcess() is Process p)
                    {
                        mainWindowProcess.value = p;
                        break;
                    }
                }
                await mainWindowProcess.value.WaitForExitAsync(gameInfo.PlayCancelTokenSource.Token);
            } while (true);
            // 再找下是否有其他的
            while (gameProcess.FindMainWindowProcess() is Process process)
            {
                mainWindowProcess.value = process;
                await mainWindowProcess.value.WaitForExitAsync(gameInfo.PlayCancelTokenSource.Token);
            }
            //await mainWindowProcess.process.WaitForAllExitAsync();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            App.ShowErrorMessage(LanguageHelper.GetString("Msg_WaitForAllExit_Exception").Format(ex.Message));
            try
            {
                await Task.Delay(TimeSpan.FromDays(1), gameInfo.PlayCancelTokenSource.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex2)
            {
                App.ShowErrorMessage(ex2.Message);
            }
        }

        gameInfo.PlayCancelTokenSource.Dispose();
        gameInfo.PlayCancelTokenSource = null;
        timer.Dispose();

        gameInfo.PlayStatus = PlayStatus.Stop;
        targetInfo.PlayStatus = PlayStatus.Stop;

        playedPeriod.EndTime = DateTime.Now;
        gameInfo.SaveJsonFile();

        // Auto backup
        if (gameInfo.SaveDataSettings.AutoBackup)
        {
            var savedata = gameInfo.NewSaveData();
            savedata.Name = LanguageHelper.GetString("SaveDataInfo_Auto_Backup");
            if (await gameInfo.AddSaveData(savedata) == false)
            {
                App.ShowErrorMessage(LanguageHelper.GetString("Msg_SaveData_Auto_Backup_Fail"));
            }
        }
    }

    private void root_Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (e.GetCurrentPoint(root_Grid).Properties.IsXButton1Pressed)
        {
            App.MainWindow.NavigateToMainPage();
            e.Handled = true;
        }
    }

    private void SegmentedItem_Loaded(object sender, RoutedEventArgs e)
    {
        var segmentedItem = sender as SegmentedItem;
        if (segmentedItem.FindDescendant("PART_Hover") is Border ele)
        {
            ele.CornerRadius = new(12);
        }
    }

    private void back_Button_Click(object sender, RoutedEventArgs e)
    {
        App.MainWindow.NavigateToMainPage();
    }

    private void blog_TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        ViewModel.Game.SaveJsonFile();
    }

    private async void play_Button_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Game.PlayStatus != PlayStatus.Stop && ViewModel.Game.StopTimingWhenNotActive)
        {
            App.ShowWarningMessage(LanguageHelper.GetString("Msg_Toggle_Off_Auto_Stop_Timing"));
            ViewModel.Game.StopTimingWhenNotActive = false;
        }
        await StartGame(ViewModel.Game, "");
    }

    private void stop_Button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Game.PlayCancelTokenSource?.Cancel();
    }

    private void local_emulator_MenuFlyout_Opening(object sender, object e)
    {
        var menu = sender as MenuFlyout;
        menu.Items.Clear();
        foreach (var profile in ViewModel.Settings.LocalEmulator.Profiles)
        {
            if (!profile.IsSeparator)
            {
                var item = new MenuFlyoutItem() { Text = profile.Name, Tag = profile.Guid };

                if (profile.Guid == LEProfileInfo.RunGuid)
                    item.Icon = new ImageIcon() { Source = (ImageSource)App.Current.Resources["LocalEmulatorYellow"] };
                else if (profile.Guid == LEProfileInfo.ManageGuid)
                    item.Icon = new ImageIcon() { Source = (ImageSource)App.Current.Resources["LocalEmulatorGray"] };
                else
                    item.Icon = new ImageIcon() { Source = (ImageSource)App.Current.Resources["LocalEmulatorPurple"] };

                item.Click += local_emulator_MenuFlyoutItem_Click;
                menu.Items.Add(item);
            }
            else
            {
                menu.Items.Add(new MenuFlyoutSeparator());
            }
        }
    }

    private async void local_emulator_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuFlyoutItem;
        await StartGame(ViewModel.Game, menuItem.Tag as string);
    }

    private void cover_Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        ViewModel.Game.LoadCover();
        if (ViewModel.Game.Covers.Count > 0)
            App.ShowImages(ViewModel.Game.Covers, 0);
    }

    private void gallery_Button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Game.LoadGallery();
        if (ViewModel.Game.Gallery.Count > 0)
            App.ShowImages(ViewModel.Game.Gallery, 0);
    }

    private void special_Button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Game.LoadSpecial();
        if (ViewModel.Game.Special.Count > 0)
            App.ShowImages(ViewModel.Game.Special, 0);
    }

    private void screenshot_Button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Game.LoadScreenshot();
        if (ViewModel.Game.Screenshot.Count > 0)
            App.ShowImages(ViewModel.Game.Screenshot, 0);
    }

    private void screen_capture_Button_Click(object sender, RoutedEventArgs e)
    {
        var targetInfo = ViewModel.Game.SelectedTarget;
        App.MainWindow.Hide();
        Task.Delay(225)
            .ContinueWith(
                t =>
                {
                    targetInfo.DoScreenCapture();
                    App.MainWindow.ShowWindow();
                },
                TaskScheduler.FromCurrentSynchronizationContext()
            );
    }

    private void library_Button_Click(object sender, RoutedEventArgs e)
    {
        MainPage.NavigateTo(typeof(LibraryAndManagePage), ViewModel.Game);
        App.MainWindow.NavigateToMainPage();
    }

    private void character_FlipView_Loaded(object sender, RoutedEventArgs e)
    {
        var preButton = character_FlipView.FindDescendant("PreviousButtonHorizontal");
        var nextButton = character_FlipView.FindDescendant("NextButtonHorizontal");
        if (preButton is Button pre)
            pre.Opacity = 0;
        if (nextButton is Button next)
            next.Opacity = 0;
    }

    private void character_illustration_Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        var images = ViewModel.Game.Characters.Select(c => c.Illustration).ToList();
        App.ShowImages(images, character_FlipView.SelectedIndex);
    }

    private void targets_Button_Click(object sender, RoutedEventArgs e)
    {
        App.MainWindow.SelecteTarget(ViewModel.Game);
    }

    private void bangumi_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        BangumiService.OpenSubjectPage(ViewModel.Game.BangumiSubjectId);
    }

    private void t2dfan_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        T2DFanService.OpenSubjectPage(ViewModel.Game.T2DFanSubjectId);
    }

    private ThemeInfo _themeOldValue;

    private void custom_theme_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        _themeOldValue = new();
        ViewModel.Game.CustomTheme.Adapt(_themeOldValue);
        custom_theme_popup_Grid.Visibility = Visibility.Visible;
    }

    private void custom_theme_confirm_Button_Click(object sender, RoutedEventArgs e)
    {
        custom_theme_popup_Grid.Visibility = Visibility.Collapsed;
        // 选择Default后，原本的色调不会加载，点击确定后才会重新计算，
        // 实现上是可以事件通知类型变更的，但是切换上要处理的太多，先这样吧
        _ = LoadTheme();
        ViewModel.Game.SaveJsonFile();
    }

    private void custom_theme_cancel_Button_Click(object sender, RoutedEventArgs e)
    {
        _themeOldValue.Adapt(ViewModel.Game.CustomTheme);
        custom_theme_popup_Grid.Visibility = Visibility.Collapsed;
        _ = LoadTheme();
    }

    private void select_game_process_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Game.SelectedTarget == null)
        {
            App.MainWindow.SelecteTarget(ViewModel.Game);
            return;
        }
        select_game_process_ListView.ItemsSource = ProcessExtensions.GetAllWindowProcess();
        select_game_process_Grid.Visibility = Visibility.Visible;
    }

    private void select_game_process_confirm_Button_Click(object sender, RoutedEventArgs e)
    {
        if (select_game_process_ListView.SelectedItem is Process gameProcess)
            _ = StartTiming(ViewModel.Game, gameProcess);
        select_game_process_Grid.Visibility = Visibility.Collapsed;
    }

    private void select_game_process_cancel_Button_Click(object sender, RoutedEventArgs e)
    {
        select_game_process_Grid.Visibility = Visibility.Collapsed;
    }

    private void toggle_stop_timing_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Game.StopTimingWhenNotActive = !ViewModel.Game.StopTimingWhenNotActive;
        if (!ViewModel.Game.StopTimingWhenNotActive && ViewModel.Game.PlayStatus != PlayStatus.Stop)
        {
            ViewModel.Game.PlayStatus = PlayStatus.Playing;
            ViewModel.Game.SelectedTarget.PlayStatus = PlayStatus.Playing;
        }
    }
}
