using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Text.Json;
using WinDash2.Models;
using WinDash2.Utils;

namespace WinDash2.WidgetOptions.FunctionKeyActions;

public class FunctionKeyActions : IWidgetOption
{
    public void Apply(Widget widget, WebView2 webView)
    {
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

                if (keyboardEvent?.Type == KeyboardEventType.Keydown &&
                    keyboardEvent.Code == KeyCode.F1)
                {
                    OpenWidgetInPreferredBrowser(widget);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing web message: {ex.Message}");
            }
        };
    }

    private static void OpenWidgetInPreferredBrowser(Widget widget)
    {
        if (!Uri.TryCreate(widget.Url, UriKind.Absolute, out var uri))
        {
            Debug.WriteLine($"Invalid widget URL: {widget.Url}");
            return;
        }

        // Allow only web URLs
        if (uri.Scheme != Uri.UriSchemeHttp &&
            uri.Scheme != Uri.UriSchemeHttps)
        {
            Debug.WriteLine($"Blocked non-web URL scheme: {uri.Scheme}");
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = uri.AbsoluteUri,
            UseShellExecute = true
        });
    }
}
