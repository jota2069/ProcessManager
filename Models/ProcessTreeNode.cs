using System.Collections.ObjectModel;

namespace ProcessManager.Models;

public class ProcessTreeNode
{
    public ProcessInfo Process { get; set; }
    public ObservableCollection<ProcessTreeNode> Children { get; set; }

    public string DisplayText => $"{Process.Name} (PID: {Process.Id})";

    public ProcessTreeNode(ProcessInfo process)
    {
        Process = process;
        Children = new ObservableCollection<ProcessTreeNode>();
    }
}