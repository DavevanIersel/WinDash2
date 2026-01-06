using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using WinDash2.Core;
using WinDash2.Models;
using WinDash2.WidgetOptions;
using WinDash2.WidgetOptions.FunctionKeyActions;
using WinDash2.WidgetOptions.HideScrollbarOption;
using Windows.Foundation;
using Windows.UI.WebUI;

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

    private static readonly IWidgetOption[] Options =
    [
        new FunctionKeyActions(),
        new UserAgentOption(),
        new PermissionsOption(),
        new TouchOption(),
        new ForceInCurrentTabOption(),
        new HideScrollbarOption(),
        new MouseNavigationOption(),
    ];
    private TypedEventHandler<CoreWebView2, CoreWebView2NavigationCompletedEventArgs>? _navigationCompletedHandler;

    public WidgetEditPage()
    {
        this.InitializeComponent();
        this.Loaded += OnPageLoad;
    }

    private async void OnPageLoad(object sender, RoutedEventArgs e)
    {
        await PreviewWebView.EnsureCoreWebView2Async();
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_navigationCompletedHandler != null)
        {
            PreviewWebView.CoreWebView2.NavigationCompleted -= _navigationCompletedHandler;
        }

        _navigationCompletedHandler = (s, e) =>
        {
            foreach (var option in Options)
            {
                option.Apply(Widget, PreviewWebView);
            }
        };

        PreviewWebView.CoreWebView2.NavigationCompleted += _navigationCompletedHandler;

        PreviewWebView.Width = Widget.Width;
        PreviewWebView.Height = Widget.Height;
        PreviewWebView.CoreWebView2.Navigate(Widget.Url);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is WidgetEditArgs args)
        {
            Widget = args.Widget;
            _widgetManager = args.WidgetManager;

            Widget.PropertyChanged += Widget_PropertyChanged;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_widgetManager == null) return;

        _widgetManager.SaveWidget(Widget, true);
        Frame.GoBack();
    }
    private void Widget_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Debug.WriteLine("Changed");
        UpdatePreview();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Frame.GoBack();
    }
}
