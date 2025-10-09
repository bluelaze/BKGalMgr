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

public sealed partial class LocalizationInfoControl : UserControl
{
    public string PickFilePath { get; set; }

    public LocalizationInfoControl()
    {
        this.InitializeComponent();
    }

    public bool IsValidLocalization()
    {
        var vm = this.DataContext as LocalizationInfo;
        return vm != null && vm.IsValid();
    }

    private void pick_startup_name_button_Click(object sender, RoutedEventArgs e)
    {
        var files = FileSystemMisc.PickFile(PickFilePath, ["Executable file|*.exe", "Shortcut|*.lnk"]);
        if (files?.Any() == true)
        {
            pick_startup_name_headeredtextbox.Text = Path.GetFileName(files.First());
        }
    }

    private void add_contributor_button_Click(object sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as LocalizationInfo;
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
            (this.DataContext as LocalizationInfo).Contributors.Remove(ctbtor);
        }
    }
}
