using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using WinDash2.Models;
using WinDash2.Views;

namespace WinDash2.WidgetOptions;

public class HideScrollbarOption : IWidgetOption
{
    public void Apply(Widget widget, WebView2 webView)
    {
        webView.CoreWebView2.DOMContentLoaded += (sender, args) =>
        {
            string script = @"
                (function() {
                    var style = document.createElement('style');
                    style.innerHTML = `
                        ::-webkit-scrollbar { width: 0 !important; height: 0 !important; }
                    `;
                    document.head.appendChild(style);
                })();
            ";
            _ = webView.CoreWebView2.ExecuteScriptAsync(script);
        };
    }
}