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
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Guid { get; set; }

    [ObservableProperty]
    public partial bool IsSeparator { get; set; } = false;
}
