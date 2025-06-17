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
    private AppWindow appWindow;
    private IntPtr hWnd;

    private readonly Widget _widget;
    private readonly WidgetManager _widgetManager;

    // For subclassing window proc
    private IntPtr oldWndProcPtr;
    private WndProcDelegate newWndProc;

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private const int WM_EXITSIZEMOVE = 0x0232;

    private readonly IWidgetOption[] options = new IWidgetOption[]
    {
        new UserAgentOption(),
        new PermissionsOption(),
        // new TouchOption(),
        // new PermissionsOption(),
        // etc.
    };

    public WidgetWindow(WidgetManager widgetManager, Widget widget)
    {
        this.InitializeComponent();

        _widget = widget;
        _widgetManager = widgetManager;

        hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        appWindow = AppWindow.GetFromWindowId(windowId);

        appWindow.MoveAndResize(new Windows.Graphics.RectInt32(widget.X, widget.Y, widget.Width, widget.Height));

        appWindow.IsShownInSwitchers = false;
        newWndProc = WndProc;
        oldWndProcPtr = SetWindowLongPtr(hWnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(newWndProc));
        InitializeWindow();
    }

    private async void InitializeWindow()
    {
        SetupTitleBar();
        SetDraggable(false);

        await WidgetWebView.EnsureCoreWebView2Async();

        foreach (var option in options)
        {
            option.Apply(_widget, WidgetWebView.CoreWebView2);
        }
        //coreWebView2.Settings.IsWebMessageEnabled = true; // Enables JS to C# messaging with the widget, using coreWebView2.WebMessageReceived and coreWebView2.PostWebMessageAsString (Can probably be removed)

        WidgetWebView.CoreWebView2.Navigate(_widget.Url);
    }

    private void SetupTitleBar()
    {
        this.ExtendsContentIntoTitleBar = true;

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
        }
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

    // P/Invoke declarations for subclassing

    private const int GWLP_WNDPROC = -4;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newProc);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
}
