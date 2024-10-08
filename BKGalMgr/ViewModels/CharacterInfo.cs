﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels;

public partial class CharacterInfo : ObservableObject
{
    [ObservableProperty]
    private string _avatar;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private string _bangumiCharacterId;
}
