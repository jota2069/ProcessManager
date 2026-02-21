using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System;

namespace ProcessManager.Models;

public partial class ProcessInfo : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private ProcessPriorityClass priority;

    [ObservableProperty]
    private long memoryUsage;

    [ObservableProperty]
    private int threadCount;

    [ObservableProperty]
    private TimeSpan cpuTime;

    public string MemoryUsageMb
    {
        get
        {
            long mb = MemoryUsage / 1024 / 1024;
            return $"{mb} MB";
        }
    }

    public override string ToString()
    {
        return $"PID: {Id} | {Name} | {MemoryUsageMb} | {Priority} | Потоки: {ThreadCount}";
    }
}