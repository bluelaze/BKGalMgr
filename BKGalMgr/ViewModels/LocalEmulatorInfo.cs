using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels;

public partial class LocalEmulatorInfo : ObservableObject
{
    [ObservableProperty]
    private string _LEProcPath;

    [ObservableProperty]
    [property: JsonIgnore]
    private List<LEProfileInfo> _profiles;
}
