using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace BKGalMgr;

public static class Program
{
    private static Mutex _singleInstanceMutex;
    internal static EventWaitHandle _mainWindowWakeUpHandle;

    // Replaces the standard App.g.i.cs.
    // Note: We can't declare Main to be async because in a WinUI app
    // this prevents Narrator from reading XAML elements.
    [STAThread]
    static void Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();

        if (ExistLaunchedApp())
            return;

        Microsoft.UI.Xaml.Application.Start(
            (p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                new App();
            }
        );
    }

    private static bool ExistLaunchedApp()
    {
        // https://stackoverflow.com/questions/14506406/wpf-single-instance-best-practices
        string singleName = Directory.GetCurrentDirectory().MD5();
        _mainWindowWakeUpHandle = new EventWaitHandle(false, EventResetMode.AutoReset, singleName.Substring(24));
        _singleInstanceMutex = new Mutex(true, singleName, out bool isOwned);

        if (isOwned)
        {
            GC.KeepAlive(_singleInstanceMutex);
            return false;
        }

        _mainWindowWakeUpHandle.Set();
        return true;
    }
}
