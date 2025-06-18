using Microsoft.Web.WebView2.Core;
using WinDash2.Models;

namespace WinDash2.WidgetOptions;

public class HideScrollbarOption : IWidgetOption
{
    public void Apply(Widget widget, CoreWebView2 coreWebView2)
    {
        coreWebView2.DOMContentLoaded += (sender, args) =>
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
            _ = coreWebView2.ExecuteScriptAsync(script);
        };
    }
}