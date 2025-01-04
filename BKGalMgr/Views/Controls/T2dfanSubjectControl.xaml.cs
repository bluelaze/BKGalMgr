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

public sealed partial class T2DFanSubjectControl : UserControl
{
    public string SubjectUrl
    {
        get { return (string)GetValue(SubjectUrlProperty); }
        set { SetValue(SubjectUrlProperty, value); }
    }
    public static readonly DependencyProperty SubjectUrlProperty = DependencyProperty.Register(
        nameof(SubjectUrl),
        typeof(string),
        typeof(T2DFanSubjectControl),
        new PropertyMetadata(default(string))
    );

    public T2DFanSubjectControl()
    {
        this.InitializeComponent();
    }
}
