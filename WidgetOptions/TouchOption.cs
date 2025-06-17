using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WinDash2.Models;

namespace WinDash2.WidgetOptions;

public class TouchOption : IWidgetOption
{
    public void Apply(Widget widget, CoreWebView2 coreWebView2)
    {
        if (widget.TouchEnabled == true)
        {
            _ = EnableTouchAsync(coreWebView2);
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