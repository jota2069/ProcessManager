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
            "gui" => FilterOnlyWithGui(filtered),
            "system" => FilterOnlySystem(filtered),
            _ => filtered
        };
        
        return filtered;
    }

    public IEnumerable<ProcessInfo> FilterByName(IEnumerable<ProcessInfo> processes, string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return processes;
        }

        string lowerSearch = searchText.ToLower();
        return processes.Where(p => p.Name.ToLower().Contains(lowerSearch));
    }

    public IEnumerable<ProcessInfo> FilterOnlyWithGui(IEnumerable<ProcessInfo> processes)
    {
        return processes.Where(p => 
            p.Id > 1000 && // Пользовательские процессы
            !p.Name.ToLower().Contains("systemd") &&
            !p.Name.ToLower().Contains("kworker") &&
            !p.Name.ToLower().Contains("daemon") &&
            !p.Name.ToLower().Contains("dbus") &&
            !p.Name.ToLower().Contains("polkit") &&
            !p.Name.ToLower().Contains("gvfs") &&
            !p.Name.ToLower().StartsWith("sd-"));
    }

    public IEnumerable<ProcessInfo> FilterOnlySystem(IEnumerable<ProcessInfo> processes)
    {
        return processes.Where(p => 
            p.Name.ToLower().Contains("system") || 
            p.Name.ToLower().Contains("svchost") ||
            p.Name.ToLower().Contains("service") ||
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
        return _currentSort switch
        {
            "pid" => _isDescending ? processes.OrderByDescending(p => p.Id) : processes.OrderBy(p => p.Id),
            "name" => _isDescending ? processes.OrderByDescending(p => p.Name) : processes.OrderBy(p => p.Name),
            "memory" => _isDescending ? processes.OrderByDescending(p => p.MemoryUsage) : processes.OrderBy(p => p.MemoryUsage),
            "priority" => _isDescending ? processes.OrderByDescending(p => p.Priority) : processes.OrderBy(p => p.Priority),
            "threads" => _isDescending ? processes.OrderByDescending(p => p.ThreadCount) : processes.OrderBy(p => p.ThreadCount),
            "cputime" => _isDescending ? processes.OrderByDescending(p => p.CpuTime) : processes.OrderBy(p => p.CpuTime),
            _ => processes
        };
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

        grid.Children.Add(CreateCell(process.Id.ToString(), Color.FromRgb(86, 156, 214), 0));
        grid.Children.Add(CreateCell(process.Name, Color.FromRgb(255, 255, 255), 1, new Thickness(10, 0, 0, 0)));
        grid.Children.Add(CreateCell(process.MemoryUsageMb, Color.FromRgb(206, 145, 120), 2));
        grid.Children.Add(CreateCell(process.Priority.ToString(), Color.FromRgb(78, 201, 176), 3));
        grid.Children.Add(CreateCell(process.ThreadCount.ToString(), Color.FromRgb(220, 220, 170), 4));
        grid.Children.Add(CreateCell($"{process.CpuTime.TotalSeconds:F1}s", Color.FromRgb(156, 220, 254), 5));

        Border border = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(37, 37, 38)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(62, 62, 66)),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 8),
            Child = grid,
            Cursor = new Cursor(StandardCursorType.Hand)
        };

        border.PointerPressed += (s, e) => onClickAction(process);
        border.PointerEntered += (s, e) => border.Background = new SolidColorBrush(Color.FromRgb(51, 51, 55));
        border.PointerExited += (s, e) => border.Background = new SolidColorBrush(Color.FromRgb(37, 37, 38));

        return border;
    }

    private TextBlock CreateCell(string text, Color color, int column, Thickness? margin = null)
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
        long affinityMask = 0;
        
        foreach (Control control in coreCheckboxes.Children)
        {
            if (control is CheckBox checkbox && checkbox.IsChecked == true)
            {
                int coreIndex = int.Parse(checkbox.Tag?.ToString() ?? "0");
                affinityMask = SetCore(affinityMask, coreIndex, true);
            }
        }

        if (affinityMask == 0)
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

    public void UpdateAffinityUI(ProcessInfo process, TextBlock infoTextBlock, StackPanel checkboxPanel)
    {
        long affinityMask = _processService.GetProcessorAffinity(process.Id);
        
        if (affinityMask == -1)
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
        StringBuilder sb = new StringBuilder();
        for (int i = coreCount - 1; i >= 0; i--)
        {
            sb.Append((mask & (1L << i)) != 0 ? "1" : "0");
        }
        return sb.ToString();
    }

    private string ToHex(long mask) => $"0x{mask:X}";
    
    private bool IsCoreEnabled(long mask, int coreIndex) => (mask & (1L << coreIndex)) != 0;
    
    private long SetCore(long mask, int coreIndex, bool enabled)
    {
        return enabled ? mask | (1L << coreIndex) : mask & ~(1L << coreIndex);
    }

    // === ПОТОКИ ===
    
    public void UpdateThreadsUI(ProcessInfo process, TextBlock countTextBlock, StackPanel threadsList)
    {
        List<ThreadInfo> threads = _processService.GetProcessThreads(process.Id);
        
        countTextBlock.Text = $"Всего потоков: {threads.Count}";
        threadsList.Children.Clear();

        foreach (ThreadInfo thread in threads)
        {
            Border border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(62, 62, 66)),
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
    
        // Создаём узлы для всех процессов
        foreach (ProcessInfo process in processes)
        {
            allNodes[process.Id] = new ProcessTreeNode(process);
            int parentId = _processService.GetParentProcessId(process.Id);
            if (parentId > 0)
            {
                parentMap[process.Id] = parentId;
            }
        }
    
        // Строим дерево
        ObservableCollection<ProcessTreeNode> rootNodes = new ObservableCollection<ProcessTreeNode>();
    
        foreach (ProcessInfo process in processes)
        {
            ProcessTreeNode node = allNodes[process.Id];
        
            if (parentMap.ContainsKey(process.Id))
            {
                int parentId = parentMap[process.Id];
                if (allNodes.ContainsKey(parentId))
                {
                    allNodes[parentId].Children.Add(node);
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
            if (found != null)
            {
                return found;
            }
        }
    
        return null;
    }
    
}