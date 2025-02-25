using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;

namespace BKGalMgr.Behaviors;

[TypeConstraint(typeof(FrameworkElement))]
public class ClickTriggerBehavior : Trigger<FrameworkElement>
{
    private bool _pointerPressed = false;

    protected override void OnAttached()
    {
        // 其他事件（而不是 PointerReleased）可能会在操作结束时触发，例如 PointerCanceled 或 PointerCaptureLost。
        AssociatedObject.PointerPressed += AssociatedObject_PointerPressed;
        AssociatedObject.PointerReleased += AssociatedObject_PointerReleased;
        AssociatedObject.PointerExited += AssociatedObject_PointerLost;
        AssociatedObject.PointerCanceled += AssociatedObject_PointerLost;
        AssociatedObject.PointerCaptureLost += AssociatedObject_PointerLost;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PointerPressed -= AssociatedObject_PointerPressed;
        AssociatedObject.PointerReleased -= AssociatedObject_PointerReleased;
        AssociatedObject.PointerExited -= AssociatedObject_PointerLost;
        AssociatedObject.PointerCanceled -= AssociatedObject_PointerLost;
        AssociatedObject.PointerCaptureLost -= AssociatedObject_PointerLost;
    }

    private void AssociatedObject_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (
            e.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse
            && e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed
        )
        {
            e.Handled = true;
            _pointerPressed = AssociatedObject.CapturePointer(e.Pointer);
        }
    }

    private void AssociatedObject_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_pointerPressed)
        {
            e.Handled = true;
            AssociatedObject.ReleasePointerCapture(e.Pointer);
            Interaction.ExecuteActions(sender, Actions, e);
        }
        _pointerPressed = false;
    }

    private void AssociatedObject_PointerLost(object sender, PointerRoutedEventArgs e)
    {
        if (_pointerPressed)
        {
            AssociatedObject.ReleasePointerCapture(e.Pointer);
        }
        _pointerPressed = false;
    }
}
