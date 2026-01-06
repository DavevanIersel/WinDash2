using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace WinDash2.Utils;

public class FullscreenManagerService
{
    private readonly AppWindow _appWindow;
    private readonly OverlappedPresenter? _overlappedPresenter;
    
    private bool _isFullscreen;
    private Windows.Graphics.RectInt32 _previousBounds;

    public FullscreenManagerService(Window window)
    {
        var hWnd = WindowNative.GetWindowHandle(window);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);
        _overlappedPresenter = _appWindow.Presenter as OverlappedPresenter;
    }

    public void ToggleFullscreen()
    {
        if (!_isFullscreen)
        {
            EnterFullscreen();
        }
        else
        {
            ExitFullscreen();
        }
    }

    public void EnterFullscreen()
    {
        // Save current bounds
        _previousBounds = new Windows.Graphics.RectInt32
        {
            X = _appWindow.Position.X,
            Y = _appWindow.Position.Y,
            Width = _appWindow.Size.Width,
            Height = _appWindow.Size.Height
        };

        // Enter fullscreen
        _appWindow.SetPresenter(FullScreenPresenter.Create());
        _isFullscreen = true;
    }

    public void ExitFullscreen()
    {
        if (!_isFullscreen)
        {
            return;
        }

        // Restore overlapped presenter
        if (_overlappedPresenter != null)
        {
            _appWindow.SetPresenter(_overlappedPresenter);
        }
        else
        {
            _appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
            
            // Restore border settings if we had to create a new presenter
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.SetBorderAndTitleBar(true, false);
                presenter.IsResizable = false;
            }
        }

        // Restore previous bounds
        _appWindow.MoveAndResize(_previousBounds);
        _isFullscreen = false;
    }
}
