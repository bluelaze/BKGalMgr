using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.Configuration;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.Helpers;

// https://learn.microsoft.com/zh-cn/windows/apps/windows-app-sdk/system-backdrop-controller
// https://learn.microsoft.com/zh-cn/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.media.systembackdrop

public enum Theme
{
    Default,
    Light,
    Dark,
}

public enum BackdropMaterial
{
    Mica,
    Mica_Alt,
    Acrylic,
    Acrylic_Base,
    Acrylic_Thin,
}

public abstract class ISystemBackdrop : SystemBackdrop
{
    public object Kind { get; set; }
    public SystemBackdropTheme Theme { get; set; }

    protected SystemBackdropConfiguration _configurationSource;

    public virtual void ChangeKind(object kind) { }

    public virtual void ChangeTheme(SystemBackdropTheme theme)
    {
        if (_configurationSource != null)
            _configurationSource.Theme = theme;
    }

    public virtual void WindowActivated(object sender, WindowActivatedEventArgs args)
    {
        if (_configurationSource != null)
            _configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
    }
}

public class MicaSystemBackdrop : ISystemBackdrop
{
    MicaController _micaController;

    protected override void OnTargetConnected(ICompositionSupportsSystemBackdrop connectedTarget, XamlRoot xamlRoot)
    {
        // Call the base method to initialize the default configuration object.
        base.OnTargetConnected(connectedTarget, xamlRoot);

        // This example does not support sharing MicaSystemBackdrop instances.
        if (_micaController is not null)
        {
            throw new Exception("This controller cannot be shared");
        }

        // Set configuration.
        _configurationSource = GetDefaultSystemBackdropConfiguration(connectedTarget, xamlRoot);
        _configurationSource.Theme = Theme;

        // Add target.
        _micaController = new MicaController() { Kind = (MicaKind)Kind };
        _micaController.SetSystemBackdropConfiguration(_configurationSource);
        _micaController.AddSystemBackdropTarget(connectedTarget);
    }

    protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget)
    {
        base.OnTargetDisconnected(disconnectedTarget);

        //_micaController.RemoveSystemBackdropTarget(disconnectedTarget);
        _micaController.Dispose();
        _micaController = null;
    }

    protected override void OnDefaultSystemBackdropConfigurationChanged(
        ICompositionSupportsSystemBackdrop target,
        XamlRoot xamlRoot
    )
    {
        //_configurationSource = new SystemBackdropConfiguration() { Theme = _configurationSource.Theme };
        //_micaController.SetSystemBackdropConfiguration(_configurationSource);
    }

    public override void ChangeKind(object kind)
    {
        _micaController.Kind = (MicaKind)kind;
    }
}

public class AcrylicSystemBackdrop : ISystemBackdrop
{
    DesktopAcrylicController _acrylicController;

    protected override void OnTargetConnected(ICompositionSupportsSystemBackdrop connectedTarget, XamlRoot xamlRoot)
    {
        // Call the base method to initialize the default configuration object.
        base.OnTargetConnected(connectedTarget, xamlRoot);

        // This example does not support sharing MicaSystemBackdrop instances.
        if (_acrylicController is not null)
        {
            throw new Exception("This controller cannot be shared");
        }

        // Set configuration.
        _configurationSource = GetDefaultSystemBackdropConfiguration(connectedTarget, xamlRoot);
        _configurationSource.Theme = Theme;

        // Add target.
        _acrylicController = new DesktopAcrylicController() { Kind = (DesktopAcrylicKind)Kind };
        _acrylicController.SetSystemBackdropConfiguration(_configurationSource);
        _acrylicController.AddSystemBackdropTarget(connectedTarget);
    }

    protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget)
    {
        base.OnTargetDisconnected(disconnectedTarget);

        //_micaController.RemoveSystemBackdropTarget(disconnectedTarget);
        _acrylicController.Dispose();
        _acrylicController = null;
    }

    protected override void OnDefaultSystemBackdropConfigurationChanged(
        ICompositionSupportsSystemBackdrop target,
        XamlRoot xamlRoot
    )
    {
        //_configurationSource = new SystemBackdropConfiguration() { Theme = _configurationSource.Theme };
        //_micaController.SetSystemBackdropConfiguration(_configurationSource);
    }

    public override void ChangeKind(object kind)
    {
        _acrylicController.Kind = (DesktopAcrylicKind)kind;
    }
}

public class ThemeHelper
{
    private ISystemBackdrop _systemBackdrop;
    private Window _window;
    private BackdropMaterial _backdrop = BackdropMaterial.Acrylic_Thin;

    public ThemeHelper(Window window)
    {
        _window = window;
        ((FrameworkElement)_window.Content).ActualThemeChanged += Window_ActualThemeChanged;
    }

    private void Window_ActualThemeChanged(FrameworkElement sender, object args)
    {
        _systemBackdrop?.ChangeTheme(ToSystemBackdropTheme(sender.ActualTheme));
    }

    public static bool IsMicaBackdropMaterial(BackdropMaterial backdrop)
    {
        switch (backdrop)
        {
            case BackdropMaterial.Mica:
            case BackdropMaterial.Mica_Alt:
                return true;
        }
        return false;
    }

    public static SystemBackdropTheme ToSystemBackdropTheme(ElementTheme theme)
    {
        switch (theme)
        {
            case ElementTheme.Dark:
                return SystemBackdropTheme.Dark;
            case ElementTheme.Light:
                return SystemBackdropTheme.Light;
            default:
                return SystemBackdropTheme.Default;
        }
    }

    public static MicaKind ToMicaKind(BackdropMaterial backdrop)
    {
        switch (backdrop)
        {
            case BackdropMaterial.Mica_Alt:
                return MicaKind.BaseAlt;
            default:
                return MicaKind.Base;
        }
    }

    public static DesktopAcrylicKind ToAcrylicKind(BackdropMaterial backdrop)
    {
        switch (backdrop)
        {
            case BackdropMaterial.Acrylic_Base:
                return DesktopAcrylicKind.Base;
            case BackdropMaterial.Acrylic_Thin:
                return DesktopAcrylicKind.Thin;
            default:
                return DesktopAcrylicKind.Default;
        }
    }

    public void ApllyTheme(Theme theme, BackdropMaterial backdrop)
    {
        switch (theme)
        {
            case Theme.Dark:
                _window.ApplyTheme(ElementTheme.Dark);
                _window.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.Dark;
                LiveCharts.Configure(config =>
                {
                    config.AddDarkTheme();
                });
                break;
            case Theme.Light:
                _window.ApplyTheme(ElementTheme.Light);
                _window.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.Light;
                LiveCharts.Configure(config =>
                {
                    config.AddLightTheme();
                });
                break;
            default:
                _window.ApplyTheme(ElementTheme.Default);
                _window.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
                LiveCharts.Configure(config =>
                {
                    config.AddDefaultTheme();
                });
                break;
        }

        if (_systemBackdrop == null || IsMicaBackdropMaterial(_backdrop) ^ IsMicaBackdropMaterial(backdrop))
        {
            _backdrop = backdrop;
            if (IsMicaBackdropMaterial(backdrop))
                _systemBackdrop = new MicaSystemBackdrop()
                {
                    Kind = ToMicaKind(_backdrop),
                    Theme = ToSystemBackdropTheme(_window.RequestedTheme()),
                };
            else
                _systemBackdrop = new AcrylicSystemBackdrop()
                {
                    Kind = ToAcrylicKind(_backdrop),
                    Theme = ToSystemBackdropTheme(_window.RequestedTheme()),
                };
            _window.SystemBackdrop = _systemBackdrop;
            return;
        }

        if (_backdrop != backdrop)
        {
            if (IsMicaBackdropMaterial(backdrop))
                _systemBackdrop.ChangeKind(ToMicaKind(backdrop));
            else
                _systemBackdrop.ChangeKind(ToAcrylicKind(backdrop));
        }
        _backdrop = backdrop;
    }
}
