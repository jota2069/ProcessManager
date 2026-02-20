using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProcessManager.Models;
using ProcessManager.Services;

namespace ProcessManager.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ProcessService _processService;
    
    public ObservableCollection<ProcessInfo> Processes { get; set; }

    public MainWindowViewModel()
    {
        _processService = new ProcessService();
        Processes = new ObservableCollection<ProcessInfo>();
        LoadProcesses();
    }

    public void LoadProcesses()
    {
        Console.WriteLine("LoadProcesses вызван!");
        Processes.Clear();
        List<ProcessInfo> processList = _processService.GetAllProcesses();
        Console.WriteLine($"Получено процессов: {processList.Count}");

        foreach (ProcessInfo process in processList)
        {
            Processes.Add(process);
        }
    
        Console.WriteLine($"В коллекции процессов: {Processes.Count}");
    }
}