using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

public sealed partial class TargetInfoControl : UserControl
{
    public TargetInfoControl()
    {
        this.InitializeComponent();
    }

    public bool IsValidTarget()
    {
        var target = this.DataContext as TargetInfo;
        return target != null && target.IsValid();
    }

    private async void sources_listview_ItemClick(object sender, ItemClickEventArgs e)
    {
        var target = this.DataContext as TargetInfo;
        // delay for selection is updated
        await Task.Delay(33);
        if (target.Source != null)
        {
            target.SelectedSource();
        }
    }

    private async void localization_listview_ItemClick(object sender, ItemClickEventArgs e)
    {
        var target = this.DataContext as TargetInfo;
        await Task.Delay(33);
        if (target.Localization != null)
        {
            target.SelectedLocalization();
        }
    }
}
