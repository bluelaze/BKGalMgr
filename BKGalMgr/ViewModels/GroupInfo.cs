using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels;

public enum GroupChangedType
{
    Add,
    Remove,
    Edit,
}

public partial class GroupInfo : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial bool IsChecked { get; set; }
}
