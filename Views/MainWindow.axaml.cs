using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ProcessManager.ViewModels;
using ProcessManager.Models;
using ProcessManager.Helpers;
using System;
using System.Collections.Generic;

namespace ProcessManager.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly ProcessSorter _sorter;
    private DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel();
        _sorter = new ProcessSorter();
        DisplayProcesses(_viewModel.Processes);
        
        StartAutoRefresh();
    }

    private void StartAutoRefresh()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _timer.Tick += (sender, args) =>
        {
            _viewModel.LoadProcesses();
            DisplayProcesses(_sorter.ApplySort(_viewModel.Processes));
        };
        _timer.Start();
    }

    private void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.LoadProcesses();
        DisplayProcesses(_sorter.ApplySort(_viewModel.Processes));
    }
    
    private void SortByPid_Click(object? sender, RoutedEventArgs e)
    {
        _sorter.ToggleSortDirection("pid");
        DisplayProcesses(_sorter.ApplySort(_viewModel.Processes));
    }

    private void SortByName_Click(object? sender, RoutedEventArgs e)
    {
        _sorter.ToggleSortDirection("name");
        DisplayProcesses(_sorter.ApplySort(_viewModel.Processes));
    }

    private void SortByMemory_Click(object? sender, RoutedEventArgs e)
    {
        _sorter.ToggleSortDirection("memory");
        DisplayProcesses(_sorter.ApplySort(_viewModel.Processes));
    }

    private void SortByPriority_Click(object? sender, RoutedEventArgs e)
    {
        _sorter.ToggleSortDirection("priority");
        DisplayProcesses(_sorter.ApplySort(_viewModel.Processes));
    }

    private void SortByThreads_Click(object? sender, RoutedEventArgs e)
    {
        _sorter.ToggleSortDirection("threads");
        DisplayProcesses(_sorter.ApplySort(_viewModel.Processes));
    }

    private void DisplayProcesses(IEnumerable<ProcessInfo> processes)
    {
        ProcessList.Children.Clear();

        foreach (ProcessInfo process in processes)
        {
            ProcessList.Children.Add(ProcessRowBuilder.CreateProcessRow(process));
        }

        ProcessCount.Text = $"Процессов: {_viewModel.Processes.Count}";
    }
}