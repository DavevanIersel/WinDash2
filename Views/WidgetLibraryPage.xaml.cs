using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using WinDash2.Core;
using WinDash2.Models;

namespace WinDash2.Views;

public sealed partial class WidgetLibraryPage : Page
{
    private WidgetManager? _widgetManager;
    private readonly ObservableCollection<Widget> _allWidgets = [];
    public ObservableCollection<Widget> FilteredWidgets { get; } = [];
    private bool _isInitialized;

    private readonly HashSet<Widget> _suppressToggles = [];

    public WidgetLibraryPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _isInitialized = false;
        base.OnNavigatedTo(e);

        _widgetManager = (WidgetManager)e.Parameter;

        foreach (var widget in _widgetManager.GetWidgets())
        {
            _allWidgets.Add(widget);
        }

        ApplyFilter("");
        DispatcherQueue.TryEnqueue(() => _isInitialized = true);
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

    private void ToggleSwitch_Loading(FrameworkElement sender, object args)
    {
        if (sender is ToggleSwitch toggleSwitch && toggleSwitch.DataContext is Widget widget && widget.Enabled)
        {
            _suppressToggles.Add(widget);
            toggleSwitch.IsOn = widget.Enabled;
        }
    }


    private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (!_isInitialized || _widgetManager == null) return;

        if (sender is ToggleSwitch toggleSwitch && toggleSwitch.DataContext is Widget widget)
        {
            if (_suppressToggles.Remove(widget)) return;

            toggleSwitch.IsEnabled = false;
            widget.Enabled = toggleSwitch.IsOn;
            _widgetManager.SaveWidget(widget, true);
            toggleSwitch.IsEnabled = true;
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (_widgetManager == null) return;

        if (sender is Button button && button.DataContext is Widget widget)
        {
            var args = new WidgetEditArgs
            {
                Widget = widget,
                WidgetManager = _widgetManager
            };

            Frame.Navigate(typeof(WidgetEditPage), args);
        }
    }
}
