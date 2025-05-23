﻿using System;
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
    private GameInfo _game;

    public readonly SettingsDto Settings;
    public LibraryAndManagePageViewModel LibraryAndManagePageViewModel { get; }

    public GamePlayPageViewModel(SettingsDto settings, LibraryAndManagePageViewModel libraryAndManagePageViewModel)
    {
        Settings = settings;
        LibraryAndManagePageViewModel = libraryAndManagePageViewModel;
    }
}
