using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinDash2.Models;
using WinDash2.Services;
using WinDash2.Views;

namespace WinDash2.Core;

public class WidgetManager
{
    private readonly WidgetFileSystemService _widgetFileSystemService;

    private readonly Dictionary<Guid, Widget> _widgetConfigs = [];
    private readonly Dictionary<Guid, WidgetWindow> _widgetWindows = [];

    public WidgetManager(WidgetFileSystemService widgetFileSystemService)
    {
        _widgetFileSystemService = widgetFileSystemService ?? throw new ArgumentNullException(nameof(widgetFileSystemService));
    }

    public async Task InitializeAsync()
    {
        var widgets = await _widgetFileSystemService.LoadAllWidgetsAsync();

        _widgetConfigs.Clear();
        foreach (var widget in widgets)
        {
            _widgetConfigs[widget.IdOrThrow] = widget;
            CreateOrUpdateWidgetWindow(widget);
        }
    }

    private void CreateOrUpdateWidgetWindow(Widget widget)
    {
        if (_widgetWindows.TryGetValue(widget.IdOrThrow, out var existingWindow))
        {
            //existingWindow.UpdateWidget(widget);
            existingWindow.Activate();
        }
        else
        {
            var widgetWindow = new WidgetWindow(widget);
            widgetWindow.Activate();

            _widgetWindows[widget.IdOrThrow] = widgetWindow;
        }
    }

    public async Task SaveWidgetAsync(Widget widget)
    {
        await _widgetFileSystemService.SaveWidgetAsync(widget);
        _widgetConfigs[widget.IdOrThrow] = widget;
        CreateOrUpdateWidgetWindow(widget);
    }

    public IReadOnlyCollection<Widget> GetWidgets() => _widgetConfigs.Values.ToList().AsReadOnly();

    public void CloseAllWidgets()
    {
        foreach (var window in _widgetWindows.Values)
        {
            window.Close();
        }
        _widgetWindows.Clear();
    }
}
