using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ProcessManager.Models;
using ProcessManager.Services;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaCursor = Avalonia.Input.Cursor;
using System.Collections.ObjectModel;

namespace ProcessManager.Helpers;

public class ProcessHelper
{
    private readonly ProcessService _processService;
    private string _currentSort = "none";
    private bool _isDescending = true;
    private string _currentFilter = "all";

    public ProcessHelper()
    {
        _processService = new ProcessService();
    }

    // === ФИЛЬТРАЦИЯ ===

    public void SetFilter(string filterType)
    {
        _currentFilter = filterType;
    }

    public IEnumerable<ProcessInfo> ApplyFilters(IEnumerable<ProcessInfo> processes, string searchText)
    {
        IEnumerable<ProcessInfo> filtered = FilterByName(processes, searchText);

        filtered = _currentFilter switch
        {
            "gui"    => FilterOnlyWithGui(filtered),
            "system" => FilterOnlySystem(filtered),
            _        => filtered
        };

        return filtered;
    }

    private IEnumerable<ProcessInfo> FilterByName(IEnumerable<ProcessInfo> processes, string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return processes;
        }

        string lowerSearch = searchText.ToLowerInvariant();
        return processes.Where(p => p.Name.ToLowerInvariant().Contains(lowerSearch));
    }

    private IEnumerable<ProcessInfo> FilterOnlyWithGui(IEnumerable<ProcessInfo> processes)
    {
        return processes.Where(p =>
            p.Id > 1000 &&
            !p.Name.ToLowerInvariant().Contains("systemd") &&
            !p.Name.ToLowerInvariant().Contains("kworker") &&
            !p.Name.ToLowerInvariant().Contains("daemon") &&
            !p.Name.ToLowerInvariant().Contains("dbus") &&
            !p.Name.ToLowerInvariant().Contains("polkit") &&
            !p.Name.ToLowerInvariant().Contains("gvfs") &&
            !p.Name.ToLowerInvariant().StartsWith("sd-"));
    }

    private IEnumerable<ProcessInfo> FilterOnlySystem(IEnumerable<ProcessInfo> processes)
    {
        return processes.Where(p =>
            p.Name.ToLowerInvariant().Contains("system") ||
            p.Name.ToLowerInvariant().Contains("svchost") ||
            p.Name.ToLowerInvariant().Contains("service") ||
            p.Id < 100);
    }

    // === СОРТИРОВКА ===

    public void ToggleSortDirection(string sortType)
    {
        if (_currentSort == sortType)
        {
            _isDescending = !_isDescending;
        }
        else
        {
            _currentSort = sortType;
            _isDescending = true;
        }
    }

    public IEnumerable<ProcessInfo> ApplySort(IEnumerable<ProcessInfo> processes)
    {
        if (string.IsNullOrEmpty(_currentSort))
        {
            return processes;
        }

        IEnumerable<ProcessInfo> sorted = _currentSort switch
        {
            "pid"      => _isDescending ? processes.OrderByDescending<ProcessInfo, int>(p => p.Id)      : processes.OrderBy<ProcessInfo, int>(p => p.Id),
            "name"     => _isDescending ? processes.OrderByDescending<ProcessInfo, string>(p => p.Name)     : processes.OrderBy<ProcessInfo, string>(p => p.Name),
            "memory"   => _isDescending ? processes.OrderByDescending<ProcessInfo, long>(p => p.MemoryUsage) : processes.OrderBy<ProcessInfo, long>(p => p.MemoryUsage),
            "priority" => _isDescending ? processes.OrderByDescending<ProcessInfo, ProcessPriorityClass>(p => p.Priority) : processes.OrderBy<ProcessInfo, ProcessPriorityClass>(p => p.Priority),
            "threads"  => _isDescending ? processes.OrderByDescending<ProcessInfo, int>(p => p.ThreadCount) : processes.OrderBy<ProcessInfo, int>(p => p.ThreadCount),
            "cpu"      => _isDescending ? processes.OrderByDescending<ProcessInfo, TimeSpan>(p => p.CpuTime) : processes.OrderBy<ProcessInfo, TimeSpan>(p => p.CpuTime),
            _          => processes
        };

        return sorted;
    }

    // === СОЗДАНИЕ UI ===

    public Border CreateProcessRow(ProcessInfo process, Action<ProcessInfo> onClickAction)
    {
        Grid grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

        grid.Children.Add(CreateCell(process.Id.ToString(), AvaloniaColor.FromRgb(86, 156, 214), 0));
        grid.Children.Add(CreateCell(process.Name, AvaloniaColor.FromRgb(255, 255, 255), 1, new Thickness(10, 0, 0, 0)));
        grid.Children.Add(CreateCell(process.MemoryUsageMb, AvaloniaColor.FromRgb(206, 145, 120), 2));
        grid.Children.Add(CreateCell(process.Priority.ToString(), AvaloniaColor.FromRgb(78, 201, 176), 3));
        grid.Children.Add(CreateCell(process.ThreadCount.ToString(), AvaloniaColor.FromRgb(220, 220, 170), 4));
        grid.Children.Add(CreateCell($"{process.CpuTime.TotalSeconds:F1}s", AvaloniaColor.FromRgb(156, 220, 254), 5));

        AvaloniaColor backgroundColor = process.Priority switch
        {
            ProcessPriorityClass.RealTime   => AvaloniaColor.FromRgb(139, 0, 0),
            ProcessPriorityClass.High       => AvaloniaColor.FromRgb(139, 69, 0),
            ProcessPriorityClass.AboveNormal => AvaloniaColor.FromRgb(184, 134, 11),
            _                               => AvaloniaColor.FromRgb(37, 37, 38)
        };

        Border border = new Border
        {
            Background = new SolidColorBrush(backgroundColor),
            BorderBrush = new SolidColorBrush(AvaloniaColor.FromRgb(62, 62, 66)),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 8),
            Child = grid,
            Cursor = new AvaloniaCursor(StandardCursorType.Hand)
        };

        border.PointerPressed += (_, _) => onClickAction(process);
        border.PointerEntered += (_, _) => border.Background = new SolidColorBrush(AvaloniaColor.FromRgb(51, 51, 55));
        border.PointerExited += (_, _)  => border.Background = new SolidColorBrush(backgroundColor);

        return border;
    }

    private TextBlock CreateCell(string text, AvaloniaColor color, int column, Thickness? margin = null)
    {
        TextBlock textBlock = new TextBlock
        {
            Text = text,
            Foreground = new SolidColorBrush(color),
            Margin = margin ?? new Thickness(0)
        };

        Grid.SetColumn(textBlock, column);
        return textBlock;
    }

    // === CPU AFFINITY ===

    public bool ApplyAffinity(ProcessInfo process, StackPanel coreCheckboxes)
    {
        long affinityMask = 0L;

        foreach (Control control in coreCheckboxes.Children)
        {
            if (control is CheckBox checkbox && checkbox.IsChecked == true)
            {
                if (int.TryParse(checkbox.Tag?.ToString(), out int coreIndex))
                {
                    affinityMask = SetCore(affinityMask, coreIndex, true);
                }
            }
        }

        if (affinityMask == 0L)
        {
            Console.WriteLine("ОШИБКА: Нужно выбрать хотя бы одно ядро");
            return false;
        }

        bool success = _processService.SetProcessorAffinity(process.Id, affinityMask);
        if (success)
        {
            Console.WriteLine($"CPU Affinity изменён: {ToHex(affinityMask)}");
        }

        return success;
    }

    public void UpdateAffinityUi(ProcessInfo process, TextBlock infoTextBlock, StackPanel checkboxPanel)
    {
        long affinityMask = _processService.GetProcessorAffinity(process.Id);

        if (affinityMask == -1L)
        {
            infoTextBlock.Text = "Не удалось получить информацию";
            return;
        }

        int coreCount = _processService.GetProcessorCount();
        string binary = ToBinary(affinityMask, coreCount);
        string hex = ToHex(affinityMask);

        infoTextBlock.Text = $"Ядер: {coreCount}\nДвоичная: {binary}\nHex: {hex}";

        checkboxPanel.Children.Clear();

        for (int i = 0; i < coreCount; i++)
        {
            CheckBox checkbox = new CheckBox
            {
                Content = $"Ядро {i}",
                IsChecked = IsCoreEnabled(affinityMask, i),
                Tag = i,
                Foreground = Brushes.White
            };
            checkboxPanel.Children.Add(checkbox);
        }
    }

    private string ToBinary(long mask, int coreCount)
    {
        StringBuilder sb = new StringBuilder(coreCount);
        for (int i = coreCount - 1; i >= 0; i--)
        {
            sb.Append((mask & (1L << i)) != 0L ? "1" : "0");
        }
        return sb.ToString();
    }

    private string ToHex(long mask) => $"0x{mask:X}";

    private bool IsCoreEnabled(long mask, int coreIndex) => (mask & (1L << coreIndex)) != 0L;

    private long SetCore(long mask, int coreIndex, bool enabled)
    {
        return enabled ? mask | (1L << coreIndex) : mask & ~(1L << coreIndex);
    }

    // === ПОТОКИ ===

    public void UpdateThreadsUi(ProcessInfo process, TextBlock countTextBlock, StackPanel threadsList)
    {
        List<ThreadInfo> threads = _processService.GetProcessThreads(process.Id);

        countTextBlock.Text = $"Всего потоков: {threads.Count}";
        threadsList.Children.Clear();

        foreach (ThreadInfo thread in threads)
        {
            Border border = new Border
            {
                Background = new SolidColorBrush(AvaloniaColor.FromRgb(62, 62, 66)),
                Padding = new Thickness(8, 5),
                Margin = new Thickness(0, 2),
                CornerRadius = new CornerRadius(3)
            };

            TextBlock text = new TextBlock
            {
                Text = $"ID: {thread.Id} | {thread.State} | {thread.Priority} | CPU: {thread.CpuTimeString}",
                Foreground = Brushes.White,
                FontSize = 11,
                FontFamily = new FontFamily("Consolas")
            };

            border.Child = text;
            threadsList.Children.Add(border);
        }
    }

    // === ДЕРЕВО ПРОЦЕССОВ ===

    public ObservableCollection<ProcessTreeNode> BuildTree(List<ProcessInfo> processes)
    {
        Dictionary<int, ProcessTreeNode> allNodes = new Dictionary<int, ProcessTreeNode>();
        Dictionary<int, int> parentMap = new Dictionary<int, int>();

        foreach (ProcessInfo process in processes)
        {
            allNodes[process.Id] = new ProcessTreeNode(process);
            int? parentId = _processService.GetParentProcessId(process.Id);
            if (parentId.HasValue && parentId.Value > 0)
            {
                parentMap[process.Id] = parentId.Value;
            }
        }

        ObservableCollection<ProcessTreeNode> rootNodes = new ObservableCollection<ProcessTreeNode>();

        foreach (ProcessInfo process in processes)
        {
            ProcessTreeNode node = allNodes[process.Id];

            if (parentMap.TryGetValue(process.Id, out int parentId))
            {
                if (allNodes.TryGetValue(parentId, out ProcessTreeNode? parentNode))
                {
                    parentNode.Children.Add(node);
                }
                else
                {
                    rootNodes.Add(node);
                }
            }
            else
            {
                rootNodes.Add(node);
            }
        }

        return rootNodes;
    }

    public ProcessTreeNode? FindNodeById(ObservableCollection<ProcessTreeNode> nodes, int processId)
    {
        foreach (ProcessTreeNode node in nodes)
        {
            if (node.Process.Id == processId)
            {
                return node;
            }

            ProcessTreeNode? found = FindNodeById(node.Children, processId);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    // === ГРАФИКИ ===

    public ScottPlot.Avalonia.AvaPlot BuildCpuChart()
    {
        ScottPlot.Avalonia.AvaPlot cpuPlot = new ScottPlot.Avalonia.AvaPlot();
        int coreCount = Environment.ProcessorCount;
        double[] coreLoads = new double[coreCount];

        for (int i = 0; i < coreCount; i++)
        {
            coreLoads[i] = Random.Shared.NextDouble() * 100.0;
        }

        cpuPlot.Plot.Add.Bars(coreLoads);
        cpuPlot.Plot.YLabel("Загрузка %");
        cpuPlot.Plot.XLabel("Ядро");
        cpuPlot.Refresh();

        return cpuPlot;
    }

    public ScottPlot.Avalonia.AvaPlot BuildMemoryChart(List<ProcessInfo> processes)
    {
        ScottPlot.Avalonia.AvaPlot memoryPlot = new ScottPlot.Avalonia.AvaPlot();

        List<ProcessInfo> top10 = processes
            .OrderByDescending(p => p.MemoryUsage)
            .Take(10)
            .ToList();

        double[] memory = top10.Select(p => (double)p.MemoryUsage / (1024.0 * 1024.0)).ToArray();
        string[] names = top10.Select(p => p.Name.Length > 15 ? p.Name[..12] + "..." : p.Name).ToArray();

        memoryPlot.Plot.Add.Bars(memory);
        memoryPlot.Plot.YLabel("Память (MB)");
        memoryPlot.Plot.XLabel("Процесс");

        memoryPlot.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
            Enumerable.Range(0, names.Length).Select(i => (double)i).ToArray(),
            names
        );

        memoryPlot.Refresh();

        return memoryPlot;
    }
}