using Microsoft.UI.Xaml;
using WinDash2.Core;

namespace WinDash2.Views;

public sealed partial class ManagerWindow : Window
{
    private readonly WidgetManager _widgetManager;

    public ManagerWindow(WidgetManager widgetManager)
    {
        this.InitializeComponent();
        _widgetManager = widgetManager;

        MainFrame.Navigate(typeof(WidgetLibraryPage), _widgetManager);
    }
}
