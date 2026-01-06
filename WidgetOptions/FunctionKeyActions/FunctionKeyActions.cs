using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Text.Json;
using WinDash2.Models;
using WinDash2.Utils;

namespace WinDash2.WidgetOptions.FunctionKeyActions;

public class FunctionKeyActions : IWidgetOption
{
    private FullscreenManagerService? _fullscreenManager;

    public void Apply(Widget widget, WebView2 webView, Window? window)
    {
        if (window != null)
        {
            _fullscreenManager = new FullscreenManagerService(window);
        }

        webView.CoreWebView2.DOMContentLoaded += async (sender, args) =>
        {
            var script = await JavaScriptSrcLoader.GetSrcFileContentsByResourceNameAsync(
                "WinDash2.WidgetOptions.FunctionKeyActions.keyboard-handler.js");

            await webView.CoreWebView2.ExecuteScriptAsync(script);
        };

        webView.CoreWebView2.WebMessageReceived += (sender, args) =>
        {
            try
            {
                var messageJson = args.TryGetWebMessageAsString();
                var keyboardEvent = JsonSerializer.Deserialize<KeyboardEventMessage>(messageJson);

                if (keyboardEvent?.Type == KeyboardEventType.Keydown)
                {
                    switch (keyboardEvent.Code)
                    {
                        case "F1":
                            BrowserUtils.OpenInDefaultBrowser(widget);
                            break;
                        case "F11":
                            _fullscreenManager?.ToggleFullscreen();
                            break;
                        case "Escape":
                            _fullscreenManager?.ExitFullscreen();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing web message: {ex.Message}");
            }
        };
    }
}
