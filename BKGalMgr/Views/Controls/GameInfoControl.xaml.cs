using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

public sealed partial class GameInfoControl : UserControl
{
    public GameInfoControl()
    {
        this.InitializeComponent();
    }

    private void TokenizingTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        var ele = sender as FrameworkElement;
        if (ele?.FindDescendant("QueryButton") is Button queryButton)
        {
            //queryButton.Visibility = Visibility.Collapsed;
            queryButton.Opacity = 0;
            queryButton.Width = 0;
        }
    }

    private void characters_tokentextbox_TokenItemAdding(
        CommunityToolkit.WinUI.Controls.TokenizingTextBox sender,
        CommunityToolkit.WinUI.Controls.TokenItemAddingEventArgs args
    )
    {
        var gameInfo = sender.DataContext as GameInfo;
        args.Item = new CharacterInfo() { Name = args.TokenText, GameFolderPath = gameInfo.FolderPath };
    }
}
