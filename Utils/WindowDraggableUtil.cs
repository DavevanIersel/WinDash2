using Microsoft.UI.Windowing;
using System;

namespace WinDash2.Utils;

public static class WindowDraggableUtil
{
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