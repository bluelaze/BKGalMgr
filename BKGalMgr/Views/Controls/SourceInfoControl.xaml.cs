using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BKGalMgr.ViewModels;
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

public sealed partial class SourceInfoControl : UserControl
{
    public SourceInfoControl()
    {
        this.InitializeComponent();
    }

    public bool IsValidSource()
    {
        var source = this.DataContext as SourceInfo;
        return source != null && source.IsValid();
    }

    private async void pick_startup_name_button_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFile file = await FileSystemMisc.PickFile(new() { ".exe", ".lnk" });
        if (file != null)
        {
            pick_startup_name_headeredtextbox.Text = file.Name;
        }
    }

    private void add_contributor_button_Click(object sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as SourceInfo;
        if (vm != null)
        {
            vm.Contributors.Add(new());
        }
    }

    private void ContributorInfoControl_Delete(object sender, RoutedEventArgs e)
    {
        var ele = sender as ContributorInfoControl;
        var ctbtor = ele.DataContext as ContributorInfo;
        if (ctbtor != null)
        {
            (this.DataContext as SourceInfo).Contributors.Remove(ctbtor);
        }
    }
}
