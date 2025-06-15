using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using WinDash2.Models;
using Windows.UI.WebUI;
using WinRT.Interop;

namespace WinDash2.Views;

public sealed partial class WidgetWindow : Window
{
    private AppWindow appWindow;
    private IntPtr hWnd;

    private readonly string _url;


    public WidgetWindow(Widget widget)
    {
        this.InitializeComponent();
        _url = widget.Url;

        hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        appWindow = AppWindow.GetFromWindowId(windowId);

        appWindow.MoveAndResize(new Windows.Graphics.RectInt32(widget.X, widget.Y, widget.Width, widget.Height));

        appWindow.Changed += AppWindow_Changed;

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
        MyWebView.CoreWebView2.Settings.IsWebMessageEnabled = true;
        MyWebView.CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;


        // Custom user agent
        MyWebView.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Android 15; Mobile; rv:133.0) Gecko/133.0 Firefox/133.0";

        // Enable touch emulation
        //await MyWebView.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setEmitTouchEventsForMouse", @"{
        //        ""enabled"": true,
        //        ""configuration"": ""mobile""
        //    }");

        // Enable devtools
        //MyWebView.CoreWebView2.OpenDevToolsWindow();

        MyWebView.CoreWebView2.Navigate(_url);
    }
    private void CoreWebView2_PermissionRequested(
    Microsoft.Web.WebView2.Core.CoreWebView2 sender,
    Microsoft.Web.WebView2.Core.CoreWebView2PermissionRequestedEventArgs args)
    {
        if (args.PermissionKind == Microsoft.Web.WebView2.Core.CoreWebView2PermissionKind.Microphone ||
            args.PermissionKind == Microsoft.Web.WebView2.Core.CoreWebView2PermissionKind.Camera)
        {
            args.State = Microsoft.Web.WebView2.Core.CoreWebView2PermissionState.Allow;
        }
        else
        {
            args.State = Microsoft.Web.WebView2.Core.CoreWebView2PermissionState.Default;
        }

        args.Handled = true;
    }

    private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (args.DidPositionChange || args.DidSizeChange)
        {
            var rect = sender.Size;
            var pos = sender.Position;
            Debug.WriteLine($"Window moved/resized: X={pos.X}, Y={pos.Y}, W={rect.Width}, H={rect.Height}");
        }
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
}
