using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Linq;
using WinDash2.Models;

namespace WinDash2.WidgetOptions;

public class UserAgentOption : IWidgetOption
{
    public void Apply(Widget widget, WebView2 webView, Window? window)
    {

        if (widget.CustomUserAgent == null || widget.CustomUserAgent.Count == 0)
        {
            return;
        }

        webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);

        webView.CoreWebView2.WebResourceRequested += (sender, args) =>
        {
            try
            {
                var uri = new Uri(args.Request.Uri);
                var matchedUa = widget.CustomUserAgent
                    .FirstOrDefault(ua => uri.Host.Contains(ua.Domain, StringComparison.OrdinalIgnoreCase));

                if (matchedUa != null)
                {
                    args.Request.Headers.SetHeader("User-Agent", matchedUa.UserAgent);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception occurred: {ex.Message}");
            }
        };
    }
}