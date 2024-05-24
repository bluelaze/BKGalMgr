using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
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
    private GamesManagePageViewModel _viewModel;
    public GamesManagePageViewModel ViewModel
    {
        get { return _viewModel; }
    }

    public GamesPage()
    {
        _viewModel = App.GetRequiredService<GamesManagePageViewModel>();
        DataContext = this;
        this.InitializeComponent();
    }

    private void linkbtn_gamename_Click(object sender, RoutedEventArgs e)
    {
        var gameInfo = (sender as FrameworkElement).DataContext as GameInfo;
        if (gameInfo != null)
        {
            ViewModel.SelectedRepository.SelectedGame = gameInfo;
            MainPage.NavigateTo(typeof(ManagePage));
        }
    }

    private async void btn_play_Click(object sender, RoutedEventArgs e)
    {
        var playBtn = sender as Button;
        var gameInfo = playBtn.DataContext as GameInfo;
        var targetInfo = gameInfo.SelectedTarget;
        if (targetInfo != null)
        {
            if (!File.Exists(targetInfo.TargetExePath))
            {
                if (targetInfo.IsArchive && await App.ShowDialogConfirm("Target is archive, confirm to deachive?"))
                {
                    App.ShowLoading();
                    await targetInfo.DeArchive();
                    App.HideLoading();
                }
                if (!File.Exists(targetInfo.TargetExePath))
                {
                    App.ShowDialogError($"Invalid TargetExePath: {targetInfo.TargetExePath}");
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
                App.ShowDialogError($"Process Start failed, TargetExePath: {targetInfo.TargetExePath}");
            }
            else
            {
                targetInfo.LastPlayDate = DateTime.Now;
                targetInfo.SaveJsonFile();

                gameInfo.LastPlayDate = targetInfo.LastPlayDate;
                gameInfo.SaveJsonFile();

                var savePlayedTime = () =>
                {
                    var timeCost = TimeSpan.FromSeconds(1);

                    targetInfo.PlayedTime += timeCost;
                    targetInfo.SaveJsonFile();

                    gameInfo.PlayedTime += timeCost;
                    gameInfo.SaveJsonFile();
                };

                var loopTimer = new Timer(
                    (_) =>
                    {
                        DispatcherQueue?.TryEnqueue(() =>
                        {
                            savePlayedTime();
                        });
                    },
                    null,
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1)
                );

                gameInfo.IsPlaying = true;
                targetInfo.IsPlaying = true;
                await gameProcess.WaitForExitAsync();
                gameInfo.IsPlaying = false;
                targetInfo.IsPlaying = false;

                gameInfo.PlayedPeriods.Insert(0, new(gameInfo.LastPlayDate, DateTime.Now));
                gameInfo.SaveJsonFile();

                loopTimer.Change(Timeout.Infinite, Timeout.Infinite);
                loopTimer.Dispose();
            }
        }
    }
}
