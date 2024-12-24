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
    private string _name;

    [ObservableProperty]
    private DateTime _label;

    [ObservableProperty]
    private TimeSpan _playedTime = TimeSpan.Zero;

    [ObservableProperty]
    private ObservableCollection<GameReviewGroupItem> _games;
}
