using CommunityToolkit.WinUI.Animations;
using CommunityToolkit.WinUI.Behaviors;
using Microsoft.Xaml.Interactivity;

namespace BKGalMgr.Animations;

/// <summary>
/// 封装这个的原因是CommunityToolkit里的StartAnimationAction，当Animation为null时，就会抛异常，
/// 如果元素未加载完成，却因事件触发了动画，就会导致异常发生。
/// </summary>
public sealed partial class StartAnimationActionWrapper : DependencyObject, IAction
{
    public AnimationSet Animation
    {
        get => (AnimationSet)GetValue(AnimationProperty);
        set => SetValue(AnimationProperty, value);
    }

    public static readonly DependencyProperty AnimationProperty = DependencyProperty.Register(
        nameof(Animation),
        typeof(AnimationSet),
        typeof(StartAnimationActionWrapper),
        new PropertyMetadata(null)
    );

    public UIElement TargetObject
    {
        get => (UIElement)GetValue(TargetObjectProperty);
        set => SetValue(TargetObjectProperty, value);
    }

    public static readonly DependencyProperty TargetObjectProperty = DependencyProperty.Register(
        nameof(TargetObject),
        typeof(UIElement),
        typeof(StartAnimationActionWrapper),
        new PropertyMetadata(null)
    );

    private StartAnimationAction _action;

    public StartAnimationActionWrapper()
    {
        _action = new();
    }

    public object Execute(object sender, object parameter)
    {
        if (Animation is null)
            return null;

        _action.Animation = Animation;
        _action.TargetObject = TargetObject;

        return _action.Execute(sender, parameter);
    }
}
