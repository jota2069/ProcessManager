using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ProcessManager.Models;
using ProcessManager.Services;
using ProcessManager.Views;

namespace ProcessManager.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ProcessService _processService;
    private ObservableCollection<ProcessInfo> _processes;
    private  ProcessInfo? _selectedProcess;

    public ObservableCollection<ProcessInfo> Processes
    {
        get => _processes;
        set => SetProperty(ref _processes, value);
    }

    public ProcessInfo? SelectedProcess
    {
        get => _selectedProcess;
        set => SetProperty(ref _selectedProcess, value);
    }

    public MainWindowViewModel()
    {
        _processService = new ProcessService();
        _processes = new ObservableCollection<ProcessInfo>();
        LoadProcesses();
    }

    public void LoadProcesses()
    {
        _processes.Clear();
        List<ProcessInfo> processList = _processService.GetAllProcesses();

        foreach (ProcessInfo process in processList)
        {
            _processes.Add(process);
        }
    }
}
