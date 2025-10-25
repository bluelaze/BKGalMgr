using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LiveChartsCore.Kernel;

namespace BKGalMgr.ViewModels;

public partial class PlayedPeriodInfo : ObservableObject, IChartEntity
{
    // 。。。yet, begin is correct, but data is save
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Period))]
    [property: JsonPropertyName("benginTime")]
    private DateTime _benginTime;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Period))]
    [property: JsonPropertyName("endTime")]
    private DateTime _endTime;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Period))]
    private TimeSpan _pauseTime = TimeSpan.Zero;

    [JsonIgnore]
    public TimeSpan Period => EndTime - BenginTime - PauseTime;

    [JsonIgnore]
    public ChartEntityMetaData MetaData { get; set; }

    /// <summary>
    /// 这个目前好像没啥用处
    /// </summary>
    [JsonIgnore]
    public Coordinate Coordinate { get; set; } = Coordinate.Empty;

    public PlayedPeriodInfo() { }

    public PlayedPeriodInfo(DateTime beginTime, DateTime endTime, TimeSpan pauseTime)
    {
        BenginTime = beginTime;
        EndTime = endTime;
        PauseTime = pauseTime;

        Coordinate = new Coordinate(BenginTime.Ticks, Period.Ticks);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        Coordinate = new Coordinate(BenginTime.Ticks, Period.Ticks);

        base.OnPropertyChanged(e);
    }
}
