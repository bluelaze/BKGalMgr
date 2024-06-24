using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using BKGalMgr.Helpers;
using BKGalMgr.Models;
using BKGalMgr.Services;
using BKGalMgr.ViewModels.Pages;
using BKGalMgr.Views;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BKGalMgr;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private static readonly IHost _host = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(c =>
        {
            c.SetBasePath(AppContext.BaseDirectory);
        })
        .ConfigureServices(
            (context, services) =>
            {
                // App Host
                services.AddHostedService<ApplicationHostService>();
                // Main window container with navigation
                services.AddSingleton<MainWindow>();
                // ViewModels
                services.AddSingleton<GamesManagePageViewModel>();
                services.AddSingleton<SettingsPageViewModel>();
                // Models
                services.AddSingleton<SettingsDto>();
            }
        )
        .Build();

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    Mutex _mutex;

    private bool ExistLaunchedApp()
    {
        //https://stackoverflow.com/questions/14506406/wpf-single-instance-best-practices
        bool isOwned;
        _mutex = new Mutex(true, Directory.GetCurrentDirectory().MD5(), out isOwned);
        EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "MainWindowWake");

        // So, R# would not give a warning that this variable is not used.
        GC.KeepAlive(_mutex);

        if (isOwned)
        {
            // Spawn a thread which will be waiting for our event
            var thread = new Thread(() =>
            {
                while (eventWaitHandle.WaitOne())
                {
                    MainWindow.DispatcherQueue.TryEnqueue(() => MainWindow.Show());
                }
            });

            // It is important mark it as background otherwise it will prevent app from exiting.
            thread.IsBackground = true;

            thread.Start();
            return false;
        }

        // Notify other instance so it could bring itself to foreground.
        eventWaitHandle.Set();

        // Terminate this instance.
        Exit();

        return true;
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        if (ExistLaunchedApp())
            return;

        _host.StartAsync();
        GetRequiredService<SettingsPageViewModel>().ApplyTheme();

        MainWindow.Closed += MainWindow_Closed;
        MainWindow.Show();
    }

    public static MainWindow MainWindow => GetRequiredService<MainWindow>();

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        GetRequiredService<SettingsDto>().SaveSettings();
    }

    public static T GetRequiredService<T>()
        where T : class
    {
        return _host.Services.GetRequiredService(typeof(T)) as T;
    }

    public static void ShowLoading() => MainWindow.ShowLoading();

    public static void HideLoading() => MainWindow.HideLoading();

    public static CompressionLevel ZipLevel() => GetRequiredService<SettingsDto>().ZipLevel;

    public static async void ShowDialogError(string errorMsg)
    {
        ContentDialog dialog = new ContentDialog();

        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        dialog.XamlRoot = MainWindow.Content.XamlRoot;
        dialog.Title = new FontIcon()
        {
            Foreground = (SolidColorBrush)Current.Resources["SystemFillColorCriticalBrush"],
            Glyph = "\uE783"
        };
        dialog.RequestedTheme = MainWindow.RequestedTheme();
        dialog.PrimaryButtonText = LanguageHelper.GetString("Dlg_Confirm");
        dialog.Content = errorMsg;
        dialog.DefaultButton = ContentDialogButton.Primary;

        await dialog.ShowAsync();
    }

    public static async Task<bool> ShowDialogConfirm(string confirmMsg)
    {
        ContentDialog dialog = new ContentDialog();

        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        dialog.XamlRoot = MainWindow.Content.XamlRoot;
        dialog.Title = new FontIcon()
        {
            Foreground = (SolidColorBrush)Current.Resources["SystemFillColorCautionBrush"],
            Glyph = "\uE7BA"
        };
        dialog.RequestedTheme = MainWindow.RequestedTheme();
        dialog.PrimaryButtonText = LanguageHelper.GetString("Dlg_Confirm");
        dialog.CloseButtonText = LanguageHelper.GetString("Dlg_Cancel");
        dialog.Content = confirmMsg;
        dialog.DefaultButton = ContentDialogButton.Primary;

        return ContentDialogResult.Primary == await dialog.ShowAsync();
    }
}
