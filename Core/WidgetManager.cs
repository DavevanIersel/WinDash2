using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WinDash2.Models;
using WinDash2.Services;
using WinDash2.Views;

namespace WinDash2.Core;

public class WidgetManager
{
    private readonly Dictionary<Guid, Widget> _widgetConfigs = [];
    private readonly Dictionary<Guid, WidgetWindow> _widgetWindows = [];
    private readonly WidgetFileSystemService _widgetFileSystemService;
    private Boolean _isDraggable = false;

    public WidgetManager(WidgetFileSystemService widgetFileSystemService)
    {
        _widgetFileSystemService = widgetFileSystemService;
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
        //move_externa_windows_experiment();
    }

    //private void move_externa_windows_experiment()
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
            window = existingWindow;
            if (!widget.Enabled)
            {
                existingWindow.Close();
                _widgetWindows.Remove(widget.IdOrThrow);
                return;
            }
            //existingWindow.UpdateWidget(widget);
        }
        else
        {
            if (!widget.Enabled)
            {
                return;
            }
            var widgetWindow = new WidgetWindow(this, widget);
            window = widgetWindow;

            _widgetWindows[widget.IdOrThrow] = widgetWindow;
        }

        window.Activate();
        window.SetDraggable(_isDraggable);
    }

    public void SetDraggable(bool draggable)
    {
        _isDraggable = draggable;
        foreach (var window in _widgetWindows.Values)
        {
            window.SetDraggable(_isDraggable);
        }
    }

    public async Task SaveWidgetAsync(Widget widget, bool rerender)
    {
        await _widgetFileSystemService.SaveWidgetAsync(widget);
        _widgetConfigs[widget.IdOrThrow] = widget;

        if (rerender)
        {
            CreateOrUpdateWidgetWindow(widget);
        }
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
