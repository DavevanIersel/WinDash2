using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinDash2.Models;
using WinDash2.Services;
using WinRT.Interop;
using Microsoft.UI;

namespace WinDash2.Views;

public sealed partial class SettingsWindow : Window
{
    private readonly SettingsService _settingsService;

    public SettingsWindow(SettingsService settingsService)
    {
        InitializeComponent();
        _settingsService = settingsService;

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
    }

    private void SetWindowSize()
    {
        var hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        
        appWindow.Resize(new SizeInt32(600, 400));
    }

    private void OnDragModeChanged(object sender, RoutedEventArgs e)
    {
        var dragMode = FreeDragRadioButton.IsChecked == true 
            ? DragMode.Free 
            : DragMode.GridBased;

        _settingsService.UpdateDragMode(dragMode);
    }
}
