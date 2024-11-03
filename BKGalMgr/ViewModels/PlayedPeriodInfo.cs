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
    [property: JsonPropertyName("benginTime")]
    private DateTime _benginTime;

    [ObservableProperty]
    [property: JsonPropertyName("endTime")]
    private DateTime _endTime;

    [JsonIgnore]
    public TimeSpan Period => EndTime - BenginTime;

    [JsonIgnore]
    public ChartEntityMetaData MetaData { get; set; }

    [JsonIgnore]
    public Coordinate Coordinate { get; set; } = Coordinate.Empty;

    public PlayedPeriodInfo() { }

    public PlayedPeriodInfo(DateTime beginTime, DateTime endTime)
    {
        BenginTime = beginTime;
        EndTime = endTime;

        Coordinate = new Coordinate(BenginTime.Ticks, (EndTime - BenginTime).Ticks);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        Coordinate = new Coordinate(BenginTime.Ticks, (EndTime - BenginTime).Ticks);

        base.OnPropertyChanged(e);
    }
}
