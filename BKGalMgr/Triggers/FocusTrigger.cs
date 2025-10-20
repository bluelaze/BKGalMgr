using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace BKGalMgr.Triggers;

public class FocusTrigger : StateTriggerBase
{
    public bool IsReverse
    {
        get => (bool)GetValue(IsReverseProperty);
        set => SetValue(IsReverseProperty, value);
    }

    public static readonly DependencyProperty IsReverseProperty = DependencyProperty.Register(
        nameof(IsReverse),
        typeof(bool),
        typeof(ControlSizeTrigger),
        new PropertyMetadata(
            false,
            (d, e) =>
            {
                var trigger = (FocusTrigger)d;
                trigger.UpdateTrigger(trigger.IsActive);
            }
        )
    );

    public FrameworkElement TargetElement
    {
        get => (FrameworkElement)GetValue(TargetElementProperty);
        set => SetValue(TargetElementProperty, value);
    }
    public static readonly DependencyProperty TargetElementProperty = DependencyProperty.Register(
        nameof(TargetElement),
        typeof(FrameworkElement),
        typeof(FocusTrigger),
        new PropertyMetadata(null, OnTargetElementPropertyChanged)
    );

    public bool IsActive { get; private set; } = false;

    public FocusTrigger()
    {
        //UpdateTrigger(IsActive);
    }

    private static void OnTargetElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((FocusTrigger)d).UpdateTargetElement((FrameworkElement)e.OldValue, (FrameworkElement)e.NewValue);
    }

    private void UpdateTargetElement(FrameworkElement oldValue, FrameworkElement newValue)
    {
        if (oldValue != null)
        {
            oldValue.GotFocus -= OnTargetElementGotFocus;
            oldValue.LostFocus -= OnTargetElementLostFocus;
        }

        if (newValue != null)
        {
            newValue.GotFocus += OnTargetElementGotFocus;
            newValue.LostFocus += OnTargetElementLostFocus;
        }

        UpdateTrigger(IsActive);
    }

    private void OnTargetElementLostFocus(object sender, RoutedEventArgs e)
    {
        UpdateTrigger(false);
    }

    private void OnTargetElementGotFocus(object sender, RoutedEventArgs e)
    {
        UpdateTrigger(true);
    }

    // Logic to evaluate and apply trigger value
    private void UpdateTrigger(bool isFocus)
    {
        if (IsReverse)
            isFocus = !isFocus;

        if (TargetElement == null)
        {
            IsActive = false;
            SetActive(false);
            return;
        }

        IsActive = isFocus;
        SetActive(isFocus);
    }
}
