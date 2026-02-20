using System;
using System.Diagnostics;

namespace ProcessManager.Models;

public class ProcessInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProcessPriorityClass Priority { get; set; }
    public long MemoryUsage { get; set; }
    public int ThreadCount { get; set; }
    public TimeSpan CpuTime { get; set; }

    public string MemoryUsageMb => $"{MemoryUsage / 1024 / 1024} MB";   
    
    public override string ToString()
    {
        return $"PID: {Id} | {Name} | {MemoryUsageMb} | {Priority} | Потоки: {ThreadCount}";
    }
}