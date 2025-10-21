using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

public sealed partial class HeaderedTextBox : UserControl
{
    public string Header
    {
        get { return (string)GetValue(HeaderProperty); }
        set { SetValue(HeaderProperty, value); }
    }
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        "Header",
        typeof(string),
        typeof(HeaderedTextBox),
        new PropertyMetadata(default(string))
    );

    public double HeaderWidth
    {
        get { return (double)GetValue(HeaderWidthProperty); }
        set { SetValue(HeaderWidthProperty, value); }
    }
    public static readonly DependencyProperty HeaderWidthProperty = DependencyProperty.Register(
        "HeaderWidth",
        typeof(double),
        typeof(HeaderedTextBox),
        new PropertyMetadata(Double.NaN)
    );

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        "Text",
        typeof(string),
        typeof(HeaderedTextBox),
        new PropertyMetadata(default(string))
    );

    public string PlaceholderText
    {
        get { return (string)GetValue(PlaceholderTextProperty); }
        set { SetValue(PlaceholderTextProperty, value); }
    }
    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        "PlaceholderText",
        typeof(string),
        typeof(HeaderedTextBox),
        new PropertyMetadata(default(string))
    );

    public double TextWidth
    {
        get { return (double)GetValue(TextWidthProperty); }
        set { SetValue(TextWidthProperty, value); }
    }
    public static readonly DependencyProperty TextWidthProperty = DependencyProperty.Register(
        "TextWidth",
        typeof(double),
        typeof(HeaderedTextBox),
        new PropertyMetadata(Double.NaN)
    );

    public bool IsReadOnly
    {
        get { return (bool)GetValue(IsReadOnlyProperty); }
        set { SetValue(IsReadOnlyProperty, value); }
    }
    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        "IsReadOnly",
        typeof(bool),
        typeof(HeaderedTextBox),
        new PropertyMetadata(default(bool))
    );

    public bool AcceptsReturn
    {
        get { return (bool)GetValue(AcceptsReturnProperty); }
        set { SetValue(AcceptsReturnProperty, value); }
    }
    public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register(
        "AcceptsReturn",
        typeof(bool),
        typeof(HeaderedTextBox),
        new PropertyMetadata(default(bool))
    );

    public TextWrapping TextWrapping
    {
        get { return (TextWrapping)GetValue(TextWrappingProperty); }
        set { SetValue(TextWrappingProperty, value); }
    }
    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
        "TextWrapping",
        typeof(TextWrapping),
        typeof(HeaderedTextBox),
        new PropertyMetadata(default(TextWrapping))
    );

    public bool CanEmpty
    {
        get { return (bool)GetValue(CanEmptyProperty); }
        set { SetValue(CanEmptyProperty, value); }
    }
    public static readonly DependencyProperty CanEmptyProperty = DependencyProperty.Register(
        "CanEmpty",
        typeof(bool),
        typeof(HeaderedTextBox),
        new PropertyMetadata(true)
    );

    public HeaderedTextBox()
    {
        this.InitializeComponent();
    }
}
