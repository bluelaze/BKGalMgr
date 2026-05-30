using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BKGalMgr.Models;

namespace BKGalMgr.ViewModels.Pages;

public partial class GamePlayPageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial GameInfo Game { get; set; }

    public readonly SettingsDto Settings;
    public LibraryAndManagePageViewModel LibraryAndManagePageViewModel { get; }

    public GamePlayPageViewModel(SettingsDto settings, LibraryAndManagePageViewModel libraryAndManagePageViewModel)
    {
        Settings = settings;
        LibraryAndManagePageViewModel = libraryAndManagePageViewModel;
    }
}
