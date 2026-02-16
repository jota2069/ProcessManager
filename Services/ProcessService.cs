using System.Diagnostics;
using System;
using System.Collections.Generic;
using ProcessManager.Models;

namespace ProcessManager.Services;

public class ProcessService
{
    public List<ProcessInfo> GetAllProcesses9()
    {
        List<ProcessInfo> processList = new List<ProcessInfo>();
        Process[] processes = Process.GetProcesses();
        foreach (Process process in processes)
        {
            try
            {
                ProcessInfo info = new ProcessInfo
                {
                    Name = process.ProcessName,
                    Id = process.Id,
                    Priority = process.PriorityClass,
                    MemoryUsage = process.WorkingSet64,
                    CpuTime = process.TotalProcessorTime,
                    ThreadCount = process.Threads.Count
                };
                processList.Add(info);
            }
            catch (Exception)
            {
                
            }
        }
        return processList;
    } 
}