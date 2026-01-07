using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using WinDash2.Models;
using WinDash2.Views;

namespace WinDash2.Services;

public class GridService
{
    private const int WindowBorderOffset = 7; // Windows 11 border size
    
    private readonly SettingsService _settingsService;
    private GridOverlayWindow? _gridOverlayWindow;

    public GridService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public bool IsGridEnabled => _settingsService.GetSettings().DragMode == DragMode.GridBased;

    #region Grid Transform calculations
    
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
    
    /// <summary>
    /// Gets the monitor that contains the given point
    /// </summary>
    private static DisplayArea GetDisplayAreaForPoint(int x, int y)
    {
        var displayAreas = DisplayArea.FindAll();
        
        for (int i = 0; i < displayAreas.Count; i++)
        {
            var display = displayAreas[i];
            if (x >= display.WorkArea.X && 
                x < display.WorkArea.X + display.WorkArea.Width &&
                y >= display.WorkArea.Y && 
                y < display.WorkArea.Y + display.WorkArea.Height)
            {
                return display;
            }
        }
        
        return DisplayArea.Primary;
    }
    
    /// <summary>
    /// Gets the monitor where the mouse cursor currently is
    /// </summary>
    private static DisplayArea GetDisplayAreaForCursor()
    {
        POINT cursorPos;
        GetCursorPos(out cursorPos);
        return GetDisplayAreaForPoint(cursorPos.X, cursorPos.Y);
    }
    
    /// <summary>
    /// Snaps coordinates to grid, relative to the monitor's origin.
    /// Uses the cursor position to determine which monitor's grid to align to.
    /// </summary>
    public (int x, int y) SnapToGrid(int x, int y)
    {
        if (!IsGridEnabled)
        {
            return (x, y);
        }

        // Use the mouse cursor position to determine which monitor's grid to snap to
        // This ensures alignment with the visible grid overlay
        var display = GetDisplayAreaForCursor();
        int monitorOriginX = display.WorkArea.X;
        int monitorOriginY = display.WorkArea.Y;

        // Calculate position relative to monitor origin
        int relativeX = x - monitorOriginX;
        int relativeY = y - monitorOriginY;

        // Snap to grid relative to monitor
        int cellSize = _settingsService.GetSettings().GridSize;
        int snappedRelativeX = (int)Math.Round((double)relativeX / cellSize) * cellSize;
        int snappedRelativeY = (int)Math.Round((double)relativeY / cellSize) * cellSize;

        // Convert back to global coordinates
        int snappedX = snappedRelativeX + monitorOriginX;
        int snappedY = snappedRelativeY + monitorOriginY;

        return (snappedX, snappedY);
    }

    public (int width, int height) SnapSizeToGrid(int width, int height)
    {
        if (!IsGridEnabled)
        {
            return (width, height);
        }

        int cellSize = _settingsService.GetSettings().GridSize;
        int snappedWidth = Math.Max(cellSize, (int)Math.Round((double)width / cellSize) * cellSize);
        int snappedHeight = Math.Max(cellSize, (int)Math.Round((double)height / cellSize) * cellSize);

        return (snappedWidth, snappedHeight);
    }

    /// <summary>
    /// Snaps a window's bounds (position and size) to the grid, accounting for window borders.
    /// Returns the final bounds that should be applied to the window.
    /// </summary>
    public (int x, int y, int width, int height) SnapWindowBounds(int x, int y, int width, int height)
    {
        if (!IsGridEnabled)
        {
            return (x, y, width, height);
        }

        var (snappedX, snappedY) = SnapToGrid(x, y);
        var (snappedWidth, snappedHeight) = SnapSizeToGrid(width, height);

        // Adjust back to include the border
        int adjustedWidth = snappedWidth + WindowBorderOffset;
        int adjustedHeight = snappedHeight + WindowBorderOffset;

        Debug.WriteLine($"Final window bounds with border: x={snappedX}, y={snappedY}, width={adjustedWidth}, height={adjustedHeight}");

        return (snappedX, snappedY, adjustedWidth, adjustedHeight);
    }
    #endregion

    #region Grid Overlay
    public void OnMoveResizeStarted()
    {
        if (!IsGridEnabled) return;
        
        EnsureOverlayCreated();
        _gridOverlayWindow?.ShowOnMonitorWithMouse();
    }

    public void UpdateOverlayPosition()
    {
        if (!IsGridEnabled) return;
        
        _gridOverlayWindow?.UpdateMonitorIfNeeded();
    }

    public void OnMoveResizeFinished()
    {
        _gridOverlayWindow?.HideOverlay();
    }

    public void DestroyOverlay()
    {
        if (_gridOverlayWindow != null)
        {
            _gridOverlayWindow.Close();
            _gridOverlayWindow = null;
        }
    }

    private void EnsureOverlayCreated()
    {
        if (!IsGridEnabled) return;
        
        if (_gridOverlayWindow == null)
        {
            _gridOverlayWindow = new GridOverlayWindow(_settingsService);
            _gridOverlayWindow.Closed += (s, e) => _gridOverlayWindow = null;
        }
    }
    #endregion
}
