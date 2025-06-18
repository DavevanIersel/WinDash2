using Microsoft.UI.Xaml.Controls;
using WinDash2.Models;
using WinDash2.Views;

namespace WinDash2.WidgetOptions;

public interface IWidgetOption
{
    void Apply(Widget widget, WebView2 webView);
}