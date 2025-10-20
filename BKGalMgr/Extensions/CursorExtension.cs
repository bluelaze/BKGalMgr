using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BKGalMgr.Behaviors;
using Microsoft.UI.Input;

namespace BKGalMgr.Extensions;

public static class CursorExtension
{
    public static readonly DependencyProperty CursorProperty = DependencyProperty.Register(
        "Cursor",
        typeof(InputSystemCursorShape),
        typeof(CursorExtension),
        new PropertyMetadata(null, CursorChanged)
    );

    public static void SetCursor(FrameworkElement element, InputSystemCursorShape value)
    {
        element.SetValue(CursorProperty, value);
    }

    public static InputSystemCursorShape GetCursor(FrameworkElement element)
    {
        return (InputSystemCursorShape)element.GetValue(CursorProperty);
    }

    private static void CursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // https://github.com/microsoft/WindowsAppSDK/discussions/1816#discussioncomment-3430802
        var element = d as UIElement;
        if (element == null)
            return;

        InputCursor cursor = InputSystemCursor.Create((InputSystemCursorShape)e.NewValue);
        Type type = typeof(FrameworkElement);
        type.InvokeMember(
            "ProtectedCursor",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance,
            null,
            element,
            new object[] { cursor }
        );
    }
}
