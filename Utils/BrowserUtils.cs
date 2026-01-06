using System;
using System.Diagnostics;
using WinDash2.Models;

namespace WinDash2.Utils;

public static class BrowserUtils
{
    public static void OpenInDefaultBrowser(Widget widget)
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
