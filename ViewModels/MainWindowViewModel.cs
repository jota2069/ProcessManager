using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ProcessManager.Models;
using ProcessManager.Services;
using System;
using System.Collections.Generic;

namespace ProcessManager.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ProcessService processService;

    public ObservableCollection<ProcessInfo> Processes { get; } = new ObservableCollection<ProcessInfo>();

    [ObservableProperty]
    private ProcessInfo? selectedProcess;

    public MainWindowViewModel()
    {
        processService = new ProcessService();
        LoadProcesses();
    }

    public void LoadProcesses()
    {
        Console.WriteLine("LoadProcesses вызван!");
        Processes.Clear();

        List<ProcessInfo> processList = processService.GetAllProcesses();
        Console.WriteLine($"Получено процессов: {processList.Count}");

        foreach (ProcessInfo process in processList)
        {
            Processes.Add(process);
        }

        Console.WriteLine($"В коллекции процессов: {Processes.Count}");
    }
}