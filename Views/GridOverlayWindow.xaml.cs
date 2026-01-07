using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Runtime.InteropServices;
using WinDash2.Services;
using Windows.Graphics;
using WinRT.Interop;

namespace WinDash2.Views;

public sealed partial class GridOverlayWindow : Window
{
    private readonly AppWindow _appWindow;
    private readonly IntPtr _hWnd;
    private readonly GridService _gridService;
    
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int GWL_EXSTYLE = -20;
    
    private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;

    public GridOverlayWindow(GridService gridService)
    {
        InitializeComponent();
        
        _gridService = gridService;
        _hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(_hWnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);
        
        SetupWindow();
    }

    private void SetupWindow()
    {
        ExtendsContentIntoTitleBar = true;
        SystemBackdrop = null;
        
        if (_appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMinimizable = false;
            presenter.IsMaximizable = false;
            presenter.SetBorderAndTitleBar(false, false);
        }

        _appWindow.IsShownInSwitchers = false;

        var exStyle = GetWindowLong(_hWnd, GWL_EXSTYLE);
        exStyle |= WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_LAYERED;
        SetWindowLong(_hWnd, GWL_EXSTYLE, exStyle);
        
        SetWindowPos(_hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    public void ShowOnMonitorWithMouse()
    {
        POINT cursorPos;
        GetCursorPos(out cursorPos);
        
        var displayAreas = DisplayArea.FindAll();
        DisplayArea targetDisplay = DisplayArea.Primary;
        
        for (int i = 0; i < displayAreas.Count; i++)
        {
            var display = displayAreas[i];
            if (cursorPos.X >= display.WorkArea.X && 
                cursorPos.X < display.WorkArea.X + display.WorkArea.Width &&
                cursorPos.Y >= display.WorkArea.Y && 
                cursorPos.Y < display.WorkArea.Y + display.WorkArea.Height)
            {
                targetDisplay = display;
                break;
            }
        }

        _appWindow.MoveAndResize(new RectInt32(
            targetDisplay.WorkArea.X,
            targetDisplay.WorkArea.Y,
            targetDisplay.WorkArea.Width,
            targetDisplay.WorkArea.Height
        ));

        DrawGrid(targetDisplay.WorkArea.Width, targetDisplay.WorkArea.Height);
        _appWindow.Show();
        
        SetWindowPos(_hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    public void HideOverlay()
    {
        _appWindow.Hide();
    }

    private void DrawGrid(int screenWidth, int screenHeight)
    {
        GridLinesCanvas.Children.Clear();

        int cellSize = _gridService.CellSize;
        var strokeBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(60, 255, 255, 255));
        
        for (int x = 0; x <= screenWidth; x += cellSize)
        {
            var line = new Line
            {
                X1 = x,
                Y1 = 0,
                X2 = x,
                Y2 = screenHeight,
                Stroke = strokeBrush,
                StrokeThickness = 1
            };
            GridLinesCanvas.Children.Add(line);
        }

        for (int y = 0; y <= screenHeight; y += cellSize)
        {
            var line = new Line
            {
                X1 = 0,
                Y1 = y,
                X2 = screenWidth,
                Y2 = y,
                Stroke = strokeBrush,
                StrokeThickness = 1
            };
            GridLinesCanvas.Children.Add(line);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}
