using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BKGalMgr.Views.Controls;

public sealed partial class HeaderedTextBox : UserControl, INotifyPropertyChanged
{
    public string Header
    {
        get { return (string)GetValue(HeaderProperty); }
        set { SetValue(HeaderProperty, value); }
    }
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(HeaderedTextBox), new PropertyMetadata(default(string)));

    public double HeaderWidth
    {
        get { return (double)GetValue(HeaderWidthProperty); }
        set { SetValue(HeaderWidthProperty, value); }
    }
    public static readonly DependencyProperty HeaderWidthProperty = DependencyProperty.Register("HeaderWidth", typeof(double), typeof(HeaderedTextBox), new PropertyMetadata(Double.NaN));

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(HeaderedTextBox), new PropertyMetadata(default(string), (DependencyObject obj, DependencyPropertyChangedEventArgs args) =>
    {
        var textBox = obj as HeaderedTextBox;
        if (textBox != null)
        {
            textBox.NotifyPropertyChanged(nameof(FakeText));
        }
    }));

    public string PlaceholderText
    {
        get { return (string)GetValue(PlaceholderTextProperty); }
        set { SetValue(PlaceholderTextProperty, value); }
    }
    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register("PlaceholderText", typeof(string), typeof(HeaderedTextBox), new PropertyMetadata(default(string)));


    public double TextWidth
    {
        get { return (double)GetValue(TextWidthProperty); }
        set { SetValue(TextWidthProperty, value); }
    }
    public static readonly DependencyProperty TextWidthProperty = DependencyProperty.Register("TextWidth", typeof(double), typeof(HeaderedTextBox), new PropertyMetadata(Double.NaN));

    public bool IsReadOnly
    {
        get { return (bool)GetValue(IsReadOnlyProperty); }
        set { SetValue(IsReadOnlyProperty, value); }
    }
    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(HeaderedTextBox), new PropertyMetadata(default(bool)));

    public bool AcceptsReturn
    {
        get { return (bool)GetValue(AcceptsReturnProperty); }
        set { SetValue(AcceptsReturnProperty, value); }
    }
    public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register("AcceptsReturn", typeof(bool), typeof(HeaderedTextBox), new PropertyMetadata(default(bool)));

    public TextWrapping TextWrapping
    {
        get { return (TextWrapping)GetValue(TextWrappingProperty); }
        set { SetValue(TextWrappingProperty, value); }
    }
    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(bool), typeof(HeaderedTextBox), new PropertyMetadata(default(TextWrapping)));

    public bool CanEmpty
    {
        get { return (bool)GetValue(CanEmptyProperty); }
        set { SetValue(CanEmptyProperty, value); }
    }
    public static readonly DependencyProperty CanEmptyProperty = DependencyProperty.Register("CanEmpty", typeof(bool), typeof(HeaderedTextBox), new PropertyMetadata(true, (DependencyObject obj, DependencyPropertyChangedEventArgs args) =>
    {
        var textBox = obj as HeaderedTextBox;
        if (textBox != null)
        {
            textBox.NotifyPropertyChanged(nameof(FakeText));
        }
    }));

    public string FakeText
    {
        get { return CanEmpty ? "FakeText" : (string)GetValue(TextProperty); }
        set { SetValue(FakeTextProperty, value); }
    }
    public static readonly DependencyProperty FakeTextProperty = DependencyProperty.Register("FakeText", typeof(string), typeof(HeaderedTextBox), new PropertyMetadata(default(string)));

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public HeaderedTextBox()
    {
        this.InitializeComponent();
    }
}
