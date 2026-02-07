using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using WinDash2.Models;

namespace WinDash2.Utils;

public static class CustomWidgetLoader
{
    public static async Task LoadAsync(Widget widget, WebView2 webView)
    {
        if (string.IsNullOrWhiteSpace(widget.Html))
        {
            if (!string.IsNullOrWhiteSpace(widget.Url))
            {
                webView.CoreWebView2.Navigate(widget.Url);
            }
            return;
        }

        var htmlPath = GetHtmlFilePath(widget.Html, widget.FileName);

        if (!File.Exists(htmlPath))
        {
            throw new FileNotFoundException($"HTML file not found: {htmlPath}", htmlPath);
        }

        var htmlContent = await File.ReadAllTextAsync(htmlPath);
        webView.CoreWebView2.NavigateToString(htmlContent);

        // Set virtual host name mapping to allow loading of local resources
        var resourceFolder = Path.GetDirectoryName(htmlPath);
        webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "widget.local",
            resourceFolder,
            CoreWebView2HostResourceAccessKind.Allow);
    }

    private static string GetHtmlFilePath(string htmlFileName, string? widgetFileName)
    {
        var widgetsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "windash2", "dev", "widgets");

        // Get the widget's folder name (without .widget.json extension)
        var widgetFolderName = Path.GetFileNameWithoutExtension(widgetFileName ?? "custom");
        if (widgetFolderName.EndsWith(Widget.FileExtension, StringComparison.OrdinalIgnoreCase))
        {
            widgetFolderName = Path.GetFileNameWithoutExtension(widgetFolderName);
        }

        // Construct the full path to the HTML file
        if (Path.IsPathRooted(htmlFileName))
        {
            // Absolute path - use as-is
            return htmlFileName;
        }
        else
        {
            // Relative path - resolve relative to the widget's folder
            // Remove leading ./ or .\ if present
            var cleanPath = htmlFileName.TrimStart('.', '/', '\\');
            return Path.Combine(widgetsPath, widgetFolderName, cleanPath);
        }
    }
}
