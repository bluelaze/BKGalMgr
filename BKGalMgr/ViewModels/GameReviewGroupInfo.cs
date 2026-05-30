using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.ViewModels;

public class GameReviewGroupItem
{
    public TimeSpan PlayedTime { get; set; }
    public GameInfo Game { get; set; }
}

public partial class GameReviewGroupInfo : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial DateTime Label { get; set; }

    [ObservableProperty]
    public partial TimeSpan PlayedTime { get; set; } = TimeSpan.Zero;

    [ObservableProperty]
    public partial ObservableCollection<GameReviewGroupItem> Games { get; set; }
}
