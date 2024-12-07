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
using Microsoft.UI.Dispatching;
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
    private Mutex _singleInstanceMutex;

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
                services.AddSingleton<LibraryAndManagePageViewModel>();
                services.AddSingleton<SettingsPageViewModel>();
                services.AddTransient<ReviewPageViewModel>();
                services.AddTransient<MigratePageViewModel>();
                // Models
                services.AddSingleton<SettingsDto>();
                // Servces
                services.AddSingleton<BangumiService>();
                services.AddSingleton<UpdateService>();
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

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        if (ExistLaunchedApp())
            return;

        _host.StartAsync();
        SettingsPageViewModel.ApplyLanguage(GetRequiredService<SettingsDto>());
        GetRequiredService<SettingsPageViewModel>().ApplyTheme();

        MainWindow.Closed += MainWindow_Closed;
        MainWindow.ShowWindow();
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        GetRequiredService<SettingsPageViewModel>().ApplySettings();
    }

    public static MainWindow MainWindow => GetRequiredService<MainWindow>();

    public static void PostUITask(DispatcherQueueHandler task)
    {
        MainWindow.DispatcherQueue.TryEnqueue(task);
    }

    public static T GetRequiredService<T>()
        where T : class
    {
        return _host.Services.GetRequiredService(typeof(T)) as T;
    }

    public static void ShowLoading() => MainWindow.ShowLoading();

    public static void HideLoading() => MainWindow.HideLoading();

    public static void ShowImages(IEnumerable<string> images, int selectedIndex) =>
        MainWindow.ShowImages(images, selectedIndex);

    public static CompressionLevel ZipLevel() => GetRequiredService<SettingsDto>().ZipLevel;

    private bool ExistLaunchedApp()
    {
        // https://stackoverflow.com/questions/14506406/wpf-single-instance-best-practices
        // csharpier-ignore-start
        string singleName = Directory.GetCurrentDirectory().MD5();
        EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, singleName.Substring(24));

        _singleInstanceMutex = new Mutex(true, singleName, out bool isOwned);
        if (isOwned)
        {
            GC.KeepAlive(_singleInstanceMutex);
            var thread = new Thread(() =>
            {
                while (eventWaitHandle.WaitOne())
                {
                    PostUITask(() => MainWindow.ShowWindow());
                }
            });

            thread.IsBackground = true;
            thread.Start();
            return false;
        }

        eventWaitHandle.Set();
        Exit();
        return true;
        // csharpier-ignore-end
    }
}
