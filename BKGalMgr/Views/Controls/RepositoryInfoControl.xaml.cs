using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.AccessCache;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BKGalMgr.Views.Controls;

public sealed partial class RepositoryInfoControl : UserControl
{
    public bool FolderPathVisible
    {
        get { return (bool)GetValue(FolderPathVisibleProperty); }
        set { SetValue(FolderPathVisibleProperty, value); }
    }
    public static readonly DependencyProperty FolderPathVisibleProperty = DependencyProperty.Register(
        "FolderPathVisible",
        typeof(bool),
        typeof(RepositoryInfoControl),
        new PropertyMetadata(true)
    );

    public RepositoryInfoControl()
    {
        this.InitializeComponent();
    }

    private async void button_pick_folder_Click(object sender, RoutedEventArgs e)
    {
        Windows.Storage.StorageFolder folder = await FileSystemMisc.PickFolder(new() { "*" });
        if (folder != null)
        {
            headeredtextbox_pick_folder.Text = folder.Path;
            headeredtextbox_name.Text = folder.Name;
        }
    }
}
