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
    private string _name;

    [ObservableProperty]
    private bool _isChecked;
}
