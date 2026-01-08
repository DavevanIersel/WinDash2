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
        _ = SetTouchEmulationAsync(webView.CoreWebView2, widget.TouchEnabled ?? false);
    }

    private static async Task SetTouchEmulationAsync(CoreWebView2 coreWebView2, bool enabled)
    {
        try
        {
            var emitTouchValue = enabled
                ? @"{ ""enabled"": true, ""configuration"": ""mobile"" }"
                : @"{ ""enabled"": false }";
            
            await coreWebView2.CallDevToolsProtocolMethodAsync(
                "Emulation.setEmitTouchEventsForMouse",
                emitTouchValue
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to set touch emulation: {ex}");
        }
    }
}