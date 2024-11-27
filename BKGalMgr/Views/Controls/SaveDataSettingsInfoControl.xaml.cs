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

public sealed partial class SaveDataSettingsInfoControl : UserControl
{
    public SaveDataSettingsInfoControl()
    {
        this.InitializeComponent();
    }

    public bool IsValidSaveDataSettings()
    {
        var vm = this.DataContext as SaveDataSettingsInfo;
        return vm != null && vm.IsValid();
    }

    private async void pick_savedata_folder_Button_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFolder folder = await FileSystemMisc.PickFolder(new() { "*" });
        if (folder != null)
        {
            pick_savedata_folder_TextBox.Text = folder.Path;
        }
    }
}
