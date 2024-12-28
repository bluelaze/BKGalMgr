using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BKGalMgr.ViewModels;
using BKGalMgr.ViewModels.Pages;
using CommunityToolkit.WinUI.Controls;
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

namespace BKGalMgr.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class GamePlayPage : Page
{
    public GamePlayPageViewModel ViewModel { get; }

    public GamePlayPage()
    {
        ViewModel = App.GetRequiredService<GamePlayPageViewModel>();
        DataContext = this;
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.Game = e.Parameter as GameInfo;
    }

    private void SegmentedItem_Loaded(object sender, RoutedEventArgs e)
    {
        var segmentedItem = sender as SegmentedItem;
        if (segmentedItem.FindDescendant("PART_Hover") is Border ele)
        {
            ele.CornerRadius = new(12);
        }
    }

    private void back_Button_Click(object sender, RoutedEventArgs e)
    {
        App.MainWindow.NavigateToMainPage();
    }
}
