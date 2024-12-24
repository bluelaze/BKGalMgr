using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using BKGalMgr.Helpers;
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
public sealed partial class HomePage : Page
{
    private int bannerLoopCount = 0;

    private IDisposable dateTimer;

    public HomePageViewModel ViewModel { get; }

    public HomePage()
    {
        ViewModel = App.GetRequiredService<HomePageViewModel>();
        DataContext = this;
        this.InitializeComponent();
    }

    private void StartTimer()
    {
        ViewModel.CurrentDate += TimeSpan.FromSeconds(1);
        dateTimer = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .ObserveOn(SynchronizationContext.Current)
            .Subscribe(_ =>
            {
                ViewModel.CurrentDate += TimeSpan.FromSeconds(1);
                // 4秒一次切图
                bannerLoopCount++;
                bannerLoopCount %= 4;
                if (bannerLoopCount == 0 && ViewModel.BannersCount != 0)
                    banner_FlipView.SelectedIndex = (banner_FlipView.SelectedIndex + 1) % ViewModel.BannersCount;
                UpdateTimePriod();
            });
    }

    private void UpdateTimePriod()
    {
        // 时段切换
        var hour = ViewModel.CurrentDate.TimeOfDay.Hours;
        switch (hour)
        {
            case >= 23:
            case < 1:
                greeting_TextBlock.Text = LanguageHelper.GetString("Home_Greeting_Midnight");
                time_period_TextBlock.Text = LanguageHelper.GetString("Home_Time_Period_Midnight");
                break;
            case >= 20:
                greeting_TextBlock.Text = LanguageHelper.GetString("Home_Greeting_Night");
                time_period_TextBlock.Text = LanguageHelper.GetString("Home_Time_Period_Night");
                break;
            case >= 18:
                greeting_TextBlock.Text = LanguageHelper.GetString("Home_Greeting_Evening");
                time_period_TextBlock.Text = LanguageHelper.GetString("Home_Time_Period_Evening");
                break;
            case >= 13:
                greeting_TextBlock.Text = LanguageHelper.GetString("Home_Greeting_Afternoon");
                time_period_TextBlock.Text = LanguageHelper.GetString("Home_Time_Period_Afternoon");
                break;
            case >= 11:
                greeting_TextBlock.Text = LanguageHelper.GetString("Home_Greeting_Noon");
                time_period_TextBlock.Text = LanguageHelper.GetString("Home_Time_Period_Noon");
                break;
            case >= 9:
                greeting_TextBlock.Text = LanguageHelper.GetString("Home_Greeting_Morning");
                time_period_TextBlock.Text = LanguageHelper.GetString("Home_Time_Period_Morning");
                break;
            case >= 6:
                greeting_TextBlock.Text = LanguageHelper.GetString("Home_Greeting_Early_Morning");
                time_period_TextBlock.Text = LanguageHelper.GetString("Home_Time_Period_Early_Morning");
                break;
            case >= 5:
                greeting_TextBlock.Text = LanguageHelper.GetString("Home_Greeting_Predawn");
                time_period_TextBlock.Text = LanguageHelper.GetString("Home_Time_Period_Predawn");
                break;
            case >= 1:
                greeting_TextBlock.Text = LanguageHelper.GetString("Home_Greeting_Late_Night");
                time_period_TextBlock.Text = LanguageHelper.GetString("Home_Time_Period_Late_Night");
                break;
            default:
                break;
        }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (!ViewModel.LibraryAndManagePageViewModel.IsLoadedRepository)
        {
            App.ShowLoading();

            await ViewModel.LibraryAndManagePageViewModel.LoadRepository();
            ViewModel.Refresh();

            // 比较准确的开启每秒计时
            ViewModel.CurrentDate = DateTime.Now;
            Observable
                .Timer(TimeSpan.FromMilliseconds(1000 - ViewModel.CurrentDate.Millisecond))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(_ => StartTimer());
            UpdateTimePriod();

            App.HideLoading();
        }
        else if (!ViewModel.Banners.Any())
        {
            ViewModel.Refresh();
        }
    }

    private void theme_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        switch ((sender as MenuFlyoutItem).Tag as string)
        {
            case "Dark":
                App.GetRequiredService<SettingsPageViewModel>().AppTheme = Helpers.Theme.Dark;
                break;
            case "Light":
                App.GetRequiredService<SettingsPageViewModel>().AppTheme = Helpers.Theme.Light;
                break;
        }
    }

    private void repository_GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        var gameInfo = e.ClickedItem as GameInfo;
        if (gameInfo != null)
        {
            ViewModel.LibraryAndManagePageViewModel.SelectedRepository = gameInfo.Repository;
            MainPage.NavigateTo(typeof(LibraryAndManagePage));
        }
    }
}
