using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ProcessManager.Models;

namespace ProcessManager.Helpers;

public class ProcessRowBuilder
{
    public static Border CreateProcessRow(ProcessInfo process)
    {
        Grid grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

        grid.Children.Add(CreateCell(process.Id.ToString(), Color.FromRgb(86, 156, 214), 0));
        grid.Children.Add(CreateCell(process.Name, Color.FromRgb(255, 255, 255), 1, new Thickness(10, 0, 0, 0)));
        grid.Children.Add(CreateCell(process.MemoryUsageMb, Color.FromRgb(206, 145, 120), 2));
        grid.Children.Add(CreateCell(process.Priority.ToString(), Color.FromRgb(78, 201, 176), 3));
        grid.Children.Add(CreateCell(process.ThreadCount.ToString(), Color.FromRgb(220, 220, 170), 4));

        return new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(37, 37, 38)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(62, 62, 66)),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 8),
            Child = grid
        };
    }

    private static TextBlock CreateCell(string text, Color color, int column, Thickness? margin = null)
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
}