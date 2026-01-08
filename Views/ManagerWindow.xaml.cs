using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinDash2.Core;
using WinRT.Interop;
using Microsoft.UI;

namespace WinDash2.Views;

public sealed partial class ManagerWindow : Window
{
    private readonly WidgetManager _widgetManager;

    public ManagerWindow(WidgetManager widgetManager)
    {
        this.InitializeComponent();
        _widgetManager = widgetManager;

        // Set the window icon
        var hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.SetIcon("Assets/logo96x96.ico");

        MainFrame.Navigate(typeof(WidgetLibraryPage), _widgetManager);
    }
}
