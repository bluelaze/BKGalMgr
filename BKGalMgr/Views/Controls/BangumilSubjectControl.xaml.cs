using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using BKGalMgr.Models;
using BKGalMgr.Models.Bangumi;
using BKGalMgr.Services;
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

public sealed partial class BangumiSubjectControl : UserControl
{
    public bool HideSearch
    {
        get { return (bool)GetValue(HideSearchProperty); }
        set { SetValue(HideSearchProperty, value); }
    }
    public static readonly DependencyProperty HideSearchProperty = DependencyProperty.Register(
        "HideSearch",
        typeof(bool),
        typeof(BangumiSubjectControl),
        new PropertyMetadata(false)
    );

    public string AccessToken
    {
        get { return (string)GetValue(AccessTokenProperty); }
        set { SetValue(AccessTokenProperty, value); }
    }
    public static readonly DependencyProperty AccessTokenProperty = DependencyProperty.Register(
        "AccessToken",
        typeof(string),
        typeof(BangumiSubjectControl),
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
        typeof(BangumiSubjectControl),
        new PropertyMetadata(default(string))
    );

    public class AddSubjectClickEventArgs : RoutedEventArgs
    {
        public string SubjectId { get; }

        public AddSubjectClickEventArgs(string id)
        {
            SubjectId = id;
        }
    }

    public event TypedEventHandler<BangumiSubjectControl, AddSubjectClickEventArgs> AddSubjectClick;

    public BangumiSubjectControl()
    {
        this.InitializeComponent();
    }

    private static float ToRatingValue(float rating)
    {
        return rating / 2;
    }

    private async void search_game_AutoSuggestBox_QuerySubmitted(
        AutoSuggestBox sender,
        AutoSuggestBoxQuerySubmittedEventArgs args
    )
    {
        if (args.QueryText.IsNullOrEmpty())
            return;

        loading_ContentPresenter.Visibility = Visibility.Visible;

        App.GetRequiredService<SettingsDto>().Bangumi.AccessToken = AccessToken;
        var searchResult = await App.GetRequiredService<BangumiService>().SearchSubjectsAsync(args.QueryText);

        loading_ContentPresenter.Visibility = Visibility.Collapsed;

        if (!searchResult.IsSuccessful)
        {
            App.ShowErrorMessage(searchResult.ErrorMessage);
            return;
        }

        search_result_ListView.ItemsSource = searchResult.Data.data;
    }

    private void add_width_subject_url_Button_Click(object sender, RoutedEventArgs e)
    {
        AddSubjectClick?.Invoke(this, new(SubjectUrl));
    }

    private void add_subject_Button_Click(object sender, RoutedEventArgs e)
    {
        var subject = (sender as Button).DataContext as Subject;
        AddSubjectClick?.Invoke(this, new(subject.id.ToString()));
    }
}
