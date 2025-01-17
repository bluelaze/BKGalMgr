using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace BKGalMgr.Views.Controls;

public sealed partial class ScrollableFlipView : FlipView
{
    public bool IsFlipEnabled
    {
        get { return (bool)GetValue(IsFlipEnabledProperty); }
        set { SetValue(IsFlipEnabledProperty, value); }
    }
    public static readonly DependencyProperty IsFlipEnabledProperty = DependencyProperty.Register(
        nameof(IsFlipEnabled),
        typeof(bool),
        typeof(ScrollableFlipView),
        new PropertyMetadata(false)
    );

    public ScrollableFlipView()
    {
        this.DefaultStyleKey = typeof(FlipView);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        // 按钮样式会失效，重新赋值
        if (GetTemplateChild("PreviousButtonHorizontal") is Button leftButton)
        {
            var temp = (ControlTemplate)App.Current.Resources["FlipViewLeftButtonTemplate"];
            leftButton.Template = temp;
        }
        if (GetTemplateChild("NextButtonHorizontal") is Button rightButton)
        {
            var temp = (ControlTemplate)App.Current.Resources["FlipViewRightButtonTemplate"];
            rightButton.Template = temp;
        }
    }

    protected override void OnPointerWheelChanged(PointerRoutedEventArgs e)
    {
        if (IsFlipEnabled)
            base.OnPointerWheelChanged(e);
    }
}
