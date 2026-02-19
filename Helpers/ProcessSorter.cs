using ProcessManager.Models;
using System.Collections.Generic;
using System.Linq;

namespace ProcessManager.Helpers;

public class ProcessSorter
{
    private string _currentSort = "none";
    private bool _isDescending = true;

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
            "pid" => _isDescending 
                ? processes.OrderByDescending(p => p.Id) 
                : processes.OrderBy(p => p.Id),
            "name" => _isDescending 
                ? processes.OrderByDescending(p => p.Name) 
                : processes.OrderBy(p => p.Name),
            "memory" => _isDescending 
                ? processes.OrderByDescending(p => p.MemoryUsage) 
                : processes.OrderBy(p => p.MemoryUsage),
            "priority" => _isDescending 
                ? processes.OrderByDescending(p => p.Priority) 
                : processes.OrderBy(p => p.Priority),
            "threads" => _isDescending 
                ? processes.OrderByDescending(p => p.ThreadCount) 
                : processes.OrderBy(p => p.ThreadCount),
            _ => processes
        };
    }
}