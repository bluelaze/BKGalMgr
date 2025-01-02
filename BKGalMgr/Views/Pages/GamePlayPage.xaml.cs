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
using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
using CommunityToolkit.WinUI.Controls;
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
public sealed partial class GamePlayPage : Page
{
    public GamePlayPageViewModel ViewModel { get; }

    public GamePlayPage()
    {
        ViewModel = App.GetRequiredService<GamePlayPageViewModel>();
        DataContext = this;
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.Game = e.Parameter as GameInfo;
        if (!ViewModel.Game.Cover.IsNullOrEmpty() && !ViewModel.Game.Cover.EndsWith(".webp"))
        {
            var colors = await Task.Run(() =>
            {
                var primaryColor = ColorHelper.GetImagePrimaryColor(ViewModel.Game.Cover);
                var secondColor = primaryColor;
                if (ColorHelper.IsHarshColor(primaryColor))
                {
                    secondColor = ColorHelper.GenerateLessHarshColor(primaryColor);
                    primaryColor = ColorHelper.GenerateLighterOrDarkerColor(secondColor, false, 20);
                }
                else
                {
                    secondColor = ColorHelper.GenerateLighterOrDarkerColor(primaryColor);
                }

                return (
                    ColorHelper.ConvertToWindowsColor(primaryColor),
                    ColorHelper.ConvertToWindowsColor(secondColor)
                );
            });
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 1),
                EndPoint = new Point(0.5, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = colors.Item1, Offset = 0.0 },
                    new GradientStop { Color = colors.Item2, Offset = 1.0 },
                },
            };
            this.Background = brush;
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

    private async Task PlayGame(GameInfo gameInfo, string leGuid)
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

        targetInfo.LastPlayDate = DateTime.Now;
        targetInfo.SaveJsonFile();

        gameInfo.LastPlayDate = targetInfo.LastPlayDate;
        gameInfo.SaveJsonFile();

        TimeSpan pauseTime = TimeSpan.Zero;
        var savePlayedTime = () =>
        {
            var timeCost = TimeSpan.FromSeconds(1);

            if (gameInfo.PlayStatus == PlayStatus.Pause)
                pauseTime += timeCost;

            if (gameInfo.PlayStatus == PlayStatus.Stop || gameInfo.PlayStatus == PlayStatus.Pause)
                return;

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

        // 有可能时启动器之类的，等待所有子线程结束
        gameInfo.PlayCancelTokenSource = new CancellationTokenSource();
        try
        {
            await gameProcess.WaitForAllExitAsync(gameInfo.PlayCancelTokenSource.Token);
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

        gameInfo.AddPlayedPeriod(new(gameInfo.LastPlayDate, DateTime.Now, pauseTime));
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

    private async void play_Button_Click(object sender, RoutedEventArgs e)
    {
        await PlayGame(ViewModel.Game, "");
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
        await PlayGame(ViewModel.Game, menuItem.Tag as string);
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
        ViewModel.Game.LoadScreenCapture();
        if (ViewModel.Game.ScreenCaptures.Count > 0)
            App.ShowImages(ViewModel.Game.ScreenCaptures, 0);
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
}
