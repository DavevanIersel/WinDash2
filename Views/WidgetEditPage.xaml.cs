using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WinDash2.Core;
using WinDash2.Models;

namespace WinDash2.Views;

public class WidgetEditArgs
{
    public required Widget Widget { get; init; }
    public required WidgetManager WidgetManager { get; init; }
}


public sealed partial class WidgetEditPage : Page
{
    private WidgetManager? _widgetManager;
    public Widget Widget { get; set; }

    public WidgetEditPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is WidgetEditArgs args)
        {
            Widget = args.Widget;
            _widgetManager = args.WidgetManager;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_widgetManager == null) return;

        _widgetManager.SaveWidget(Widget, true);
        Frame.GoBack();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Frame.GoBack();
    }
}
