using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using WinDash2.Models;

namespace WinDash2.WidgetOptions;

public class ForceInCurrentTabOption : IWidgetOption
{
    public void Apply(Widget widget, WebView2 webView)
    {
        var forceList = widget.ForceInCurrentTab;
        if (forceList == null || forceList.Count == 0)
            return;

        // Convert each pattern to a regex:
        // - Escape all regex special characters except '*'
        // - Replace '*' with '.*' to match any sequence of characters
        // - Add '^' at the start and '$' at the end to match the entire URL
        //   Example: "/foo/*/bar" becomes "^/foo/.*/bar$"
        var patterns = forceList
            .Select(p => "^" + Regex.Escape(p).Replace("\\*", ".*") + "$")
            .ToList();

        webView.CoreWebView2.NewWindowRequested += (sender, args) =>
        {
            if (patterns.Any(pattern => args.Uri != null && Regex.IsMatch(args.Uri, pattern, RegexOptions.IgnoreCase)))
            {
                args.Handled = true;
                sender.Navigate(args.Uri);
            }
        };
    }
}