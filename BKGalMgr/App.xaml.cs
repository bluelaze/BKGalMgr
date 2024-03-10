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
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .ConfigureAppConfiguration(c => { c.SetBasePath(AppContext.BaseDirectory); })
        .ConfigureServices((context, services) =>
        {
            // App Host
            services.AddHostedService<ApplicationHostService>();
            // Main window container with navigation
            services.AddSingleton<MainWindow>();
            // ViewModels
            services.AddSingleton<GamesManagePageViewModel>();
            services.AddSingleton<SettingsPageViewModel>();
            // Models
            services.AddSingleton<SettingsModel>();
        }).Build();

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _host.StartAsync();
        GetRequiredService<SettingsPageViewModel>().ApplyTheme();

        MainWindow.Activate();
        MainWindow.Closed += MainWindow_Closed;
    }

    public static MainWindow MainWindow => GetRequiredService<MainWindow>();

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        GetRequiredService<SettingsModel>().SaveSettings();
    }

    public static T GetRequiredService<T>() where T : class
    {
        return _host.Services.GetRequiredService(typeof(T)) as T;
    }

    public static void ShowLoading() => MainWindow.ShowLoading();
    public static void HideLoading() => MainWindow.HideLoading();

    public static CompressionLevel ZipLevel() => GetRequiredService<SettingsModel>().LoadedSettings.ZipLevel;

    public static async void ShowDialogError(string errorMsg)
    {
        ContentDialog dialog = new ContentDialog();

        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        dialog.XamlRoot = MainWindow.Content.XamlRoot;
        dialog.Title = new FontIcon() { Foreground = (SolidColorBrush)Current.Resources["SystemFillColorCriticalBrush"], Glyph = "\uE783" };
        dialog.PrimaryButtonText = "Confirm";
        dialog.Content = errorMsg;
        dialog.RequestedTheme = MainWindow.RequestedTheme();

        await dialog.ShowAsync();
    }

    public static async Task<bool> ShowDialogConfirm(string confirmMsg)
    {
        ContentDialog dialog = new ContentDialog();

        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        dialog.XamlRoot = MainWindow.Content.XamlRoot;
        dialog.Title = new FontIcon() { Foreground = (SolidColorBrush)Current.Resources["SystemFillColorCautionBrush"], Glyph = "\uE7BA" };
        dialog.PrimaryButtonText = "Confirm";
        dialog.CloseButtonText = "Cancel";
        dialog.Content = confirmMsg;
        dialog.RequestedTheme = MainWindow.RequestedTheme();

        return ContentDialogResult.Primary == await dialog.ShowAsync();
    }
}
