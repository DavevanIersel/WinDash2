using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WinDash2.Models;

namespace WinDash2.WidgetOptions;

public class TouchOption : IWidgetOption
{
    public void Apply(Widget widget, WebView2 webView, Window? window)
    {
        if (widget.TouchEnabled == true)
        {
            _ = EnableTouchAsync(webView.CoreWebView2);
        }
    }

    private static async Task EnableTouchAsync(CoreWebView2 coreWebView2)
    {
        try
        {
            await coreWebView2.CallDevToolsProtocolMethodAsync(
                "Emulation.setEmitTouchEventsForMouse",
                @"{ ""enabled"": true, ""configuration"": ""mobile"" }"
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to enable touch emulation: {ex}");
        }
    }
}