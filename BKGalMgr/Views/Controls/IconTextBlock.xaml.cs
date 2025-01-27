using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace BKGalMgr.Views.Controls;

public sealed partial class IconTextBlock : UserControl
{
    public IconElement Icon
    {
        get { return (IconElement)GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(IconElement),
        typeof(IconTextBlock),
        new PropertyMetadata(null)
    );

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(IconTextBlock),
        new PropertyMetadata(default(string))
    );

    public IconTextBlock()
    {
        this.InitializeComponent();
        Loaded += IconTextBlock_Loaded;
        IsEnabledChanged += IconTextBlock_IsEnabledChanged;
    }

    private void IconTextBlock_Loaded(object sender, RoutedEventArgs e)
    {
        if (IsEnabled)
            VisualStateManager.GoToState(this, "Enabled", false);
        else
            VisualStateManager.GoToState(this, "Disabled", false);
    }

    private void IconTextBlock_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsEnabled)
            VisualStateManager.GoToState(this, "Enabled", false);
        else
            VisualStateManager.GoToState(this, "Disabled", false);
    }

    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "PointerOver", false);
        base.OnPointerEntered(e);
    }

    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "Normal", false);
        base.OnPointerExited(e);
    }

    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "Pressed", false);
        base.OnPointerPressed(e);
    }
}
