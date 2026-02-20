using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            return -1;
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
                    threadList.Add(new ThreadInfo
                    {
                        Id = thread.Id,
                        Priority = thread.PriorityLevel,
                        State = thread.ThreadState,
                        CpuTime = thread.TotalProcessorTime
                    });
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
    
    public int GetParentProcessId(int processId)
    {
        try
        {
            Process process = Process.GetProcessById(processId);
        
            // Используем WMI для получения родительского процесса (работает и на Linux через /proc)
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "ps",
                Arguments = $"-o ppid= -p {processId}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        
            using (Process proc = Process.Start(startInfo))
            {
                if (proc != null)
                {
                    string output = proc.StandardOutput.ReadToEnd().Trim();
                    if (int.TryParse(output, out int parentId))
                    {
                        return parentId;
                    }
                }
            }
        }
        catch
        {
        }
    
        return 0;
    }
    
}