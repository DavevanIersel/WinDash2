using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using WinDash2.Core;
using WinDash2.Models;
using WinDash2.Services;
using WinDash2.Utils;

namespace WinDash2.Views;

public sealed partial class WidgetLibraryPage : Page
{
    private readonly WidgetManager _widgetManager;
    private readonly WidgetFileSystemService _widgetFileSystemService;
    private readonly ObservableCollection<Widget> _allWidgets = [];
    public ObservableCollection<Widget> FilteredWidgets { get; } = [];

    public WidgetLibraryPage()
    {
        InitializeComponent();

        ArgumentNullException.ThrowIfNull(App.AppHost);
        _widgetManager = App.AppHost.Services.GetRequiredService<WidgetManager>();
        _widgetFileSystemService = App.AppHost.Services.GetRequiredService<WidgetFileSystemService>();

        InitializeWidgets();
    }

    private void InitializeWidgets()
    {
        _allWidgets.Clear();
        foreach (var widget in _widgetManager.GetWidgets())
        {
            _allWidgets.Add(widget);
        }

        ApplyFilter("");
    }

    private void ApplyFilter(string query)
    {
        var lower = query.ToLowerInvariant();
        FilteredWidgets.Clear();
        foreach (var widget in _allWidgets
            .Where(w => w.Name.ToLowerInvariant().Contains(lower))
            .OrderBy(w => w.Name, StringComparer.OrdinalIgnoreCase))
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
        if (sender is not ToggleSwitch toggleSwitch || toggleSwitch.DataContext is not Widget widget)
        {
            return;
        }

        // Only save if the toggle state actually differs from the widget state
        // This prevents saving when we're just syncing UI to match the widget
        if (widget.Enabled == toggleSwitch.IsOn)
        {
            return;
        }

        toggleSwitch.IsEnabled = false;
        widget.Enabled = toggleSwitch.IsOn;
        _widgetManager.SaveWidget(widget, true);
        toggleSwitch.IsEnabled = true;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not Widget widget)
        {
            return;
        }

        var args = new WidgetEditArgs
        {
            Widget = widget,
            WidgetManager = _widgetManager
        };

        Frame.Navigate(typeof(WidgetEditPage), args);
    }

    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        var newWidget = new Widget
        {
            Id = Guid.NewGuid(),
            Name = "New Widget",
            Url = "",
            Width = 600,
            Height = 400,
            Enabled = false,
            TouchEnabled = false
        };

        var args = new WidgetEditArgs
        {
            Widget = newWidget,
            WidgetManager = _widgetManager,
            IsNewWidget = true
        };

        Frame.Navigate(typeof(WidgetEditPage), args);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not Widget widget)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Delete Widget",
            Content = $"Are you sure you want to permanently delete '{widget.Name}'? This cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            _widgetManager.DeleteWidget(widget);
            _allWidgets.Remove(widget);
            FilteredWidgets.Remove(widget);
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SettingsPage));
    }

    private async void FaviconImage_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Image image || image.DataContext is not Widget widget)
        {
            return;
        }

        try
        {
            image.Source = await FaviconUtil.LoadFaviconAsync(widget, _widgetFileSystemService.WidgetsFolderPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load favicon for widget '{widget.Name}': {ex.Message}");
        }
    }
}