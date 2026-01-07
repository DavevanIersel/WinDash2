using System;
using WinDash2.Models;
using WinDash2.Views;

namespace WinDash2.Services;

public class GridService
{
    private const int DefaultCellSize = 50;
    private const int WindowBorderOffset = 7; // Windows 11 border size
    
    private readonly SettingsService _settingsService;
    private GridOverlayWindow? _gridOverlayWindow;
    private int _cellSize = DefaultCellSize;

    public GridService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public bool IsGridEnabled => _settingsService.GetSettings().DragMode == DragMode.GridBased;

    public int CellSize
    {
        get => _cellSize;
        set => _cellSize = value > 0 ? value : DefaultCellSize;
    }

    #region Grid Transform calculations
    public (int x, int y) SnapToGrid(int x, int y)
    {
        if (!IsGridEnabled)
        {
            return (x, y);
        }

        int snappedX = (int)Math.Round((double)x / _cellSize) * _cellSize;
        int snappedY = (int)Math.Round((double)y / _cellSize) * _cellSize;

        return (snappedX, snappedY);
    }

    public (int width, int height) SnapSizeToGrid(int width, int height)
    {
        if (!IsGridEnabled)
        {
            return (width, height);
        }

        int snappedWidth = Math.Max(_cellSize, (int)Math.Round((double)width / _cellSize) * _cellSize);
        int snappedHeight = Math.Max(_cellSize, (int)Math.Round((double)height / _cellSize) * _cellSize);

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

        // Account for window border when snapping
        var (snappedX, snappedY) = SnapToGrid(x + WindowBorderOffset, y + WindowBorderOffset);
        var (snappedWidth, snappedHeight) = SnapSizeToGrid(width - (WindowBorderOffset * 2), height - (WindowBorderOffset * 2));
        
        // Adjust back to include the border
        int finalX = snappedX + WindowBorderOffset;
        int finalY = snappedY - WindowBorderOffset;
        int finalWidth = snappedWidth + WindowBorderOffset * 2;
        int finalHeight = snappedHeight + WindowBorderOffset;

        return (finalX, finalY, finalWidth, finalHeight);
    }
    #endregion

    #region Grid Overlay
    public void OnMoveResizeStarted()
    {
        if (!IsGridEnabled) return;
        
        EnsureOverlayCreated();
        _gridOverlayWindow?.ShowOnMonitorWithMouse();
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
            _gridOverlayWindow = new GridOverlayWindow(this);
            _gridOverlayWindow.Closed += (s, e) => _gridOverlayWindow = null;
        }
    }
    #endregion
}
