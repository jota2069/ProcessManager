using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ProcessManager.Models;

namespace ProcessManager.Services;

public class ProcessService
{
    public List<ProcessInfo> GetAllProcesses()
    {
        List<ProcessInfo> processList = new List<ProcessInfo>();
        Process[] processes = Process.GetProcesses();

        foreach (Process process in processes)
        {
            try
            {
                ProcessInfo info = new ProcessInfo
                {
                    Id = process.Id,
                    Name = process.ProcessName,
                    Priority = process.PriorityClass,
                    MemoryUsage = process.WorkingSet64,
                    CpuTime = process.TotalProcessorTime,
                    ThreadCount = process.Threads.Count
                };
                processList.Add(info);
            }
            catch (Exception)
            {
                // Пропускаем недоступные процессы
            }
        }

        return processList;
    }

    public bool SetProcessPriority(int processId, ProcessPriorityClass priority)
    {
        try
        {
            Process process = Process.GetProcessById(processId);
            process.PriorityClass = priority;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка изменения приоритета: {ex.Message}");
            return false;
        }
    }

    public int GetProcessorCount()
    {
        return Environment.ProcessorCount;
    }

    public long GetProcessorAffinity(int processId)
    {
        try
        {
            Process process = Process.GetProcessById(processId);
            return process.ProcessorAffinity.ToInt64();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка получения affinity: {ex.Message}");
            return -1L;
        }
    }

    public bool SetProcessorAffinity(int processId, long affinityMask)
    {
        try
        {
            Process process = Process.GetProcessById(processId);
            process.ProcessorAffinity = new IntPtr(affinityMask);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка установки affinity: {ex.Message}");
            return false;
        }
    }

    public List<ThreadInfo> GetProcessThreads(int processId)
    {
        List<ThreadInfo> threadList = new List<ThreadInfo>();

        try
        {
            Process process = Process.GetProcessById(processId);

            foreach (ProcessThread thread in process.Threads)
            {
                try
                {
                    ThreadInfo info = new ThreadInfo
                    {
                        Id = thread.Id,
                        Priority = thread.PriorityLevel,
                        State = thread.ThreadState,
                        CpuTime = thread.TotalProcessorTime
                    };
                    threadList.Add(info);
                }
                catch
                {
                    // Пропускаем недоступные потоки
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка получения потоков: {ex.Message}");
        }

        return threadList;
    }

    public int? GetParentProcessId(int processId)
    {
        if (processId <= 0)
        {
            return null;
        }

        if (OperatingSystem.IsLinux())
        {
            try
            {
                string statusPath = $"/proc/{processId}/status";
                if (!File.Exists(statusPath))
                {
                    return null;
                }

                string[] lines = File.ReadAllLines(statusPath);
                string? ppidLine = null;

                foreach (string line in lines)
                {
                    if (line.StartsWith("PPid:"))
                    {
                        ppidLine = line;
                        break;
                    }
                }

                if (ppidLine != null)
                {
                    string valuePart = ppidLine.Substring(5).Trim();
                    if (int.TryParse(valuePart, out int ppid) && ppid > 0)
                    {
                        return ppid;
                    }
                }
            }
            catch
            {
                // ошибки чтения /proc игнорируем
            }

            return null;
        }

        return null;
    }
}