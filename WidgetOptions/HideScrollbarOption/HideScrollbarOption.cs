using Microsoft.UI.Xaml.Controls;
using System;
using WinDash2.Models;
using WinDash2.Utils;

namespace WinDash2.WidgetOptions.HideScrollbarOption;

public class HideScrollbarOption : IWidgetOption
{
    public void Apply(Widget widget, WebView2 webView)
    {
        webView.CoreWebView2.DOMContentLoaded += async (sender, args) =>
        {
            var script = await JavaScriptSrcLoader.GetSrcFileContentsByResourceNameAsync("WinDash2.WidgetOptions.HideScrollbarOption.hide-scrollbar.js");
            await webView.CoreWebView2.ExecuteScriptAsync(script);
        };
    }
}