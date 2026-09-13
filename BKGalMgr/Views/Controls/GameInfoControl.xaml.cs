using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BKGalMgr.Helpers;
using BKGalMgr.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BKGalMgr.Views.Controls;

public sealed partial class GameInfoControl : UserControl
{
    public GameInfo ViewModel
    {
        get { return (GameInfo)GetValue(ViewModelProperty); }
        set { SetValue(ViewModelProperty, value); }
    }
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(GameInfo),
        typeof(GameInfoControl),
        new PropertyMetadata(null)
    );

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
        if (ele?.FindDescendant<TextBox>() is TextBox innerTextBox)
        {
            var flyout = new MenuFlyout();
            flyout.Items.Add(new MenuFlyoutItem { Command = new StandardUICommand(StandardUICommandKind.Paste) });

            var customPasteItem = new MenuFlyoutItem
            {
                Text = LanguageHelper.GetString("Commom_MenuItem_PasteWithSpaceSplite/Text"),
            };
            customPasteItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                var dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    string text = await dataPackageView.GetTextAsync();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        innerTextBox.Text = string.Join(',', text.Split(' ')) + ",";
                    }
                }
            };

            flyout.Items.Add(customPasteItem);
            innerTextBox.ContextFlyout = flyout;
        }
    }

    private void characters_tokentextbox_TokenItemAdding(
        CommunityToolkit.WinUI.Controls.TokenizingTextBox sender,
        CommunityToolkit.WinUI.Controls.TokenItemAddingEventArgs args
    )
    {
        args.Item = new CharacterInfo() { Name = args.TokenText, GameFolderPath = ViewModel.FolderPath };
    }
}
