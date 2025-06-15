using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using WinDash2.Core;
using WinDash2.Models;

namespace WinDash2.Views;

public sealed partial class ManagerWindow : Window
{
    private readonly ObservableCollection<Widget> _allWidgets = new();

    public ObservableCollection<Widget> FilteredWidgets { get; } = new();

    public ManagerWindow(WidgetManager widgetManager)
    {
        this.InitializeComponent();

        // Load all widgets into _allWidgets
        foreach (var widget in widgetManager.GetWidgets())
        {
            _allWidgets.Add(new Widget
            {
                Id = widget.Id,
                Name = widget.Name,
                Enabled = widget.Enabled
            });
        }

        // Initially, copy all to filtered view
        ApplyFilter("");
    }

    private void ApplyFilter(string query)
    {
        var lower = query.ToLowerInvariant();

        // Clear and repopulate FilteredWidgets
        FilteredWidgets.Clear();

        foreach (var widget in _allWidgets.Where(w => w.Name.ToLowerInvariant().Contains(lower)))
        {
            FilteredWidgets.Add(widget);
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = (sender as TextBox)?.Text ?? "";
        ApplyFilter(query);
    }

    private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        // Optional: Update underlying _allWidgets too, if needed
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
    }
}
