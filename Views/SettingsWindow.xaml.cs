using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using WinDash2.Models;
using WinDash2.Services;
using WinRT.Interop;
using Microsoft.UI;

namespace WinDash2.Views;

public sealed partial class SettingsWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly GridService _gridService;

    public SettingsWindow(SettingsService settingsService, GridService gridService)
    {
        InitializeComponent();
        _settingsService = settingsService;
        _gridService = gridService;

        SetWindowSize();

        var settings = _settingsService.GetSettings();
        
        if (settings.DragMode == DragMode.Free)
        {
            FreeDragRadioButton.IsChecked = true;
        }
        else
        {
            GridBasedRadioButton.IsChecked = true;
        }

        GridSizeNumberBox.Value = settings.GridSize;
    }

    private void SetWindowSize()
    {
        var hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        
        appWindow.Resize(new SizeInt32(800, 600));
    }

    private void OnDragModeChanged(object sender, RoutedEventArgs e)
    {
        var dragMode = FreeDragRadioButton.IsChecked == true 
            ? DragMode.Free 
            : DragMode.GridBased;

        _settingsService.UpdateDragMode(dragMode);
    }

    private void OnGridSizeChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!double.IsNaN(args.NewValue))
        {
            _settingsService.UpdateGridSize((int)args.NewValue);
        }
    }
}
