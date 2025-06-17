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
using WinDash2.WidgetOptions;
using WinRT.Interop;

namespace WinDash2.Views;

public sealed partial class WidgetWindow : Window
{
    private readonly AppWindow appWindow;
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    private readonly IntPtr oldWndProcPtr;
    private readonly WndProcDelegate newWndProc;

    private readonly Widget _widget;
    private readonly WidgetManager _widgetManager;

    private const int WM_EXITSIZEMOVE = 0x0232;
    private const int GWLP_WNDPROC = -4;

    private readonly IWidgetOption[] options =
    [
        new UserAgentOption(),
        new PermissionsOption(),
    ];

    public WidgetWindow(WidgetManager widgetManager, Widget widget)
    {
        InitializeComponent();

        _widget = widget;
        _widgetManager = widgetManager;

        var hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        appWindow = AppWindow.GetFromWindowId(windowId);
        newWndProc = WndProc;
        oldWndProcPtr = SetWindowLongPtr(hWnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(newWndProc));

        InitializeWindow();
    }

    private async void InitializeWindow()
    {
        SetupTitleBar();
        SetDraggable(false);

        appWindow.MoveAndResize(new Windows.Graphics.RectInt32(_widget.X, _widget.Y, _widget.Width, _widget.Height));
        appWindow.IsShownInSwitchers = false;

        await WidgetWebView.EnsureCoreWebView2Async();

        foreach (var option in options)
        {
            option.Apply(_widget, WidgetWebView.CoreWebView2);
        }

        WidgetWebView.CoreWebView2.Navigate(_widget.Url);
    }

    private void SetupTitleBar()
    {
        ExtendsContentIntoTitleBar = true;

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
        }
    }

    public void SetDraggable(bool showFrame)
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

    private IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_EXITSIZEMOVE)
        {
            OnMoveResizeFinished();
        }

        return CallWindowProc(oldWndProcPtr, hwnd, msg, wParam, lParam);
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

    // P/Invoke declarations for subclassing

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newProc);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
}
