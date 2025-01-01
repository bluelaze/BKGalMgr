using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace BKGalMgr.ViewModels;

public partial class NotificationInfo : ObservableObject
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _message;

    [ObservableProperty]
    private bool _isOpen = true;

    [ObservableProperty]
    private InfoBarSeverity _Severity = InfoBarSeverity.Informational;
}
