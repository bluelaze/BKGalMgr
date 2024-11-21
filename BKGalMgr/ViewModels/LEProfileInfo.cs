using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels;

public partial class LEProfileInfo : ObservableObject
{
    public const string RunGuid = "run";
    public const string ManageGuid = "manage";
    public const string SeparatorGuid = "separator";

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _guid;

    [ObservableProperty]
    private bool _isSeparator = false;
}
