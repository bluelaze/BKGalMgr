using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Helpers;
using Windows.Foundation.Collections;

namespace BKGalMgr.Triggers;

// 自定义，增加CanTrigger
// https://github.com/CommunityToolkit/Windows/blob/main/components/Triggers/src/IsNullOrEmptyStateTrigger.cs

/// <summary>
/// Enables a state if an Object is <c>null</c> or a String/IEnumerable is empty
/// </summary>
public class IsNullOrEmptyStateTrigger : StateTriggerBase
{
    /// <summary>
    /// Gets or sets a value indicating
    /// whether this trigger will be active or not.
    /// </summary>
    public bool CanTrigger
    {
        get => (bool)GetValue(CanTriggerProperty);
        set => SetValue(CanTriggerProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="CanTrigger"/> DependencyProperty.
    /// </summary>
    public static readonly DependencyProperty CanTriggerProperty = DependencyProperty.Register(
        nameof(CanTrigger),
        typeof(bool),
        typeof(ControlSizeTrigger),
        new PropertyMetadata(true, (d, e) => ((IsNullOrEmptyStateTrigger)d).UpdateTrigger())
    );

    /// <summary>
    /// Gets or sets the value used to check for <c>null</c> or empty.
    /// </summary>
    public object Value
    {
        get { return GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="Value"/> DependencyProperty
    /// </summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(object),
        typeof(IsNullOrEmptyStateTrigger),
        new PropertyMetadata(null, OnValuePropertyChanged)
    );

    /// <summary>
    /// Creates a new instance of <see cref="IsNullOrEmptyStateTrigger"/>.
    /// </summary>
    public IsNullOrEmptyStateTrigger()
    {
        UpdateTrigger();
    }

    private new void SetActive(bool IsActive)
    {
        if (!CanTrigger)
        {
            base.SetActive(false);
            return;
        }
        base.SetActive(IsActive);
    }

    private void UpdateTrigger()
    {
        var val = Value;
        SetActive(IsNullOrEmpty(val));

        if (val == null)
        {
            return;
        }

        // Try to listen for various notification events
        // Starting with INorifyCollectionChanged
        var valNotifyCollection = val as INotifyCollectionChanged;
        if (valNotifyCollection != null)
        {
            var weakEvent = new WeakEventListener<IsNullOrEmptyStateTrigger, object, NotifyCollectionChangedEventArgs>(
                this
            )
            {
                OnEventAction = static (instance, source, args) => instance.SetActive(IsNullOrEmpty(source)),
                OnDetachAction = (weakEventListener) =>
                    valNotifyCollection.CollectionChanged -= weakEventListener.OnEvent,
            };

            valNotifyCollection.CollectionChanged += weakEvent.OnEvent;
            return;
        }

        // Not INotifyCollectionChanged, try IObservableVector
        var valObservableVector = val as IObservableVector<object>;
        if (valObservableVector != null)
        {
            var weakEvent = new WeakEventListener<IsNullOrEmptyStateTrigger, object, IVectorChangedEventArgs>(this)
            {
                OnEventAction = static (instance, source, args) => instance.SetActive(IsNullOrEmpty(source)),
                OnDetachAction = (weakEventListener) => valObservableVector.VectorChanged -= weakEventListener.OnEvent,
            };

            valObservableVector.VectorChanged += weakEvent.OnEvent;
            return;
        }

        // Not INotifyCollectionChanged, try IObservableMap
        var valObservableMap = val as IObservableMap<object, object>;
        if (valObservableMap != null)
        {
            var weakEvent = new WeakEventListener<IsNullOrEmptyStateTrigger, object, IMapChangedEventArgs<object>>(this)
            {
                OnEventAction = static (instance, source, args) => instance.SetActive(IsNullOrEmpty(source)),
                OnDetachAction = (weakEventListener) => valObservableMap.MapChanged -= weakEventListener.OnEvent,
            };

            valObservableMap.MapChanged += weakEvent.OnEvent;
        }
    }

    private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var obj = (IsNullOrEmptyStateTrigger)d;
        obj.UpdateTrigger();
    }

    private static bool IsNullOrEmpty(object val)
    {
        if (val == null)
        {
            return true;
        }

        // Object is not null, check for an empty string
        var valString = val as string;
        if (valString != null)
        {
            return valString.Length == 0;
        }

        // Object is not a string, check for an empty ICollection (faster)
        var valCollection = val as ICollection;
        if (valCollection != null)
        {
            return valCollection.Count == 0;
        }

        // Object is not an ICollection, check for an empty IEnumerable
        if (val is IEnumerable valEnumerable
            // Workaround to regression introduced in https://github.com/unoplatform/uno/pull/16834
            // Track https://github.com/unoplatform/uno/issues/17311 for cleanup
#if HAS_UNO
            && val is not FrameworkElement
#endif
        )
        {
            foreach (var item in valEnumerable)
            {
                // Found an item, not empty
                return false;
            }

            return true;
        }

        // Not null and not a known type to test for emptiness
        return false;
    }
}
