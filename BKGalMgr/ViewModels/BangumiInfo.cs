using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels;

public partial class BangumiInfo : ObservableObject
{
    [ObservableProperty]
    private string _accessToken;
}
