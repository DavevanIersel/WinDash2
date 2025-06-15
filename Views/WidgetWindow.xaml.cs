using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using WinDash2.Core;
using WinDash2.Models;
using WinRT.Interop;

namespace WinDash2.Views;

public sealed partial class WidgetWindow : Window
{
    private AppWindow appWindow;
    private IntPtr hWnd;

    private readonly Widget _widget;
    private readonly WidgetManager _widgetManager;

    // For subclassing window proc
    private IntPtr oldWndProcPtr;
    private WndProcDelegate newWndProc;

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private const int WM_ENTERSIZEMOVE = 0x0231;
    private const int WM_EXITSIZEMOVE = 0x0232;

    public WidgetWindow(WidgetManager widgetManager, Widget widget)
    {
        this.InitializeComponent();

        _widget = widget;
        _widgetManager = widgetManager;

        hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        appWindow = AppWindow.GetFromWindowId(windowId);

        appWindow.MoveAndResize(new Windows.Graphics.RectInt32(widget.X, widget.Y, widget.Width, widget.Height));

        // Subclass the window proc to intercept native messages
        newWndProc = WndProc;
        oldWndProcPtr = SetWindowLongPtr(hWnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(newWndProc));

        // Subscribe to Changed event if you want (optional)
        // appWindow.Changed += AppWindow_Changed;

        InitializeWindow();
    }

    private async void InitializeWindow()
    {
        this.ExtendsContentIntoTitleBar = true;
        DragToggle.Toggled += DragToggle_Toggled;
        SetFrame(false);

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
        }

        // Ensure WebView2 is ready
        await MyWebView.EnsureCoreWebView2Async();
        var coreWebView2 = MyWebView.CoreWebView2;

        coreWebView2.Settings.IsWebMessageEnabled = true;
        coreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
        SetupUserAgent(_widget, coreWebView2);

        coreWebView2.Navigate(_widget.Url);
    }

    private static void SetupUserAgent(Widget widget, CoreWebView2 coreWebView2)
    {
        if (widget.CustomUserAgent?.Count > 0)
        {
            coreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);

            coreWebView2.WebResourceRequested += (sender, args) =>
            {
                try
                {
                    var uri = new Uri(args.Request.Uri);
                    var host = uri.Host;

                    var matchedUa = widget.CustomUserAgent
                        .FirstOrDefault(ua => host.Contains(ua.Domain, StringComparison.OrdinalIgnoreCase));

                    if (matchedUa != null)
                    {
                        args.Request.Headers.SetHeader("User-Agent", matchedUa.UserAgent);
                    }
                }
                catch
                {
                    // Ignore malformed URIs
                }
            };
        }
    }

    private void CoreWebView2_PermissionRequested(CoreWebView2 sender, CoreWebView2PermissionRequestedEventArgs args)
    {
        if (args.PermissionKind == CoreWebView2PermissionKind.Microphone ||
            args.PermissionKind == CoreWebView2PermissionKind.Camera)
        {
            args.State = CoreWebView2PermissionState.Allow;
        }
        else
        {
            args.State = CoreWebView2PermissionState.Default;
        }

        args.Handled = true;
    }

    private async void OnMoveResizeFinished()
    {
        var rect = appWindow.Size;
        var pos = appWindow.Position;

        _widget.X = pos.X;
        _widget.Y = pos.Y;
        _widget.Width = rect.Width;
        _widget.Height = rect.Height;

        Debug.WriteLine($"Move/resize finished: X={pos.X}, Y={pos.Y}, W={rect.Width}, H={rect.Height}");

        await _widgetManager.SaveWidgetAsync(_widget, false);
    }

    private IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_EXITSIZEMOVE:
                OnMoveResizeFinished();
                break;
        }

        return CallWindowProc(oldWndProcPtr, hwnd, msg, wParam, lParam);
    }

    private void SetFrame(bool showFrame)
    {
        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            if (showFrame)
            {
                presenter.IsResizable = true;
                presenter.SetBorderAndTitleBar(true, true);
            }
            else
            {
                presenter.IsResizable = false;
                presenter.SetBorderAndTitleBar(true, false);

                SetTitleBar(null);
                appWindow.TitleBar.SetDragRectangles(Array.Empty<Windows.Graphics.RectInt32>());
            }
        }
    }

    private void DragToggle_Toggled(object sender, RoutedEventArgs e)
    {
        var toggle = sender as ToggleSwitch;
        SetFrame(toggle?.IsOn ?? false);
    }

    // P/Invoke declarations for subclassing

    private const int GWLP_WNDPROC = -4;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newProc);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
}
