using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml.Controls;

namespace WinDash2.Extensions;

public static class FrameExtensions
{
    public static void NavigateDI<TPage>(this Frame frame, IHost host)
        where TPage : Page
    {
        var page = host.Services.GetRequiredService<TPage>();
        frame.Content = page;
    }
}
