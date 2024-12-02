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

public sealed partial class CharacterInfoControl : UserControl
{
    public CharacterInfoControl()
    {
        this.InitializeComponent();
    }

    public bool IsValidCharacter()
    {
        var vm = this.DataContext as CharacterInfo;
        return vm != null && !vm.Name.IsNullOrEmpty();
    }

    private async void pick_illustration_Button_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFile file = await FileSystemMisc.PickFile(GlobalInfo.GameCoverSupportFormats.ToList());
        if (file != null)
        {
            pick_illustration_HeaderedTextBox.Text = file.Name;
        }
    }

    private void remove_birthday_Button_Click(object sender, RoutedEventArgs e)
    {
        if(this.DataContext is CharacterInfo c)
        {
            c.Birthday = new();
        }
    }
}
