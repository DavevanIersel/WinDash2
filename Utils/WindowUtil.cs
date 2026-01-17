using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;

namespace WinDash2.Utils;

public static class WindowUtil
{
    /// <summary>
    /// Gets the HWND for the "active window" from a service that exposes a window property.
    /// Returns null if the service is not registered or the window is null.
    /// 
    /// Note: IntPtr.Zero is the equivalent of a null pointer for window handles.
    /// Passing IntPtr.Zero to APIs like FolderPicker.InitializeWithWindow
    /// indicates "no parent window" and is safe.
    /// </summary>
    public static IntPtr GetActiveWindowHandle<TService>(Func<TService, Window?> getWindow) where TService : class
    {
        try
        {
            if (App.AppHost?.Services.GetService<TService>() is not { } service)
            {
                return IntPtr.Zero;
            }

            if (getWindow(service) is not { } window)
            {
                return IntPtr.Zero;
            }

            return WinRT.Interop.WindowNative.GetWindowHandle(window);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetActiveWindowHandle<{typeof(TService).Name}> failed: {ex.Message}");
            return IntPtr.Zero;
        }
    }

    public static void SetDraggable(AppWindow appWindow, bool showFrame, Action? setTitleBarNull = null)
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

                setTitleBarNull?.Invoke();
                appWindow.TitleBar.SetDragRectangles(Array.Empty<Windows.Graphics.RectInt32>());
            }
        }
    }
}