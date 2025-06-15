using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Linq;
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

        InitializeWindow(widget);
    }

    private async void InitializeWindow(Widget widget)
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
        var coreWebView2 = MyWebView.CoreWebView2;

        coreWebView2.Settings.IsWebMessageEnabled = true;
        coreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
        SetupUserAgent(widget, coreWebView2);

        coreWebView2.Navigate(_url);
    }

    private static void SetupUserAgent(Widget widget, CoreWebView2 coreWebView2)
    {
        if (widget.CustomUserAgent?.Count > 0)
        {
            // Register filter to intercept all requests
            coreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);

            coreWebView2.WebResourceRequested += (sender, args) =>
            {
                try
                {
                    var uri = new Uri(args.Request.Uri);
                    var host = uri.Host;

                    var matchedUa = widget.CustomUserAgent
                        .FirstOrDefault(ua => host.Contains(ua.Domain, StringComparison.OrdinalIgnoreCase));

                    if (matchedUa != null)
                    {
                        args.Request.Headers.SetHeader("User-Agent", matchedUa.UserAgent);
                    }
                }
                catch
                {
                    // Optional: log or ignore malformed URIs
                }
            };
        }
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
