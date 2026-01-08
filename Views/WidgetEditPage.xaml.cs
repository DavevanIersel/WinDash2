using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
    public bool IsNewWidget { get; init; } = false;
}

public sealed partial class WidgetEditPage : Page
{
    private WidgetManager? _widgetManager;
    private bool _isNewWidget;
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
        // Check if CoreWebView2 is initialized
        if (PreviewWebView.CoreWebView2 == null)
        {
            return;
        }

        // Apply options before navigation
        foreach (var option in Options)
        {
            option.Apply(Widget, PreviewWebView, null);
        }

        PreviewWebView.Width = Widget.Width;
        PreviewWebView.Height = Widget.Height;
        
        // Only navigate if URL is not empty and is a valid URI
        if (!string.IsNullOrWhiteSpace(Widget.Url) && Uri.TryCreate(Widget.Url, UriKind.Absolute, out _))
        {
            PlaceholderPanel.Visibility = Visibility.Collapsed;
            PreviewWebView.Visibility = Visibility.Visible;
            PreviewWebView.CoreWebView2.Navigate(Widget.Url);
        }
        else
        {
            // Show native placeholder when URL is empty or invalid
            PreviewWebView.Visibility = Visibility.Collapsed;
            PlaceholderPanel.Visibility = Visibility.Visible;
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is WidgetEditArgs args)
        {
            Widget = args.Widget;
            _widgetManager = args.WidgetManager;
            _isNewWidget = args.IsNewWidget;

            Widget.PropertyChanged += Widget_PropertyChanged;
            TouchEnabledSwitch.IsOn = Widget.TouchEnabled.GetValueOrDefault();
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_widgetManager == null) return;

        if (_isNewWidget)
        {
            // Generate a filename based on widget name
            var safeName = string.Join("_", Widget.Name.Split(Path.GetInvalidFileNameChars()));
            if (string.IsNullOrWhiteSpace(safeName))
            {
                safeName = "widget";
            }
            Widget.FileName = $"{safeName.ToLower()}.widget.json";
            
            // Ensure the widget has an ID
            Widget.Id ??= Guid.NewGuid();
        }

        _widgetManager.SaveWidget(Widget, true);
        Frame.GoBack();
    }

    private void Widget_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Debug.WriteLine("Changed");
        UpdatePreview();
    }

    private void TouchEnabledSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            Widget.TouchEnabled = toggleSwitch.IsOn;
            UpdatePreview();
        }
    }

    private void AddUserAgent_Click(object sender, RoutedEventArgs e)
    {
        Widget.CustomUserAgent ??= [];
        Widget.CustomUserAgent.Add(new UserAgentMapping
        {
            Domain = "",
            UserAgent = ""
        });

        UpdateUserAgentsinEditor();
    }

    private void RemoveUserAgent_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is UserAgentMapping mapping)
        {
            Widget.CustomUserAgent?.Remove(mapping);
            
            UpdateUserAgentsinEditor();
        }
    }

    private void UpdateUserAgentsinEditor()
    {
        UserAgentsList.ItemsSource = null;
        UserAgentsList.ItemsSource = Widget.CustomUserAgent;
        UpdatePreview();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Frame.GoBack();
    }
}
