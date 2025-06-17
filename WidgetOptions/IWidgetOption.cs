using Microsoft.Web.WebView2.Core;
using WinDash2.Models;

namespace WinDash2.WidgetOptions;

public interface IWidgetOption
{
    void Apply(Widget widget, CoreWebView2 coreWebView2);
}