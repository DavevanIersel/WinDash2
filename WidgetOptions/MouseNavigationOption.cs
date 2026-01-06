using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinDash2.Models;

namespace WinDash2.WidgetOptions;

public class MouseNavigationOption : IWidgetOption
{
    public void Apply(Widget widget, WebView2 webView, Window? window)
    {
        webView.PointerPressed += WebView_PointerPressed;
    }

    private void WebView_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not WebView2 webView) return;

        var pointer = e.GetCurrentPoint(webView);
        if (pointer.Properties.IsXButton1Pressed)
        {
            if (webView.CoreWebView2.CanGoBack)
                webView.CoreWebView2.GoBack();
            e.Handled = true;
        }
        else if (pointer.Properties.IsXButton2Pressed)
        {
            if (webView.CoreWebView2.CanGoForward)
                webView.CoreWebView2.GoForward();
            e.Handled = true;
        }
    }
}