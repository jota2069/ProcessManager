using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ProcessManager.ViewModels;
using ProcessManager.Models;
using ProcessManager.Helpers;
using ProcessManager.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ProcessManager.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly ProcessController _processController;
    private readonly ProcessHelper _helper;
    private DispatcherTimer? _timer;
    private ProcessInfo? _selectedProcess;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;

        _processController = new ProcessController();
        _helper = new ProcessHelper();

        // Только после инициализации VM можно работать с её данными
        DisplayProcesses(_viewModel.Processes);
        StartAutoRefresh();
    }

    private void StartAutoRefresh()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _timer.Tick += (s, e) => RefreshProcessList();
        _timer.Start();
    }

    private void RefreshProcessList()
    {
        _viewModel.LoadProcesses();
        IEnumerable<ProcessInfo> filtered = _helper.ApplyFilters(_viewModel.Processes, SearchBox.Text ?? "");
        DisplayProcesses(_helper.ApplySort(filtered));
    }

    private void RefreshButton_Click(object? sender, RoutedEventArgs e) => RefreshProcessList();
    
    private void SortByPid_Click(object? sender, RoutedEventArgs e)
    {
        _helper.ToggleSortDirection("pid");
        RefreshProcessList();
    }

    private void SortByName_Click(object? sender, RoutedEventArgs e)
    {
        _helper.ToggleSortDirection("name");
        RefreshProcessList();
    }

    private void SortByMemory_Click(object? sender, RoutedEventArgs e)
    {
        _helper.ToggleSortDirection("memory");
        RefreshProcessList();
    }

    private void SortByPriority_Click(object? sender, RoutedEventArgs e)
    {
        _helper.ToggleSortDirection("priority");
        RefreshProcessList();
    }

    private void SortByThreads_Click(object? sender, RoutedEventArgs e)
    {
        _helper.ToggleSortDirection("threads");
        RefreshProcessList();
    }

    private void SortByCpuTime_Click(object? sender, RoutedEventArgs e)
    {
        _helper.ToggleSortDirection("cpu");
        RefreshProcessList();
    }

    private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e) => RefreshProcessList();

    private void ClearSearch_Click(object? sender, RoutedEventArgs e)
    {
        SearchBox.Text = "";
    }

    private void IntervalInput_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_timer == null) return;
    
        decimal? value = IntervalInput.Value;
        double seconds = value.HasValue ? (double)value.Value : 3.0;
    
        _timer.Stop();
        _timer.Interval = TimeSpan.FromSeconds(seconds);
        _timer.Start();
    
        Console.WriteLine($"Интервал обновления изменён на {seconds} сек");
    }

    private void FilterAll_Click(object? sender, RoutedEventArgs e)
    {
        _helper.SetFilter("all");
        RefreshProcessList();
    }

    private void FilterGui_Click(object? sender, RoutedEventArgs e)
    {
        _helper.SetFilter("gui");
        RefreshProcessList();
    }

    private void FilterSystem_Click(object? sender, RoutedEventArgs e)
    {
        _helper.SetFilter("system");
        RefreshProcessList();
    }

    private void PriorityComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
    }

    private void ApplyPriority_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedProcess == null) return;

        ComboBoxItem? selected = PriorityComboBox.SelectedItem as ComboBoxItem;
        if (selected == null) return;

        ProcessPriorityClass priority = _processController.ParsePriority(selected.Content?.ToString() ?? "Normal");
        
        if (_processController.SetPriority(_selectedProcess, priority))
        {
            RefreshProcessList();
        }
    }

    private void KillProcess_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedProcess == null) return;

        if (_processController.KillProcess(_selectedProcess))
        {
            _selectedProcess = null;
            SelectedProcessInfo.Text = "Не выбран";
            RefreshProcessList();
        }
    }

    private void ApplyCpuAffinity_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedProcess == null) return;
        _helper.ApplyAffinity(_selectedProcess, CoreCheckboxes);
    }

    private void DisplayProcesses(IEnumerable<ProcessInfo> processes)
    {
        ProcessList.Children.Clear();

        foreach (ProcessInfo process in processes)
        {
            ProcessList.Children.Add(_helper.CreateProcessRow(process, OnProcessSelected));
        }

        ProcessCount.Text = $"Процессов: {_viewModel.Processes.Count}";
    }

    private void OnProcessSelected(ProcessInfo process)
    {
        _selectedProcess = process;
        SelectedProcessInfo.Text = $"{process.Name} (PID: {process.Id})";
    
        _helper.UpdateAffinityUi(process, AffinityInfo, CoreCheckboxes);
        _helper.UpdateThreadsUi(process, ThreadsCount, ThreadsList);
    }
    
    private void BuildTree_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.LoadProcesses();
        ProcessTreeView.ItemsSource = _helper.BuildTree(_viewModel.Processes.ToList());
        TreeInfo.Text = $"Построено дерево из {_viewModel.Processes.Count} процессов";
    }

    private void ProcessTreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ProcessTreeView.SelectedItem is ProcessTreeNode node)
        {
            OnProcessSelected(node.Process);
        }
    }
    
    private void UpdateCharts_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.LoadProcesses();
        CpuChartContainer.Child = _helper.BuildCpuChart();
        MemoryChartContainer.Child = _helper.BuildMemoryChart(_viewModel.Processes.ToList());
    }
    
    
    
}