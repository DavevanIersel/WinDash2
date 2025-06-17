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

    private readonly WidgetManager _widgetManager;

    private readonly bool _isInitialized;


    public ManagerWindow(WidgetManager widgetManager)
    {
        _widgetManager = widgetManager;
        this.InitializeComponent();

        foreach (var widget in widgetManager.GetWidgets())
        {
            _allWidgets.Add(widget);
        }

        ApplyFilter("");

        _isInitialized = true;
    }

    private void ApplyFilter(string query)
    {
        var lower = query.ToLowerInvariant();

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

    private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (!_isInitialized)
            return;

        if (sender is ToggleSwitch toggleSwitch && toggleSwitch.DataContext is Widget widget)
        {
            toggleSwitch.IsEnabled = false;
            widget.Enabled = toggleSwitch.IsOn;
            await _widgetManager.SaveWidgetAsync(widget, true);

            toggleSwitch.IsEnabled = true;
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
    }
}
