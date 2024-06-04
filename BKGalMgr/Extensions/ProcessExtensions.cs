using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.Extensions;

public static class ProcessExtensions
{
    // https://stackoverflow.com/questions/17922725/monitor-child-processes-of-a-process
    public static IList<Process> GetChildProcesses(this Process process) =>
        new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={process.Id}")
            .Get()
            .Cast<ManagementObject>()
            .Select(mo => Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])))
            .ToList();
}
