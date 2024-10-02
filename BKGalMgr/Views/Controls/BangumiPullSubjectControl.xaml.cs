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

public sealed partial class BangumiPullSubjectControl : UserControl
{
    public string AccessToken
    {
        get { return (string)GetValue(AccessTokenProperty); }
        set { SetValue(AccessTokenProperty, value); }
    }
    public static readonly DependencyProperty AccessTokenProperty = DependencyProperty.Register(
        "AccessToken",
        typeof(string),
        typeof(BangumiPullSubjectControl),
        new PropertyMetadata(default(string))
    );

    public string SubjectUrl
    {
        get { return (string)GetValue(SubjectUrlProperty); }
        set { SetValue(SubjectUrlProperty, value); }
    }
    public static readonly DependencyProperty SubjectUrlProperty = DependencyProperty.Register(
        "SubjectUrl",
        typeof(string),
        typeof(BangumiPullSubjectControl),
        new PropertyMetadata(default(string))
    );

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public BangumiPullSubjectControl()
    {
        this.InitializeComponent();
    }
}
