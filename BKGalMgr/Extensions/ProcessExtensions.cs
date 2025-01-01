using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BKGalMgr.Extensions;

public static class ProcessExtensions
{
    // https://stackoverflow.com/questions/17922725/monitor-child-processes-of-a-process
    public static IList<Process> GetChildProcesses(this Process process) =>
        new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessId={process.Id}")
            .Get()
            .Cast<ManagementObject>()
            .Select(mo => Process.GetProcessById(Convert.ToInt32(mo["ProcessId"])))
            .ToList();

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
}
