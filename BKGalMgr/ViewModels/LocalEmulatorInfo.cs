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
    public partial string LEProcPath { get; set; }

    [ObservableProperty]
    [JsonIgnore]
    public partial List<LEProfileInfo> Profiles { get; set; }
}
