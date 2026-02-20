using System;
using System.Diagnostics;
using ProcessManager.Models;

namespace ProcessManager.Services;

public class ProcessController
{
    private readonly ProcessService _processService;

    public ProcessController()
    {
        _processService = new ProcessService();
    }

    public bool SetPriority(ProcessInfo process, ProcessPriorityClass priority)
    {
        if (priority == ProcessPriorityClass.RealTime)
        {
            Console.WriteLine("ПРЕДУПРЕЖДЕНИЕ: Приоритет Realtime может нарушить работу системы!");
        }

        bool success = _processService.SetProcessPriority(process.Id, priority);

        if (success)
        {
            Console.WriteLine($"Приоритет процесса {process.Name} изменён на {priority}");
        }
        else
        {
            Console.WriteLine($"ОШИБКА: Не удалось изменить приоритет процесса {process.Name}. Возможно недостаточно прав.");
        }

        return success;
    }

    public bool KillProcess(ProcessInfo process)
    {
        try
        {
            Process proc = Process.GetProcessById(process.Id);
            proc.Kill();
            Console.WriteLine($"Процесс {process.Name} (PID: {process.Id}) завершён");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ОШИБКА завершения процесса: {ex.Message}");
            return false;
        }
    }

    public ProcessPriorityClass ParsePriority(string priorityString)
    {
        return priorityString switch
        {
            "Idle" => ProcessPriorityClass.Idle,
            "BelowNormal" => ProcessPriorityClass.BelowNormal,
            "Normal" => ProcessPriorityClass.Normal,
            "AboveNormal" => ProcessPriorityClass.AboveNormal,
            "High" => ProcessPriorityClass.High,
            "Realtime" => ProcessPriorityClass.RealTime,
            _ => ProcessPriorityClass.Normal
        };
    }
}