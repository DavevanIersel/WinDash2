using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WinDash2.Core;
using WinDash2.Models;
using WinDash2.Services;
using WinDash2.Utils;
using WinDash2.WidgetOptions;
using WinDash2.WidgetOptions.FunctionKeyActions;
using WinDash2.WidgetOptions.HideScrollbarOption;
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
    private readonly GridService _gridService;
    private bool _isRerendering = false;

    private const int WM_EXITSIZEMOVE = 0x0232;
    private const int WM_ENTERSIZEMOVE = 0x0231;
    private const int WM_MOVING = 0x0216;
    private const int GWLP_WNDPROC = -4;

    private readonly IWidgetOption[] options =
    [
        new FunctionKeyActions(),
        new UserAgentOption(),
        new TouchOption(),
        new ForceInCurrentTabOption(),
        new HideScrollbarOption(),
    ];

    public WidgetWindow(WidgetManager widgetManager, GridService gridService, Widget widget)
    {
        InitializeComponent();

        _widget = widget;
        _widgetManager = widgetManager;
        _gridService = gridService;

        var hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        appWindow = AppWindow.GetFromWindowId(windowId);
        newWndProc = WndProc;
        oldWndProcPtr = SetWindowLongPtr(hWnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(newWndProc));

        _ = InitializeWindow();
    }

    private async Task InitializeWindow()
    {
        SetupTitleBar();
        SetDraggable(false);

        // Set window title to widget name
        Title = _widget.Name;
        WidgetTitle.Text = _widget.Name;

        appWindow.MoveAndResize(new Windows.Graphics.RectInt32(_widget.X, _widget.Y, _widget.Width, _widget.Height));
        appWindow.IsShownInSwitchers = false;

        await WidgetWebView.EnsureCoreWebView2Async();

        foreach (var option in options)
        {
            option.Apply(_widget, WidgetWebView, this);
        }

        this.Closed += OnWindowClosed;

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

        // Hide the titlebar icon
        appWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
    }

    public void SetDraggable(bool showFrame)
    {
        if (appWindow.Presenter is not OverlappedPresenter presenter) return;

        presenter.IsResizable = showFrame;
        presenter.SetBorderAndTitleBar(true, showFrame);

        // Show/hide custom drag bar overlay
        DragBar.Visibility = showFrame ? Visibility.Visible : Visibility.Collapsed;

        if (showFrame && DragBar.XamlRoot != null)
        {
            // Set the entire drag bar as draggable area, except the close button
            var scale = DragBar.XamlRoot.RasterizationScale;
            var dragBarRect = new Windows.Graphics.RectInt32(
                0, 0,
                (int)(appWindow.Size.Width),
                (int)(32 * scale)
            );
            appWindow.TitleBar.SetDragRectangles([dragBarRect]);
        }
        else
        {
            SetTitleBar(null);
            appWindow.TitleBar.SetDragRectangles([]);
        }
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        // Only disable widget if this is a user-initiated close, not a rerender
        if (!_isRerendering)
        {
            _widget.Enabled = false;
            _widgetManager.SaveWidget(_widget, false);
        }
    }

    public void CloseForRerender()
    {
        _isRerendering = true;
        this.Close();
    }

    private IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_ENTERSIZEMOVE)
        {
            OnMoveResizeStarted();
        }
        else if (msg == WM_MOVING)
        {
            OnMoving();
        }
        else if (msg == WM_EXITSIZEMOVE)
        {
            OnMoveResizeFinished();
        }

        return CallWindowProc(oldWndProcPtr, hwnd, msg, wParam, lParam);
    }

    private void OnMoveResizeStarted()
    {
        _gridService.OnMoveResizeStarted();
    }

    private void OnMoving()
    {
        _gridService.UpdateOverlayPosition();
    }

    private void OnMoveResizeFinished()
    {
        _gridService.OnMoveResizeFinished();

        var rect = appWindow.Size;
        var pos = appWindow.Position;

        var (finalX, finalY, finalWidth, finalHeight) = _gridService.SnapWindowBounds(
            pos.X, pos.Y, rect.Width, rect.Height);

        if (finalX != pos.X || finalY != pos.Y || finalWidth != rect.Width || finalHeight != rect.Height)
        {
            appWindow.MoveAndResize(new Windows.Graphics.RectInt32(finalX, finalY, finalWidth, finalHeight));
        }

        _widget.X = finalX;
        _widget.Y = finalY;
        _widget.Width = finalWidth;
        _widget.Height = finalHeight;

        _widgetManager.SaveWidget(_widget, false);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newProc);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
}
