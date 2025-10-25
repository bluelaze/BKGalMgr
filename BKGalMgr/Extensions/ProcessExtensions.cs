using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BKGalMgr.Extensions;

public static class ProcessExtensions
{
    // https://stackoverflow.com/questions/17922725/monitor-child-processes-of-a-process
    public static IList<Process> GetChildProcesses(this Process process)
    {
        return new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessId={process.Id}")
            .Get()
            .Cast<ManagementObject>()
            .Select(mo => Process.GetProcessById(Convert.ToInt32(mo["ProcessId"])))
            .ToList();
    }

    // https://stackoverflow.com/questions/7189117/find-all-child-processes-of-my-own-net-process-find-out-if-a-given-process-is/67235225#67235225
    public static async Task WaitForAllExitAsync(this Process process, CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return;
        if (!process.HasExited)
            await process.WaitForExitAsync(token);

        if (token.IsCancellationRequested)
            return;
        foreach (var child in process.GetChildProcesses())
        {
            await WaitForAllExitAsync(child, token);
            if (token.IsCancellationRequested)
                return;
        }
    }

    public static bool IsMainWindowProcess(this Process process)
    {
        if (!process.MainWindowTitle.IsNullOrEmpty() && WindowExtension.IsWindowVisible(process.MainWindowHandle))
            return true;
        return false;
    }

    public static Process FindMainWindowProcess(this Process process)
    {
        if (process.IsMainWindowProcess())
            return process;

        foreach (var child in process.GetChildProcesses())
        {
            if (child.FindMainWindowProcess() is Process p)
                return p;
        }

        return null;
    }

    public static BitmapImage GetIcon(this Process process)
    {
        try
        {
            // 如果是管理员权限启动的程序MainModule会获取失败，例如geek
            var icon = Icon.ExtractAssociatedIcon(process.MainModule.FileName);
            return icon.ToBitmap().ToWinUIBitmapImage();
        }
        catch
        {
            return null;
        }
    }

    public static BitmapImage GetIcon(int pid)
    {
        return Process.GetProcessById(pid).GetIcon();
    }

    // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Diagnostics.Process/src/System/Diagnostics/ProcessManager.Win32.cs
    // https://github.com/Blinue/Magpie/blob/main/src/Magpie/NewProfileViewModel.cpp
    public static List<Process> GetAllWindowProcess()
    {
        List<Process> list = new List<Process>();
        WindowExtension.EnumWindows(
            (IntPtr wnd, IntPtr param) =>
            {
                if (
                    WindowExtension.IsWindowVisible(wnd)
                    && !WindowExtension.IsWindowCloaked(wnd)
                    && WindowExtension.GetWindowThreadProcessId(wnd, out int processId) != 0
                )
                {
                    try
                    {
                        list.Add(Process.GetProcessById(processId));
                    }
                    catch { }
                }
                return true;
            },
            IntPtr.Zero
        );
        return list.DistinctBy(p => p.Id)
            .Where(
                (p) =>
                {
                    return !p.MainWindowTitle.IsNullOrEmpty();
                }
            )
            .ToList();
    }

    public static Process GetProcessByWindow(IntPtr hWnd)
    {
        if (WindowExtension.GetWindowThreadProcessId(hWnd, out int processId) != 0)
        {
            return Process.GetProcessById(processId);
        }
        return null;
    }

    public static Process GetForegroundWindowProcess()
    {
        return GetProcessByWindow(WindowExtension.GetForegroundWindow());
    }
}
