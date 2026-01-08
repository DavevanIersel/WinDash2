using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WinDash2.Models;
using WinDash2.Services;
using WinDash2.Views;

namespace WinDash2.Core;

public class WidgetManager
{
    private readonly Dictionary<Guid, Widget> _widgetConfigs = [];
    private readonly Dictionary<Guid, WidgetWindow> _widgetWindows = [];
    private readonly WidgetFileSystemService _widgetFileSystemService;
    private readonly GridService _gridService;
    private Boolean _isDraggable = false;

    public WidgetManager(WidgetFileSystemService widgetFileSystemService, GridService gridService)
    {
        _widgetFileSystemService = widgetFileSystemService;
        _gridService = gridService;
    }

    public void Initialize()
    {
        var widgets = _widgetFileSystemService.LoadAllWidgets();

        _widgetConfigs.Clear();
        foreach (var widget in widgets)
        {
            _widgetConfigs[widget.IdOrThrow] = widget;
            CreateOrUpdateWidgetWindow(widget);
        }
    }

    //private void move_external_windows_experiment()
    //{
    //    [DllImport("user32.dll")]
    //    static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    //    [DllImport("user32.dll", SetLastError = true)]
    //    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    //    // For example, finding Steam
    //    var hSteam = FindWindow(null, "GitHub Desktop");
    //    if (hSteam != IntPtr.Zero)
    //    {
    //        MoveWindow(hSteam, 3295, 680, 1000, 200, true);
    //    }
    //}

    private void CreateOrUpdateWidgetWindow(Widget widget)
    {
        WidgetWindow? window;

        if (_widgetWindows.TryGetValue(widget.IdOrThrow, out var existingWindow))
        {
            bool enabled = widget.Enabled;

            existingWindow.Close();
            _widgetWindows.Remove(widget.IdOrThrow);

            if (!enabled) return;

            window = CreateWidgetWindow(widget);
        }
        else
        {
            if (!widget.Enabled)
            {
                return;
            }

            window = CreateWidgetWindow(widget);
        }

        window.Activate();
        window.SetDraggable(_isDraggable);
    }

    private WidgetWindow CreateWidgetWindow(Widget widget)
    {
        var widgetWindow = new WidgetWindow(this, _gridService, widget);

        _widgetWindows[widget.IdOrThrow] = widgetWindow;
        return widgetWindow;
    }

    public void SetDraggable(bool draggable)
    {
        _isDraggable = draggable;
        foreach (var window in _widgetWindows.Values)
        {
            window.SetDraggable(_isDraggable);
        }
    }

    public void SaveWidget(Widget widget, bool rerender)
    {
        var widgetId = widget.IdOrThrow;
        var isNewWidget = !_widgetConfigs.ContainsKey(widgetId);
        
        _widgetConfigs[widgetId] = widget;

        if (rerender)
        {
            Debug.WriteLine($"Rerendering {widget.Name}");
            CreateOrUpdateWidgetWindow(widget);
        }

        Debug.WriteLine($"Saving {widget.Name}");
        _widgetFileSystemService.SaveWidget(widget);
    }

    public IReadOnlyCollection<Widget> GetWidgets() => _widgetConfigs.Values.ToList().AsReadOnly();

    public void CloseAllWidgets()
    {
        _gridService.DestroyOverlay();
        
        foreach (var window in _widgetWindows.Values)
        {
            window.Close();
        }
        _widgetWindows.Clear();
    }
}
